using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatchEnd : MonoBehaviour
{
    private Latch latch = null;
    private bool isInitiator = false;
    private GameObject block;

    void Start()
    {
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

    public void LatchTo(LatchEnd other)
    {
        isInitiator = true;
        other.isInitiator = false;

        latch = PhysicsLatch.LatchBetween(block, other.block);
        other.latch = latch;
    }

    public void Unlatch()
    {
        latch.Destroy();
        latch = null;
    }

    public bool IsInitiator()
    {
        return isInitiator;
    }

}
