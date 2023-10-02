using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class KinematicsCompound : MonoBehaviour
{
    private const float TOLERANCE = 0.01f;
    private Dictionary<GameObject, GameObject> controllerToGrabbedBlock = new Dictionary<GameObject, GameObject>();
    private XRGrabInteractable compoundInteractable;


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

        compoundInteractable = gameObject.AddComponent<XRGrabInteractable>();
        compoundInteractable.throwOnDetach = false;
        compoundInteractable.useDynamicAttach = true;
    }

    public void Commit()
    {
        ReRegister();
    }

    public void ReRegister()
    {
        Unregister();
        Register();
    }

    private void Register()
    {
        var interactable = compoundInteractable.GetComponent<IXRInteractable>();
        compoundInteractable.interactionManager.RegisterInteractable(interactable);
    }

    private void Unregister()
    {
        var interactable = compoundInteractable.GetComponent<IXRInteractable>();
        compoundInteractable.interactionManager.UnregisterInteractable(interactable);
    }

    public void AddBlock(GameObject block)
    {
        RemovePriorRigidbodyAndXrComponents(block);
        block.transform.parent = transform;
        compoundInteractable.colliders.Add(block.GetComponent<Collider>());
    }

    public void OnGrab(SelectEnterEventArgs enterEvent)
    {
        var interactor = enterEvent.interactorObject;
        var controller = ControllerOf(interactor);
        Pose attachPose = enterEvent.interactableObject.GetAttachPoseOnSelect(interactor);
        Debug.Log("grab: on compound grab with [" + controller.name +  "] attach pose: " + attachPose);
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform blockTransform = transform.GetChild(i);
            var block = blockTransform.gameObject;
            var collider = block.GetComponent<Collider>();

            if (collider != null && Vector3.Distance(attachPose.position, collider.ClosestPoint(attachPose.position)) < TOLERANCE)
            {
                Debug.Log("grab: Found grabbed block: " + block.name);
                if (controllerToGrabbedBlock.ContainsKey(controller))
                {
                    Debug.LogWarning("compound grab for controller [" + controller.name + "] already has a grabbed block assigned against it: " + controllerToGrabbedBlock[controller].name);
                }
                controllerToGrabbedBlock[controller] = block;
            }
        }
    }

    public void OnRelease(SelectExitEventArgs exitEvent)
    {
        var interactor = exitEvent.interactorObject;
        var controller = ControllerOf(interactor);
        Debug.Log("release: on compound release with [" + controller.name +  "]");
        if (controllerToGrabbedBlock.ContainsKey(controller))
        {
            var releasedBlock = controllerToGrabbedBlock[controller];
            controllerToGrabbedBlock.Remove(controller);
            Debug.Log("release: Found released block: " + releasedBlock);
        }
        else
        {
            Debug.LogWarning("compound release for controller [" + controller.name + "] has no grabbed block associated with it");
        }
    }

    private static GameObject ControllerOf(IXRSelectInteractor interactor)
    {
        return interactor.transform.parent.gameObject;
    }

    private static void RemovePriorRigidbodyAndXrComponents(GameObject block)
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
