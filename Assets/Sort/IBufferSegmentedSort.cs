using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBufferSegmentedSort<T> where T : struct
{
	void Sort(ComputeBufferBase<T> buffer, uint[] segmentsHeads);

	void SortIndirect(ComputeBufferBase<T> buffer, CounterBuffer<uint> segmentHeads);
}
