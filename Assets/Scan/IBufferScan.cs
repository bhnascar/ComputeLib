using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBufferScan<T> where T : struct
{
	void Scan(ComputeBufferBase<T> buffer, int count);

	void ScanIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<uint> count);
}
