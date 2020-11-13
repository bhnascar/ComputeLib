using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OctreeLoader : MonoBehaviour
{
    public int maxDepth = 2;

    private Octree octree;

    void Start() {
        var mesh = GetComponent<MeshFilter>().mesh;
        octree = new Octree(mesh.bounds, maxDepth);
        octree.Insert(mesh);
    }

    void OnDestroy() {
        octree.Dispose();
    }

    void OnDrawGizmos() {
        if (Application.isPlaying) {
            octree.Draw();
        }
    }
}
