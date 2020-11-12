using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterBuffer<T> : ComputeBufferBase<T> where T : struct
{
	public CounterBuffer(int count, uint counterValue = 0) : base(count, ComputeBufferType.Counter) {
		SetCounterValue(counterValue);
	}

	public void CopyCount(RawBuffer<uint> other, int itemOffset = 0) {
		ComputeBuffer.CopyCount(Buffer, other.Buffer, itemOffset * other.Buffer.stride);
	}

	public void SetCounterValue(uint counterValue) {
		Buffer.SetCounterValue(counterValue);
	}

	public uint GetCounterValue() {
		uint counterValue;
		using (var countBuffer = new RawBuffer<uint>(1)) {
			CopyCount(countBuffer);
			counterValue = countBuffer[0];
		}
		return counterValue;
	}
}
