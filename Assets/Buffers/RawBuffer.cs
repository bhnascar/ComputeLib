using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawBuffer<T> : ComputeBufferBase<T> where T : struct
{
	public RawBuffer(int count, int counterValue = 0) : base(count, ComputeBufferType.Raw) {
	}
}
