using System;
using UnityEngine;

public class HashSet : IBufferSet<int>, IDisposable
{
    private static ComputeShader shader;
    private static int insertKernel;
    private static int containsKernel;
    private static int removeKernel;

    private StructuredBuffer<int> setData;
    private const int EMPTY = 0x7FFFFFFF;

    static HashSet() {
        shader = Resources.Load<ComputeShader>("HashSet");
        insertKernel = shader.FindKernel("Insert");
        containsKernel = shader.FindKernel("Contains");
        removeKernel = shader.FindKernel("Remove");
    }

    public HashSet(int maxCount) {
        setData = new StructuredBuffer<int>(maxCount);
        Clear();
    }

    public void Dispose() {
        setData.Dispose();
    }

    public void Insert(ComputeBufferBase<int> values, ComputeBufferBase<int> results) {
        uint gx, gy, gz;
        shader.GetKernelThreadGroupSizes(insertKernel, out gx, out gy, out gz);
        int numGroupsX = Mathf.CeilToInt((float)values.Count / gx);

        BindHashSetCommon(insertKernel);
        BindKernelsCommon(insertKernel, values, results);
        shader.Dispatch(insertKernel, numGroupsX, 1, 1);
    }

    public void Contains(ComputeBufferBase<int> values, ComputeBufferBase<int> results) {
        uint gx, gy, gz;
        shader.GetKernelThreadGroupSizes(containsKernel, out gx, out gy, out gz);
        int numGroupsX = Mathf.CeilToInt((float)values.Count / gx);

        BindHashSetCommon(containsKernel);
        BindKernelsCommon(containsKernel, values, results);
        shader.Dispatch(containsKernel, numGroupsX, 1, 1);
    }

    public void Remove(ComputeBufferBase<int> values, ComputeBufferBase<int> results) {
        uint gx, gy, gz;
        shader.GetKernelThreadGroupSizes(removeKernel, out gx, out gy, out gz);
        int numGroupsX = Mathf.CeilToInt((float)values.Count / gx);

        BindHashSetCommon(removeKernel);
        BindKernelsCommon(removeKernel, values, results);
        shader.Dispatch(removeKernel, numGroupsX, 1, 1);
    }

    public void Clear() {
        int[] emptyData = new int[setData.Count];
        for (int i = 0; i < emptyData.Length; i++) {
            emptyData[i] = EMPTY;
        }
        setData.SetData(emptyData);
    }

    public void BindHashSetCommon(int kernel) {
        shader.SetInt("set_size", setData.Count);
        shader.SetBuffer(kernel, "set_data", setData.Buffer);
    }

    private void BindKernelsCommon(int kernel, ComputeBufferBase<int> values, ComputeBufferBase<int> results) {
        shader.SetInt("count", values.Count);
        shader.SetBuffer(kernel, "input", values.Buffer);
        shader.SetBuffer(kernel, "results", results.Buffer);
    }
}
