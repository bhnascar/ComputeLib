using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MortonEncodingTest
    {
        [Test]
        public void EncodeDecode()
        {
            Vector3 coords = new Vector3(1,2,3);
            Assert.AreEqual(Morton.Decode(Morton.Encode(coords)), coords);
        }
    }
}
