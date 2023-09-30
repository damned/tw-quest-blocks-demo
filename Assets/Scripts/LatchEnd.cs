using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatchEnd : MonoBehaviour
{
    private Latch latch = null;
    private bool isInitiator = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        latch = new PhysicsLatch();
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
