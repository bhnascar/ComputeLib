using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BitonicSortTest
    {
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(8192)]
        [TestCase(65536)]
        public void TestPow2(int count)
        {
            using (StructuredBuffer<int> buffer = new StructuredBuffer<int>(count)) {
                int[] testData = new int[count];
                for (int i = 0; i < count; i++) {
                    testData[i] = count - 1 - i;
                }
                buffer.SetData(testData);

                BitonicSort sorter = new BitonicSort();
                sorter.Sort(buffer, buffer.Count);

                int[] result = buffer.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(result[i], i);
                }
            }
        }

        [TestCase(24)]
        [TestCase(48)]
        [TestCase(100)]
        [TestCase(500)]
        public void TestUnalignedSizes(int count)
        {
            using (StructuredBuffer<int> buffer = new StructuredBuffer<int>(count)) {
                int[] testData = new int[count];
                for (int i = 0; i < count; i++) {
                    testData[i] = count - 1 - i;
                }
                buffer.SetData(testData);

                BitonicSort sorter = new BitonicSort();
                sorter.Sort(buffer, buffer.Count);

                int[] result = buffer.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(result[i], i);
                }
            }
        }
    }
}
