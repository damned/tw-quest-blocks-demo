using System;
using UnityEngine;

public class LatchEnd : MonoBehaviour
{
    public GameObject Block
    {
        get {
            return block;
        }
    }

    private Latch latch = null;
    private bool isInitiator = false;
    private GameObject block;
    private Magnet magnet;

    void Start()
    {
        magnet = GetComponent<Magnet>();
        block = transform.parent.gameObject;
    }

    void Update()
    {
        
    }

    public bool IsLatched()
    {
        if (latch != null)
        {
            return true;
        }
        return false;
    }

    public void LatchTo(LatchEnd to)
    {
        if (IsLatched())
        {
            Debug.LogWarning("woah, i think your logic is off, not going to latch again - i'm already latched via: " + latch);
            return;
        }
        if (this.block.name == to.block.name)
        {
            Debug.LogWarning("woah, i think your logic is off, you're telling me to latch to myself: " + this);
            return;
        }
        isInitiator = true;
        latch = PhysicsLatch.LatchBetween(this, to);
        magnet.OnLatch();
    }

    public void Unlatch()
    {
        latch.Destroy();
    }

    public void OnLatch(Latch latchInitiatedByOther)
    {
        isInitiator = false;
        latch = latchInitiatedByOther;
        magnet.OnLatch();
    }

    public void OnUnlatch()
    {
        latch = null;
        magnet.OnUnlatch();
    }

    public bool IsInitiator()
    {
        return isInitiator;
    }

    public bool IsOtherBlockGrabbed()
    {
        if (!IsLatched())
        {
            return false;
        }
        return latch.OtherEndTo(this).IsBlockGrabbed();
    }

    public bool IsBlockGrabbed()
    {
        return gameObject.GetComponent<Magnet>().IsBlockGrabbed();
    }

    public override string ToString()
    {
        return "[LatchEnd at " + transform.localPosition + " on " + block.name + "]";
    }
}
