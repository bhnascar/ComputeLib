using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NaiveScanTest
    {
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(512)]
        [TestCase(1024)]
        [TestCase(4096)]
        [TestCase(16384)]
        [TestCase(65536)]
        [TestCase(1048576)]
        public void TestPow2(int count)
        {
            using (StructuredBuffer<int> buffer = new StructuredBuffer<int>(count)) {
                int sum = 0;
                int[] testData = new int[count];
                int[] referenceScan = new int[count];
                for (int i = 0; i < count; i++) {
                    testData[i] = 1;
                    referenceScan[i] = sum;
                    sum += testData[i];
                }
                buffer.SetData(testData);

                NaiveScan scanner = new NaiveScan();
                scanner.Scan(buffer, buffer.Count);

                int[] result = buffer.GetData();
                for (int i = 0; i < referenceScan.Length; i++) {
                    Assert.AreEqual(result[i], referenceScan[i]);
                }
            }
        }

        [TestCase(17)]
        [TestCase(27)]
        [TestCase(77)]
        [TestCase(107)]
        public void TestUnalignedSizes(int count)
        {
            using (StructuredBuffer<int> buffer = new StructuredBuffer<int>(count)) {
                int sum = 0;
                int[] testData = new int[count];
                int[] referenceScan = new int[count];
                for (int i = 0; i < count; i++) {
                    testData[i] = 1;
                    referenceScan[i] = sum;
                    sum += testData[i];
                }
                buffer.SetData(testData);

                NaiveScan scanner = new NaiveScan();
                scanner.Scan(buffer, buffer.Count);

                int[] result = buffer.GetData();
                for (int i = 0; i < referenceScan.Length; i++) {
                    Assert.AreEqual(result[i], referenceScan[i]);
                }
            }
        }
    }
}
