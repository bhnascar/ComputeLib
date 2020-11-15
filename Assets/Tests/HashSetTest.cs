using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HashSetTest
    {
        [Test]
        public void TestInsert() {
            int count = 100;
            int[] testData = new int[count];
            for (int i = 0; i < count; i++) {
                testData[i] = i;
            }

            using (var set = new HashSet(count))
            using (var input = new StructuredBuffer<int>(count)) 
            using (var results = new StructuredBuffer<int>(count)) {
                input.SetData(testData);

                // First insertion should succeed.
                set.Insert(input, results);
                var data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }

                // Second insertion should fail because elements
                // already exist.
                set.Insert(input, results);
                data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 0);
                }
            }
        }

        [Test]
        public void TestProbing() {
            int count = 6;
            int[] testData = new int[count];
            for (int i = 0; i < count; i++) {
                testData[i] = i * count; // These keys will have hash conflicts.
            }

            using (var set = new HashSet(count))
            using (var input = new StructuredBuffer<int>(count)) 
            using (var results = new StructuredBuffer<int>(count)) {
                input.SetData(testData);

                // First insertion should succeed.
                set.Insert(input, results);
                var data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }

                // Contains should succeed.
                set.Contains(input, results);
                data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }

                // Remove should succeed.
                set.Remove(input, results);
                data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }
            }
        }

        [Test]
        public void TestContains() {
            int count = 100;
            int[] testData = new int[count];
            for (int i = 0; i < count; i++) {
                testData[i] = i;
            }

            using (var set = new HashSet(count))
            using (var input = new StructuredBuffer<int>(count)) 
            using (var results = new StructuredBuffer<int>(count)) {
                input.SetData(testData);
                
                // Nothing should be found.
                set.Contains(input, results);
                var data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 0);
                }

                // Now insert. All elements should be found.
                set.Insert(input, results);
                set.Contains(input, results);
                data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }
            }
        }

        [Test]
        public void TestRemove() {
            int count = 100;
            int[] testData = new int[count];
            for (int i = 0; i < count; i++) {
                testData[i] = i;
            }

            using (var set = new HashSet(count))
            using (var input = new StructuredBuffer<int>(count)) 
            using (var results = new StructuredBuffer<int>(count)) {
                input.SetData(testData);

                // Empty set - all removes should fail.
                set.Remove(input, results);
                var data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 0);
                }

                // Now insert. All removes should succeed.
                set.Insert(input, results);
                set.Remove(input, results);
                data = results.GetData();
                for (int i = 0; i < count; i++) {
                    Assert.AreEqual(data[i], 1);
                }
            }
        }

        [Test]
        public void TestOverflow() {
            int diff = 10;
            int count = 100;
            int[] testData = new int[count];
            for (int i = 0; i < count; i++) {
                testData[i] = i;
            }

            using (var set = new HashSet(count - diff))
            using (var input = new StructuredBuffer<int>(count)) 
            using (var results = new StructuredBuffer<int>(count)) {
                input.SetData(testData);

                // # failures should be difference between set size
                // and input size.
                int numFailures = 0;
                set.Insert(input, results);
                var data = results.GetData();
                for (int i = 0; i < count; i++) {
                    if (data[i] == 0) {
                        numFailures++;
                    }
                }
                Assert.AreEqual(numFailures, diff);
            }
        }
    }
}
