using System;
using UnityEngine;

public class ScanCompact : IBufferCompact<int>
{
    private static ComputeShader shader;
    private static int compactKernel;
    private static int copyBackKernel;

    static ScanCompact() {
        shader = Resources.Load<ComputeShader>("ScanCompact");
        compactKernel = shader.FindKernel("Compact");
        copyBackKernel = shader.FindKernel("CopyBack");
    }

	public void Compact(ComputeBufferBase<int> buffer, ComputeBufferBase<int> keys, int count) {
        var scan = new NaiveScan();
        scan.Scan(keys, count);

        uint gx, gy, gz;
        shader.GetKernelThreadGroupSizes(compactKernel, out gx, out gy, out gz);
        int numGroupsX = Mathf.CeilToInt((float)count / gx);

        using (var tmp = new StructuredBuffer<int>(count)) {
            shader.SetInt("count", count);
            
            shader.SetBuffer(compactKernel, "data", buffer.Buffer);
            shader.SetBuffer(compactKernel, "output", tmp.Buffer);
            shader.SetBuffer(compactKernel, "keys", keys.Buffer);
            shader.Dispatch(compactKernel, numGroupsX, 1, 1);

            shader.SetBuffer(copyBackKernel, "data", tmp.Buffer);
            shader.SetBuffer(copyBackKernel, "output", buffer.Buffer);
            shader.Dispatch(copyBackKernel, numGroupsX, 1, 1);
        }
    }

	public void CompactIndirect(ComputeBufferBase<int> buffer, ComputeBufferBase<int> keys, ComputeBufferBase<uint> count) {
        throw new NotImplementedException();
    }
}
