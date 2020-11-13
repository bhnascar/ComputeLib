using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgsBuffer : ComputeBufferBase<uint>
{
	public ArgsBuffer() : base(3, ComputeBufferType.IndirectArguments) {
	}
}
