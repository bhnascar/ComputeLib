using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

// Solid voxelizer using GPU rasterization pipeline and # of
// surface crossings to determine interior voxels. Expects
// watertight meshes.
public class Voxelizer
{
  private float particleDiameter;
  private bool writeDebugTexture;
  private Material material;
  private List<Vector3> particles;

  public Voxelizer(float particleDiameter, bool writeDebugTexture = true) {
    this.particleDiameter = particleDiameter;
    this.writeDebugTexture = writeDebugTexture;
    this.material = Resources.Load<Material>("VoxelizerMaterial");
  }

  public void Build(Mesh mesh, Camera camera) {
    CommandBuffer buffer = new CommandBuffer();
    buffer.name = "Voxelizer";

    float maxDimension = Mathf.Max(
        mesh.bounds.size.x,
        Mathf.Max(mesh.bounds.size.y, mesh.bounds.size.z)
    );
    Bounds gridBounds = new Bounds(
        mesh.bounds.min + 0.5f * maxDimension * Vector3.one,
        maxDimension * Vector3.one
    );

    // Allocate 3D bit grid, represented as a 2D texture with bit depth being the
    // third dimension (width x height x bit depth). Using ARGBInt gives 32 bit 
    // ints per component, for a max grid size of 128 bits.
    int gridSize = (int)Mathf.Min(maxDimension / particleDiameter, 128);
    RenderTexture grid = RenderTexture.GetTemporary(
        gridSize, gridSize, 0, RenderTextureFormat.ARGBInt);
    grid.Create();
    Debug.Log("Grid dimensions: " + gridSize);

    // Zero-initialize grid.
    buffer.SetRenderTarget(grid);
    buffer.ClearRenderTarget(false, true, Color.clear, 0);

    // Position camera to look at mesh.
    Vector3 from = new Vector3(gridBounds.center.x, gridBounds.center.y, gridBounds.min.z - 1);
    Vector3 to = gridBounds.center;
    // Matrix4x4 viewMatrix = Matrix4x4.LookAt(from, to, Vector3.up);
    // camera.worldToCameraMatrix = viewMatrix;
    camera.transform.position = from;
    camera.transform.LookAt(to);

    // Build orthographic projection matrix with the viewing volume being
    // the mesh bounds (in camera space).
    // Vector3 minCorner = viewMatrix * gridBounds.min;
    // Vector3 maxCorner = viewmatrix * gridBounds.max;
    Vector3 minCorner = camera.worldToCameraMatrix.MultiplyPoint(gridBounds.min);
    Vector3 maxCorner = camera.worldToCameraMatrix.MultiplyPoint(gridBounds.max);
    float left = minCorner.x;
    float right = maxCorner.x;
    float bottom = minCorner.y;
    float top = maxCorner.y;
    float near = minCorner.z;
    float far = maxCorner.z;
    Matrix4x4 projectionMatrix = Matrix4x4.Ortho(left, right, bottom, top, near, far);
    projectionMatrix.SetColumn(2, projectionMatrix.GetColumn(2) * -1);
    Debug.Log(string.Format("Projected min {0} to {1}", minCorner, projectionMatrix.MultiplyPoint(minCorner)));
    Debug.Log(string.Format("Projected max {0} to {1}", maxCorner, projectionMatrix.MultiplyPoint(maxCorner)));
    camera.projectionMatrix = projectionMatrix;

    // Compute interior voxels in the fragment shader. Takes advantage of the rasterization 
    // pipeline to fill voxels that are wholly in triangle interiors.
    buffer.SetGlobalInt("grid_dimension", gridSize);
    buffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
    buffer.DrawMesh(mesh, Matrix4x4.identity, this.material);
    Graphics.ExecuteCommandBuffer(buffer);

    // if (writeDebugTexture) {
    //   WriteDebugTexture(grid);
    // }

    particles = ToParticles(grid, gridBounds);

    // Cleanup.
    RenderTexture.ReleaseTemporary(grid);
  }

  public void WriteDebugTexture(RenderTexture texture) {
    // Capture debug texture.
    RenderTexture.active = texture;
    Texture2D debugTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, false);
    debugTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
    debugTexture.Apply();
    
    // Write to PNG file.
    byte[] bytes = ImageConversion.EncodeToPNG(debugTexture);
    File.WriteAllBytes(Application.dataPath + "/Voxelizer/Debug.png", bytes);
    Debug.Log("Wrote debug image to: " + Application.dataPath + "/Voxelizer/Debug.png");

    Object.Destroy(debugTexture);
  }

  public unsafe List<Vector3> ToParticles(RenderTexture texture, Bounds gridBounds) {
    // Read pixels from texture.
    RenderTexture.active = texture;
    Texture2D debugTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, false);
    debugTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
    debugTexture.Apply();
    Color[] pixels = debugTexture.GetPixels(0);
    Object.Destroy(debugTexture);

    // Convert active bits in color channels to particle position.
    List<Vector3> particles = new List<Vector3>();
    for (int y = 0; y < texture.height; y++) {
      for (int x = 0; x < texture.width; x++) {
        Color pixel = pixels[y * texture.width + x];
        for (int z = 0; z < texture.width; z++) {
          int mask = 1 << (z % 32);
          float component = pixel[z / 32];
          int component_as_int = *((int*)&component); // Requires unsafe qualifier
          if ((component_as_int & mask) != 0) {
            Vector3 voxelCenterOffset = new Vector3(
              0.5f * particleDiameter,
              0.5f * particleDiameter,
              0 // Z-sampling in fragment shader is shifted by +0.5f
            );
            particles.Add(gridBounds.min + particleDiameter * new Vector3(x, y, z) + voxelCenterOffset);
          }
        }
      }
    }
    Debug.Log("Found " + particles.Count + " particles!");
    return particles;
  }

  public void Draw() {
    if (particles == null) {
      return;
    }
    float particleRadius = particleDiameter / 2;
    foreach (var particle in particles) {
      Gizmos.DrawSphere(particle, particleRadius);
    }
  }
}
