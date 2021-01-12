using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VoxelizerLoader : MonoBehaviour {
  public float particleDiameter = 0.1f;
  private Voxelizer voxelizer;

  void Start() {
    voxelizer = new Voxelizer(particleDiameter);
    var mesh = GetComponent<MeshFilter>().mesh;
    voxelizer.Build(mesh);
  }

  void OnDrawGizmos() {
    if (Application.isPlaying) {
        voxelizer.Draw();
    }
  }
}
