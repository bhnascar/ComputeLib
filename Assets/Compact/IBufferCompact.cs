using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBufferCompact<T> where T : struct
{
	void Compact(ComputeBufferBase<T> buffer, ComputeBufferBase<int> keys, int count);

	void CompactIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<int> keys, ComputeBufferBase<uint> count);
}
