using System.Collections;
using System.Linq;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;


public class ShadowCreatorTests
{
    Vector3EqualityComparer closeEnoughVectorComparer = Vector3EqualityComparer.Instance;

    public class CreateAlignmentHandle_ShouldAddAParentHandleToTheParentBlockOfTheMagnet : ShadowCreatorTests
    {
        [Test]
        public void ShouldCreateAShadowBlockWithCollidersAndMagnetScriptsTurnedOffAndRendererTurnedOffByDefault()
        {
            var magnet = CreateMagnetAndParentBlock();
            var block = magnet.transform.parent.gameObject;
            
            var shadowMagnet = new ShadowCreator().CreateShadowBlock(magnet);
            var shadowBlock = shadowMagnet.transform.parent.gameObject;

            Assert.AreNotEqual(block.GetInstanceID(), shadowBlock.GetInstanceID());

            Assert.IsFalse(shadowBlock.GetComponent<Renderer>().enabled);

            var shadowMagnetScript = shadowBlock.GetComponentInChildren<MagneticSnapper>();

            Assert.IsFalse(shadowMagnetScript.enabled);
            Assert.IsFalse(shadowMagnet.GetComponent<Collider>().enabled);
        }
    }

    private static GameObject CreateMagnetAndParentBlock()
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.AddComponent<Rigidbody>();

        var magnet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        magnet.AddComponent<MagneticSnapper>();
        magnet.transform.parent = block.transform;
        magnet.transform.Translate(magnet.transform.forward * 0.5f);

        return magnet;
    }
}
