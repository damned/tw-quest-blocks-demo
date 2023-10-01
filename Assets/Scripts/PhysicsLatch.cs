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
        this.toEnd = toEnd;
        fromBlock = fromEnd.Block;
        toBlock = toEnd.Block;
    }

    public static Latch LatchBetween(LatchEnd fromEnd, LatchEnd toEnd)
    {
        Debug.Log("creating latch from " + fromEnd + ", to latch end: " + toEnd);
        return new PhysicsLatch(fromEnd, toEnd).Apply();
    }

    public Latch Apply()
    {
        Debug.Log("adding fixedjoint latch...");
        fixedJoint = fromBlock.AddComponent<FixedJoint>();
        var toRigidbody = toBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = toRigidbody;
        toEnd.OnLatch(this);
        Debug.Log("latched");
        return this;
    }

    public void Destroy()
    {
        Debug.Log("destroying fixedjoint latch...");
        GameObject.Destroy(fixedJoint);
        fixedJoint = null;
        fromEnd.OnUnlatch();
        toEnd.OnUnlatch();
        Debug.Log("unlatched");
    }

    public LatchEnd OtherEndTo(LatchEnd refLatchEnd)
    {
        if (refLatchEnd == toEnd)
        {
            return fromEnd;
        }
        else if (refLatchEnd == fromEnd)
        {
            return toEnd;
        }
        throw new System.Exception("Cannot determine other end to given latch end (" + refLatchEnd + "), as it is not one of this latch's known ends");
    }

    public override string ToString()
    {
        return "physics latch from: " + fromEnd + ", to: " + toEnd;
    }
}
