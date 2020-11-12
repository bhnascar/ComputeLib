using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ScanCompactTest
    {
        [TestCase(10)]
        [TestCase(1000)]
        [TestCase(100000)]
        public void TestCompact(int count)
        {
            int[] data = new int[count];
            int[] selected = new int[count];

            for (int i = 0; i < count; i++) {
                data[i] = i;
                selected[i] = i % 2;
            }

            using (StructuredBuffer<int> buffer = new StructuredBuffer<int>(count)) 
            using (StructuredBuffer<int> keys = new StructuredBuffer<int>(count))
            {
                buffer.SetData(data);
                keys.SetData(selected);

                ScanCompact compactor = new ScanCompact();
                compactor.Compact(buffer, keys, buffer.Count);

                int[] result = buffer.GetData();
                for (int i = 0; i < count / 2; i++) {
                    Assert.AreEqual(result[i], 2 * i + 1);
                }
            }
        }
    }
}
