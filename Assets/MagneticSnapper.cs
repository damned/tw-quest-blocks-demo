using System;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MagneticSnapper : MonoBehaviour
{
    public Material snappingMaterial;
    public GameObject shadowBlock;
    public bool debugMode = false;

    private GameObject thisBlock;
    private Collider thisMagnetCollider;
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;

    private Collider otherMagnetCollider = null;
    private GameObject otherMagnet = null;
    private Transform otherMagnetTransform = null; // NB null if free or latched
    private FixedJoint fixedJoint = null;

    void Start()
    {
        thisBlock = transform.parent.gameObject;
        meshRenderer = thisBlock.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;
        thisMagnetCollider = GetComponent<Collider>();
        shadowBlockMagnetAlignmentHandle = shadowBlock.transform.parent.gameObject;
        InitDebugMode();
        Debug.Log("Default material: " + defaultMaterial);
    }

    void Update()
    {
        if (otherMagnetTransform != null)
        {
            MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Entered trigger");
        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) {
            Debug.Log("I am the greatest: " + thisBlock.name);
            var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
            Debug.Log("Signed angle: " + signedAngle);
            if (Math.Abs(signedAngle) > 165f)
            {
                otherMagnetCollider = collider;
                otherMagnet = collider.gameObject;
                otherMagnetTransform = otherMagnet.transform;

                // make shadow block visible instead of this block
                ShowRealBlock(false);
                ShowShadowBlock(true);

                MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");

        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) 
        {
            Debug.Log("I am still the greatest, but am leaving now...");
            if (otherMagnetTransform != null)
            {
                Debug.Log("I do have a reference to other magnet: hide shadow, show real block");
                ShowRealBlock(true);
                ShowShadowBlock(false);
            }
            else
            {
                Debug.Log("Oh, i don't have a reference to other magnet");
            }
            otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
            otherMagnet = null;
            otherMagnetCollider = null;
        }
    }

    void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
        if (fixedJoint != null)
        {
            Debug.Log("grabbed and there's already a latch from this block");
            UnlatchOtherBlock();
        }
    }

    void OnRelease()
    {
        Debug.Log("Released: " + thisBlock.name);
        if (otherMagnetTransform == null)
        {
            Debug.Log("Oh, i don't have a reference to other magnet");
        }
        else
        {
            Debug.Log("I do have a reference to other magnet: attempt latch and hide shadow block");
            ShowRealBlock(true);
            ShowShadowBlock(false);

            LatchToBlock(otherMagnet, otherMagnetTransform);
        }
    }

    private void ShowShadowBlock(bool showBlock)
    {
        shadowBlock.GetComponent<Renderer>().enabled = showBlock;
    }

    private void ShowRealBlock(bool showBlock)
    {
        meshRenderer.enabled = showBlock;
    }

    private void LatchToBlock(GameObject otherMagnet, Transform otherMagnetTransform)
    {
        // release the grab on this block
        //
        // NB needs re-enabling later
        //
        var grabber = thisBlock.GetComponent<XRGrabInteractable>();
        grabber.enabled = false;
        grabber.enabled = true;
        SnapThisBlockToOther(thisBlock, otherMagnetTransform);

        var otherBlock = otherMagnetTransform.parent.gameObject;

        // disable magnet colliders so don't keep on rehashing all this
        thisMagnetCollider.enabled = false;
        otherMagnetCollider.enabled = false;

        LatchThisBlockToOtherBlock(thisBlock, otherBlock);

        otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
        otherMagnet = null;
        // otherMagnetCollider retained for re-enablement if unlatched
    }

    // here for reference
    void UnlatchOtherBlock()
    {
        Debug.Log("Unlatching...");
        if (fixedJoint == null)
        {
            Debug.Log("Cannot unlatch - no fixed joint to unlatch");
            return;
        }

        if (fixedJoint.connectedBody == null)
        {
            Debug.LogWarning("Unlatch weirdness - fixed joint has no connected body");
        }

        // nb there will be one per magnet
        Destroy(fixedJoint);
        fixedJoint = null;

        if (otherMagnetCollider == null)
        {
            Debug.LogWarning("Unlatch weirdness - otherMagnetCollider not set");
        }

        // this should re-trigger snapping if magnet colliders overlap
        thisMagnetCollider.enabled = true;
        otherMagnetCollider.enabled = true;

        otherMagnetCollider = null;
        Debug.Log("Unlatched");
    }

    void LatchThisBlockToOtherBlock(GameObject thisBlock, GameObject otherBlock)
    {
        fixedJoint = thisBlock.AddComponent<FixedJoint>(); // nb there will be one per magnet
        var otherRigidbody = otherBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = otherRigidbody;
    }

    void SnapThisBlockToOther(GameObject thisBlock, Transform otherMagnetTransform)
    {
        MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        thisBlock.transform.position = shadowBlock.transform.position;
        thisBlock.transform.rotation = shadowBlock.transform.rotation;
    }

    private void MoveShadowMagnetAlignmentHandleToFaceMagnet(Transform magnetTransform)
    {
        shadowBlockMagnetAlignmentHandle.transform.position = magnetTransform.position;
        RotateTransformToFaceReferenceTransform(shadowBlockMagnetAlignmentHandle.transform, magnetTransform);
    }

    private void RotateTransformToFaceReferenceTransform(Transform transform, Transform referenceTransform)
    {
        transform.rotation = referenceTransform.rotation;
        transform.Rotate(0, 180f, 0);
    }

    private void InitDebugMode()
    {
        shadowBlockMagnetAlignmentHandle.GetComponentsInChildren<MeshRenderer>()
            .ToList().ForEach(r => r.enabled = debugMode);
    }
}
