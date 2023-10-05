using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MagneticBlock : MonoBehaviour
{
    private bool isGrabbed = false;
    private XRGrabInteractable grabInteractable;
    private List<Magnet> magnetScripts;

    // Start is called before the first frame update
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        magnetScripts = GetComponentsInChildren<Magnet>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        magnetScripts.ForEach(ms => ms.OnGrab());
    }

    public void OnRelease(SelectExitEventArgs args)
    {
        magnetScripts.ForEach(ms => ms.OnRelease());
        isGrabbed = false;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    public bool IsInACompound()
    {
        foreach (var magnet in magnetScripts)
        {
            if (magnet.IsLatched)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsALeafInACompound()
    {
        int latchCount = 0;
        foreach (var magnet in magnetScripts)
        {
            if (magnet.IsLatched)
            {
                latchCount++;
            }
        }
        return latchCount == 1;
    }
}
