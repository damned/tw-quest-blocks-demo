using UnityEngine;

public class PhysicsLatch : Latch
{
    private readonly GameObject fromBlock;
    private readonly GameObject toBlock;
    private readonly LatchEnd fromEnd;
    private readonly LatchEnd toEnd;
    private FixedJoint fixedJoint;

    public PhysicsLatch(LatchEnd fromEnd, LatchEnd toEnd)
    {
        this.fromEnd = fromEnd;
        this.toEnd = fromEnd;
        fromBlock = fromEnd.Block;
        toBlock = toEnd.Block;
    }

    public static Latch LatchBetween(LatchEnd fromEnd, LatchEnd toEnd)
    {
        Debug.Log("creating latch, to latch end: " + toEnd);
        return new PhysicsLatch(fromEnd, toEnd).Apply();
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
        fromEnd.OnUnlatch();
        toEnd.OnUnlatch();
    }

}
