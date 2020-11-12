using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class ComputeBufferBase<T> : IDisposable where T : struct
{
	public ComputeBuffer Buffer { get; private set; }

	public int Count => Buffer.count;
	public int Stride => Buffer.stride;

	public ComputeBufferBase(int count, ComputeBufferType type) {
		int elemSize = Marshal.SizeOf(typeof(T));
		Buffer = new ComputeBuffer(count, elemSize, type);
	}

	public override string ToString() {
		T[] data = GetData();
		string str = "";
		for (int i = 0; i < data.Length; i++) {
				str += data[i] + " ";
		}
		return str;
	}

	#region IDisposable

	public void Dispose() {
		if (Buffer != null) {
			Buffer.Dispose();
			Buffer = null;
		}
	}

	#endregion

	#region Bracket accessors

	public T this[int i] {
		get {
			return GetItemAtIndex(i);
		}
		set {
			SetItemAtIndex(i, value);
		}
	}

	public T[] this[int start, int end] {
		get {
			return GetItemsAtIndices(start, end);
		}
		set {
			SetItemsAtIndices(start, end, value);
		}
	}

	#endregion

	#region Original API (for drop-in compatibility)

	public void GetData(Array data) {
		Buffer.GetData(data);
	}

	public void GetData(Array data, int destOffset, int startOffset, int count) {
		Buffer.GetData(data, destOffset, startOffset, count);
	}

	public IntPtr GetNativeBufferPtr() {
		return Buffer.GetNativeBufferPtr();
	}

	public void Release() {
		Dispose();
	}

	public void SetData(T[] data) {
		Buffer.SetData(data);
	}

	public void SetData(Array data, int startOffset, int destOffset, int count) {
		Buffer.SetData(data, startOffset, destOffset, count);
	}

	#endregion

	#region Extension API

	public T[] GetData() {
		T[] data = new T[Count];
		Buffer.GetData(data);
		return data;
	}

	public T GetItemAtIndex(int index) {
		T[] items = GetData();
		return items[index];
	}

	public T[] GetItemsAtIndices(int start, int end) {
		T[] items = GetData();
		T[] subItems = new T[end - start];
		Array.Copy(items, start, subItems, 0, end - start);
		return subItems;
	}

	public void SetItemAtIndex(int index, T item) {
		T[] items = GetData();
		items[index] = item;
		SetData(items);
	}

	public void SetItemsAtIndices(int start, int end, T[] subItems) {
		T[] items = GetData();
		Array.Copy(subItems, 0, items, start, end - start);
		SetData(items);
	}

	#endregion
}
