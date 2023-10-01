using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class KinematicsCompound : MonoBehaviour
{
    void Start()
    {
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
