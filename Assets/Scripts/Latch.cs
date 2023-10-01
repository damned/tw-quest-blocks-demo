using UnityEngine;

public class Latch
{
    private readonly LatchEnd fromEnd;
    private readonly LatchEnd toEnd;
    private readonly LatchBinder binder;

    private FixedJoint fixedJoint;

    public Latch(LatchEnd fromEnd, LatchEnd toEnd, LatchBinder binder)
    {
        this.fromEnd = fromEnd;
        this.toEnd = toEnd;
        this.binder = binder;
    }

    public static Latch LatchBetween(LatchEnd fromEnd, LatchEnd toEnd)
    {
        Debug.Log("creating latch from " + fromEnd + ", to latch end: " + toEnd);
        return new Latch(fromEnd, toEnd, new PhysicsBinder()).Apply();
    }

    public Latch Apply()
    {
        binder.Apply(fromEnd, toEnd);
        toEnd.OnLatch(this);
        Debug.Log("latched");
        return this;
    }

    public void Destroy()
    {
        binder.Destroy();
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
        return "latch from: " + fromEnd + ", to: " + toEnd;
    }
}
