using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class KinematicsBinder : LatchBinder
{
    private GameObject fromBlock;
    private GameObject toBlock;
    private GameObject compoundParent;

    public void Apply(LatchEnd fromEnd, LatchEnd toEnd)
    {
        Debug.Log("creating kinematics binder: creating kinematics compound...");
        fromBlock = fromEnd.Block;
        toBlock = toEnd.Block;

        compoundParent = new GameObject
        {
            name = "KinematicsCompound"
        };
        var compound = compoundParent.AddComponent<KinematicsCompound>();

        // compound.Init();

        // NB: seem to need to add compound rigidbody before reparenting
        // blocks under it, otherwise XRGI does not work consistently
        var compoundRigidbody = compoundParent.AddComponent<Rigidbody>();
        compoundRigidbody.isKinematic = true;
        compoundRigidbody.useGravity = false;

        BindIntoCompound(fromBlock, compoundParent.transform);
        BindIntoCompound(toBlock, compoundParent.transform);

        var xrgi = compoundParent.AddComponent<XRGrabInteractable>();
        xrgi.throwOnDetach = false;
        xrgi.useDynamicAttach = true;

        FixDynamicGrabInteractableFormation(xrgi);
    }

    private static void FixDynamicGrabInteractableFormation(XRGrabInteractable xrgi)
    {
        var interactable = xrgi.GetComponent<IXRInteractable>();
        xrgi.interactionManager.RegisterInteractable(interactable);
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


    public void Destroy()
    {
        Debug.Log("removing compound parent to unbind and remove latch...");
        UnbindFromCompound(fromBlock);
        UnbindFromCompound(toBlock);
        GameObject.Destroy(compoundParent);
    }

    private static void UnbindFromCompound(GameObject block)
    {
        block.transform.parent = null;
        var rigidbody = block.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }
}