using System;
using UnityEngine;

public class Octree : IDisposable
{
    // TODO: Pad to align to float4?
    internal unsafe struct Node {
        public int has_children;
        public fixed int children[8];
    }

    private int maxDepth;
    private Bounds bounds;
    private StructuredBuffer<Node> nodes;

    private int[] leafData;

    private static ComputeShader shader;
    private static int computeLeavesKernel;
    private static int markUniqueLeavesKernel;

    private BitonicSort sorter;
    private ScanCompact compactor;

    static Octree() {
        shader = Resources.Load<ComputeShader>("Octree");
        computeLeavesKernel = shader.FindKernel("ComputeLeaves");
        markUniqueLeavesKernel = shader.FindKernel("MarkUniqueLeaves");
    }

    public Octree(Bounds bounds, int maxDepth = 5) {
        int maxNodes = 0;
        for (int i = 1; i <= maxDepth; i++) {
            maxNodes += i * i * i;
        }

        nodes = new StructuredBuffer<Node>(maxNodes);
        nodes.SetData(new Node[maxNodes]);

        this.bounds = bounds;
        this.maxDepth = maxDepth;

        sorter = new BitonicSort();
        compactor = new ScanCompact();
    }

    public void Dispose() {
        nodes.Dispose();
	}

    public void Insert(Mesh mesh) {
        Insert(mesh.vertices);
    }

    public void Insert(Vector3[] data) {
        uint gx, gy, gz;
        shader.GetKernelThreadGroupSizes(computeLeavesKernel, out gx, out gy, out gz);
        int numGroupsX = Mathf.CeilToInt((float)data.Length / gx);

        using (var leaves = new StructuredBuffer<int>(data.Length))
        using (var keys = new CounterBuffer<int>(data.Length))
        using (var points = new StructuredBuffer<Vector3>(data.Length)) {
            points.SetData(data);

            shader.SetFloats("size", bounds.size.x, bounds.size.y, bounds.size.z);
            shader.SetFloats("min_corner", bounds.min.x, bounds.min.y, bounds.min.z);
            shader.SetInt("max_depth", maxDepth);
            shader.SetInt("point_count", data.Length);

            shader.SetBuffer(computeLeavesKernel, "leaves", leaves.Buffer);
            shader.SetBuffer(computeLeavesKernel, "points", points.Buffer);
            shader.Dispatch(computeLeavesKernel, numGroupsX, 1, 1);

            sorter.Sort(leaves, data.Length);

            shader.SetBuffer(markUniqueLeavesKernel, "leaves", leaves.Buffer);
            shader.SetBuffer(markUniqueLeavesKernel, "unique", keys.Buffer);
            shader.Dispatch(markUniqueLeavesKernel, numGroupsX, 1, 1);

            compactor.Compact(leaves, keys, data.Length);

            int leafCount = (int)keys.GetCounterValue();
            leafData = leaves.GetData();

            // TODO:
            /*
            for (int i = 0; i < maxDepth; i++) {
                // Mark and subdivide.
            }
            */
        }
    }

    public void Draw() {
        if (leafData == null) {
            return;
        }
        Vector3 leafSize = bounds.size / (1 << maxDepth);
        for (int i = 0; i < leafData.Length; i++) {
            int leaf = leafData[i];
            if (leaf != 0) {
                var coords = Morton.Decode(leaf);
                var center = bounds.min + Vector3.Scale(coords + 0.5f * Vector3.one, leafSize);
                Gizmos.DrawWireCube(center, leafSize);
            }
        }
    }
}
