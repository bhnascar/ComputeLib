using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuredBuffer<T> : ComputeBufferBase<T> where T : struct
{
	public StructuredBuffer(int count, int counterValue = 0) : base(count, ComputeBufferType.Default) {
	}
}
