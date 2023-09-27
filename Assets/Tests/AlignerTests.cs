using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;


public class AlignerTests
{
    Vector3EqualityComparer closeEnoughVectorComparer = Vector3EqualityComparer.Instance;

    public class CreateAlignmentHandle_ShouldAddAParentHandleToTheParentBlockOfTheMagnet : AlignerTests
    {
        [Test]
        public void ShouldAddHandleAtSamePositionAndDirectionInZAxis_SoThatTheBlockCanBeEasilyAlignedWithRespectToTheMagnet()
        {
            var magnet = CreateOutwardFacingMagnetAtPositiveZOnNewBlockAtOriginUsingScale(1);
            var blockTransform = magnet.transform.parent;

            var aligner = new MagnetAligner();
            var handle = aligner.CreateAlignmentHandle(magnet);

            Assert.AreEqual(blockTransform.parent, handle.transform);

            Assert.That(handle.transform.position, Is.EqualTo(magnet.transform.position).Using(closeEnoughVectorComparer));
            Assert.AreEqual(magnet.transform.rotation, handle.transform.rotation);
        }

        [Test]
        public void ShouldAddHandleAtSamePositionAndDirectionInXAxis_SoThatTheBlockCanBeEasilyAlignedWithRespectToTheMagnet()
        {
            var magnet = CreateOutwardFacingMagnetAtDirectionOnNewBlockAtOriginUsingScale(new Vector3(1, 0, 0), 1);
            var blockTransform = magnet.transform.parent;

            var aligner = new MagnetAligner();
            var handle = aligner.CreateAlignmentHandle(magnet);

            Assert.AreEqual(blockTransform.parent, handle.transform);

            Assert.That(handle.transform.position, Is.EqualTo(magnet.transform.position).Using(closeEnoughVectorComparer));
            Assert.AreEqual(magnet.transform.rotation.eulerAngles, handle.transform.rotation.eulerAngles);
        }
    }

    public class ShouldCreateMagnetAndBlockForTestsTests : AlignerTests
    {
        [Test]
        public void ShouldHaveSetupBlockAndMagnetCorrectlyAtUnitScale()
        {
            var magnet = CreateOutwardFacingMagnetAtPositiveZOnNewBlockAtOriginUsingScale(1);
            var blockTransform = magnet.transform.parent;

            Assert.AreEqual(new Vector3(0, 0, 0), blockTransform.position);
            Assert.AreEqual(new Vector3(0, 0, 0), blockTransform.rotation.eulerAngles);
            

            Assert.AreEqual(new Vector3(0, 0, 0.5f), magnet.transform.position); 
            Assert.AreEqual(new Vector3(0, 0, 0.5f), magnet.transform.localPosition); 
            Assert.AreEqual(new Vector3(0, 0, 0), magnet.transform.rotation.eulerAngles);
        }

        [Test]
        public void ShouldHaveSetupBlockAndMagnetCorrectlyAtHalfScale()
        {
            var magnet = CreateOutwardFacingMagnetAtPositiveZOnNewBlockAtOriginUsingScale(0.5f);
            var blockTransform = magnet.transform.parent;

            Assert.AreEqual(new Vector3(0, 0, 0), blockTransform.position);
            Assert.AreEqual(new Vector3(0, 0, 0), blockTransform.rotation.eulerAngles);
            

            Assert.AreEqual(new Vector3(0, 0, 0.5f), magnet.transform.localPosition); 
            Assert.AreEqual(new Vector3(0, 0, 0.25f), magnet.transform.position); 
            Assert.AreEqual(new Vector3(0, 0, 0), magnet.transform.rotation.eulerAngles);
        }
    }

    private GameObject CreateOutwardFacingMagnetAtPositiveZOnNewBlockAtOriginUsingScale(float scale)
    {
        return CreateOutwardFacingMagnetAtDirectionOnNewBlockAtOriginUsingScale(new Vector3(0, 0, 1), scale);
    }

    private static GameObject CreateOutwardFacingMagnetAtDirectionOnNewBlockAtOriginUsingScale(Vector3 direction, float scale)
    {
        var block = new GameObject();
        var magnet = new GameObject();

        magnet.transform.parent = block.transform;
        block.transform.localScale = new Vector3(scale, scale, scale);
        magnet.transform.LookAt(direction);
        magnet.transform.Translate(magnet.transform.forward * scale / 2);

        return magnet;
    }
}
