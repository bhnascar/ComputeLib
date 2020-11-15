using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBufferSet<T> where T : struct
{
    void Insert(ComputeBufferBase<T> values, ComputeBufferBase<int> results = null);

    void Contains(ComputeBufferBase<T> values, ComputeBufferBase<int> results = null);

    void Remove(ComputeBufferBase<T> values, ComputeBufferBase<int> results = null);
}
