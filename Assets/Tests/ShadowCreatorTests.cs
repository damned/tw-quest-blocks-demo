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


        [Test]
        public void ShouldCreateAShadowBlockForASpecificMagnetOnAMultiMagnetBlock()
        {
            var magnet = CreateMagnetAndParentBlock();
            var block = magnet.transform.parent.gameObject;

            var secondMagnet = CreateMagnetOn(block);
            secondMagnet.transform.localPosition = new Vector3(1, 1, 1);
            secondMagnet.name = "Magnet2";

            var shadowMagnet = new ShadowCreator().CreateShadowBlock(magnet);
            var shadowBlock = shadowMagnet.transform.parent.gameObject;

            Assert.AreNotEqual(block.GetInstanceID(), shadowBlock.GetInstanceID());

            Assert.IsFalse(shadowBlock.GetComponent<Renderer>().enabled);

            var shadowMagnetScript = shadowMagnet.GetComponent<MagneticSnapper>();

            Assert.IsFalse(shadowMagnetScript.enabled);
            Assert.IsFalse(shadowMagnet.GetComponent<Collider>().enabled);

            Assert.That(shadowMagnet.transform.localPosition, Is.EqualTo(magnet.transform.localPosition).Using(closeEnoughVectorComparer));

            var secondShadowMagnetTransform = shadowBlock.transform.Find("Magnet2");
            var secondShadowMagnet = secondShadowMagnetTransform.gameObject;

            Assert.IsFalse(secondShadowMagnet.GetComponent<MagneticSnapper>().enabled);
            Assert.IsFalse(secondShadowMagnet.GetComponent<Collider>().enabled);
            
            Assert.That(secondShadowMagnet.transform.localPosition, Is.EqualTo(secondMagnet.transform.localPosition).Using(closeEnoughVectorComparer));
         }
    }

    private static GameObject CreateMagnetAndParentBlock()
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.AddComponent<Rigidbody>();

        var magnet = CreateMagnetOn(block);
        magnet.transform.Translate(magnet.transform.forward * 0.5f);

        return magnet;
    }

    private static GameObject CreateMagnetOn(GameObject block)
    {
        var magnet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        magnet.AddComponent<MagneticSnapper>();
        magnet.transform.parent = block.transform;
        return magnet;
    }

}
