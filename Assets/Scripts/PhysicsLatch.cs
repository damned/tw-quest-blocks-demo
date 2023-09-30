using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsLatch : Latch
{
    private GameObject fromBlock;
    private GameObject toBlock;
    private FixedJoint fixedJoint;

    public PhysicsLatch(GameObject fromBlock, GameObject toBlock)
    {
        this.fromBlock = fromBlock;
        this.toBlock = toBlock;
    }

    public static Latch LatchBetween(GameObject fromBlock, GameObject toBlock)
    {
        Debug.Log("creating latch, toBlock: " + toBlock);
        return new PhysicsLatch(fromBlock, toBlock).Apply();
    }

    private Latch Apply()
    {
        fixedJoint = fromBlock.AddComponent<FixedJoint>();
        var toRigidbody = toBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = toRigidbody;
        return this;
    }

    public void Destroy()
    {
        GameObject.Destroy(fixedJoint);
        fixedJoint = null;
    }

}
