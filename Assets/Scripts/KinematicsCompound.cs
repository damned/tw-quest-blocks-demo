using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class KinematicsCompound : MonoBehaviour
{

    void Start()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void Update()
    {
    }

    public void Init()
    {
        gameObject.name = "KinematicsCompound";
        // NB: seem to need to add compound rigidbody before reparenting
        // blocks under it, otherwise XRGI does not work consistently
        var compoundRigidbody = gameObject.AddComponent<Rigidbody>();
        compoundRigidbody.isKinematic = true;
        compoundRigidbody.useGravity = false;
    }

    public void CommitUpdates()
    {
        var xrgi = gameObject.AddComponent<XRGrabInteractable>();
        xrgi.throwOnDetach = false;
        xrgi.useDynamicAttach = true;

        FixDynamicGrabInteractableFormation(xrgi);
    }

    private static void FixDynamicGrabInteractableFormation(XRGrabInteractable xrgi)
    {
        var interactable = xrgi.GetComponent<IXRInteractable>();
        xrgi.interactionManager.RegisterInteractable(interactable);
    }

    public void AddBlock(GameObject block)
    {
        BindIntoCompound(block, transform);
    }

    public void OnGrab(SelectEnterEventArgs enterEvent)
    {
        Pose attachPose = enterEvent.interactableObject.GetAttachPoseOnSelect(enterEvent.interactorObject);
        Debug.Log("on compound grab, attach pose: " + attachPose);
        int count = transform.childCount;
        Debug.Log("compound child count: " + count);
        for(int i = 0; i < count; i++)
        {
            Transform blockTransform = transform.GetChild(i);
            var block = blockTransform.gameObject;
            var collider = block.GetComponent<Collider>();

            var closestPoint = collider == null ? "n/a" : "" + collider.ClosestPoint(attachPose.position);
            Debug.Log("for ref, child / block " + block.name + 
                ", transform: " + blockTransform.position + 
                ", distance: " + Vector3.Distance(blockTransform.position, attachPose.position) +
                ", closest point on collider: " + closestPoint);
        }
    }

    public void OnRelease(SelectExitEventArgs exitEvent)
    {
        Debug.Log("on compound release");
    }

    private static void BindIntoCompound(GameObject block, Transform compoundTransform)
    {
        RemoveRigidbodyAndXrComponents(block);
        block.transform.parent = compoundTransform;
    }

    private static void RemoveRigidbodyAndXrComponents(GameObject block)
    {
        var xrgi = block.GetComponent<XRGrabInteractable>();
        var interactable = xrgi.GetComponent<IXRInteractable>();
        xrgi.interactionManager.UnregisterInteractable(interactable);
        GameObject.Destroy(xrgi);
        var xrggt = block.GetComponent<XRGeneralGrabTransformer>();
        GameObject.Destroy(xrggt);
        var rigidbody = block.GetComponent<Rigidbody>();
        GameObject.Destroy(rigidbody);
    }
}
