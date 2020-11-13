using System;
using UnityEngine;

public class Octree : IDisposable
{
    // TODO: Pad to align to float4?
    internal unsafe struct Node {
        #pragma warning disable 0649
        public int child_flags;
        public fixed int children[8];
        #pragma warning restore 0649
    }

    private int maxDepth;
    private Bounds bounds;
    private CounterBuffer<Node> nodes;

    private Node[] nodeData;
    private int nodeCount;

    private static ComputeShader shader;
    private static int computeLeavesKernel;
    private static int markUniqueLeavesKernel;
    private static int subdivideKernel;

    private BitonicSort sorter;
    private ScanCompact compactor;

    static Octree() {
        shader = Resources.Load<ComputeShader>("Octree");
        computeLeavesKernel = shader.FindKernel("ComputeLeaves");
        markUniqueLeavesKernel = shader.FindKernel("MarkUniqueLeaves");
        subdivideKernel = shader.FindKernel("Subdivide");
    }

    public Octree(Bounds bounds, int maxDepth = 5) {
        int maxNodes = 0;
        for (int i = 1; i <= maxDepth; i++) {
            int res = 1 << i;
            maxNodes += res * res * res;
        }

        nodes = new CounterBuffer<Node>(maxNodes, 1);
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

    public unsafe void Insert(Vector3[] data) {
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
            int leafGroupsX = Mathf.CeilToInt((float)leafCount / gx);

            for (int i = 0; i < maxDepth; i++) {
                shader.SetInt("leaf_count", leafCount);
                shader.SetInt("current_level", i);
                shader.SetBuffer(subdivideKernel, "leaves", leaves.Buffer);
                shader.SetBuffer(subdivideKernel, "nodes", nodes.Buffer);
                shader.Dispatch(subdivideKernel, leafGroupsX, 1, 1);
            }

            nodeData = nodes.GetData();
            nodeCount = (int)nodes.GetCounterValue();
        }
    }

    public void Draw() {
        if (nodeData == null) {
            return;
        }
        DrawNode(0, 0, Vector3.zero);
    }

    private unsafe void DrawNode(int idx, int depth, Vector3 coords) {
        Node node = nodeData[idx];
        var nodeSize = bounds.size / (1 << depth);
        var center = bounds.min + Vector3.Scale(coords + 0.5f * Vector3.one, nodeSize);

        Gizmos.DrawWireCube(center, nodeSize);

        for (int i = 0; i < 8; i++) {
            int child = node.children[i];
            if (child > 0 && child < nodeData.Length) {
                Vector3 child_coords = Morton.Decode(i);
                DrawNode(child, depth + 1, 2f * coords + child_coords);
            }
        }
    }
}
