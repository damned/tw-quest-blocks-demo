using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.Interaction.Toolkit.TestSupport;

public class HandsFreePlayTests
{
    Vector3EqualityComparer closeEnoughVectorComparer = Vector3EqualityComparer.Instance;
    
    [SetUp]
    public void TearDown()
    {
        TestUtilities.DestroyAllSceneObjects();
    }

    [SetUp]
    public void SetUp()
    {
        Physics.autoSimulation = true;
    }

    [UnityTest]
    public IEnumerator ShouldSnapAndLatchWhenBlockMovesIntoAnotherAtFacingMagnets()
    {
        var magnet = BlockBuilder.CreateMagnetAndParentBlock();
        var block = magnet.transform.parent.gameObject;

        var magnet2 = BlockBuilder.CreateMagnetAndParentBlock();
        var block2 = magnet2.transform.parent.gameObject;
        block2.transform.Translate(Vector3.forward * 2);
        block2.transform.Rotate(Vector3.up, 180f);

        block2.GetComponent<Rigidbody>().isKinematic = true;
        Debug.Log("physics simulation mode: " + Physics.autoSimulation);

        yield return new Update();
        
        Debug.Log("waited for update and thereby initialisation");

        Assert.IsFalse(magnet2.GetComponent<Magnet>().IsLatched);

        for (int i = 0; i < 12; i++)
        {
            Debug.Log("moving forward, block position: " + block2.transform.position);
            block2.transform.Translate(Vector3.forward * 0.1f);

            yield return new WaitForFixedUpdate();
        }
        
        Assert.IsTrue(magnet2.GetComponent<Magnet>().IsLatched);

        block2.transform.position = new Vector3(1, 1, 1);
        Assert.AreEqual(0f, block.transform.position.x, 0.001f);

        yield return new WaitForFixedUpdate();

        Assert.That(block.transform.position, Is.EqualTo(new Vector3(1, 1, 0)).Using(closeEnoughVectorComparer));
    }

}
