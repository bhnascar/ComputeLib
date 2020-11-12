using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBufferSort<T> where T : struct
{
	void Sort(ComputeBufferBase<T> buffer, int count = 0);

	void SortIndirect(ComputeBufferBase<T> buffer, ComputeBufferBase<uint> count);
}
