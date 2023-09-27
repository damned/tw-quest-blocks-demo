using System;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MagnetAligner))]
public class MagneticSnapper : MonoBehaviour
{
    public Material snappingMaterial;
    public GameObject shadowBlock;
    public GameObject shadowBlockMagnet;
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
    private MagneticSnapper otherMagnetScript = null;
    private MagneticSnapper greaterMagnetBackReference = null;
    private bool isGrabbed;


    void Start()
    {
        Debug.Log("magnet instance id: " + gameObject.GetInstanceID());
        thisBlock = transform.parent.gameObject;
        meshRenderer = thisBlock.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;
        thisMagnetCollider = GetComponent<Collider>();

        if (shadowBlock != null)
        {
            Debug.Log("Using manually rigged shadow block for: " + thisBlock.name);
            shadowBlockMagnetAlignmentHandle = shadowBlock.transform.parent.gameObject;
        }
        else
        {
            if (shadowBlockMagnet == null)
            {
                Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);

            }
            shadowBlock = shadowBlockMagnet.transform.parent.gameObject;

            Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);
            shadowBlockMagnetAlignmentHandle = GetComponent<MagnetAligner>().CreateAlignmentHandle(shadowBlockMagnet);
        }
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
        if (gameObject.GetInstanceID() < collider.gameObject.GetInstanceID()) {
            Debug.Log("I am the greatest: " + thisBlock.name);
            var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
            Debug.Log("Signed angle: " + signedAngle);
            if (Math.Abs(signedAngle) > 165f)
            {
                otherMagnetCollider = collider;
                otherMagnet = collider.gameObject;

                Debug.Log("setting otherMagnetTransform due to alignment in trigger enter");
                Debug.Log("otherMagnet: " + otherMagnet);
                otherMagnetTransform = otherMagnet.transform;

                if (IsNotGrabbedByBothHands(otherMagnet)) {
                    Debug.Log("hands free attraction -> immediate latch");
                    ShowRealBlock(true);
                    ShowShadowBlock(false);
                    SnapAndLatchToOtherBlock();
                    Debug.Log("Latched");
                }
                else
                {
                    // start snapping = make shadow block visible instead of this block
                    ShowRealBlock(false);
                    ShowShadowBlock(true);

                    MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
                }
            }
        }
    }

    private bool IsNotGrabbedByBothHands(GameObject otherMagnet)
    {
        return !(IsGrabbed() && MagnetScriptOf(otherMagnet).IsGrabbed());
    }


    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");

        if (gameObject.GetInstanceID() < collider.gameObject.GetInstanceID()) 
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
            Debug.Log("clearing otherMagnetTransform after exiting trigger");
            otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
            otherMagnet = null;
            otherMagnetCollider = null;
        }
    }

    void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
        isGrabbed = true;
        if (fixedJoint != null)
        {
            Debug.Log("grabbed and there's already a latch from this block - i should be the greater: " + thisBlock.name);
            if (otherMagnetScript == null)
            {
                Debug.LogWarning("Latched weirdness - no stored reference to otherMagnetScript");
                return;
            }
            if (!otherMagnetScript.HasGreaterMagnetBackReference())
            {
                Debug.LogWarning("Latched weirdness - stored other magnet but has no back reference");
                return;
            }
            if (otherMagnetScript.IsGrabbed())
            {
                Debug.Log("Unlatching as other block is also currently grabbed");
                UnlatchOtherBlock();
            }
            else
            {
                Debug.Log("Not unlatching as other block not currently grabbed");
            }
        }
        else 
        {
            if (HasGreaterMagnetBackReference()) 
            {
                Debug.Log("Got a back reference to greater magnet - i should be the lesser: " + thisBlock.name);
                if (greaterMagnetBackReference.IsGrabbed()) {
                    Debug.Log("Unlatching via greater as other block is also currently grabbed");
                    greaterMagnetBackReference.UnlatchOtherBlock();
                }
            }
        }
    }

    bool IsGrabbed()
    {
        return isGrabbed;
    }


    MagneticSnapper MagnetScriptOf(GameObject otherMagnet)
    {
        return otherMagnet.GetComponent<MagneticSnapper>();
    }

    void OnRelease()
    {
        Debug.Log("Released: " + thisBlock.name);
        isGrabbed = false;
        if (otherMagnetTransform == null)
        {
            Debug.Log("Oh, i don't have a reference to other magnet - either free or latched");
        }
        else
        {
            Debug.Log("I do have a reference to other magnet: attempt latch and hide shadow block");
            ShowRealBlock(true);
            ShowShadowBlock(false);
            ReleaseGrabOnThisBlock();
            SnapAndLatchToOtherBlock();
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

    private void ReleaseGrabOnThisBlock()
    {
        var grabber = thisBlock.GetComponent<XRGrabInteractable>();
        grabber.enabled = false;
        grabber.enabled = true;
    }

    private void SnapAndLatchToOtherBlock()
    {
        Debug.Log("Latching to other block, otherMagnetTransform: " + otherMagnetTransform.gameObject.name);
        SnapThisBlockToOther(thisBlock, otherMagnetTransform);

        var otherBlock = otherMagnetTransform.parent.gameObject;

        // disable magnet colliders so don't keep on rehashing all this
        thisMagnetCollider.enabled = false;
        otherMagnetCollider.enabled = false;

        LatchThisBlockToOtherBlock(thisBlock, otherBlock);

        // save information for unlatching purposes
        // - otherMagnetCollider retained to be re-enabled after unlatching
        // - otherMagnetScript retained for querying/updating other magnet
        otherMagnetScript = MagnetScriptOf(otherMagnet);
        otherMagnetScript.SetLatchBackReference(this);

        Debug.Log("clearing otherMagnetTransform after latch: " + thisBlock.name);
        otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
        otherMagnet = null;
        Debug.Log("cleared, otherMagnetTransform = " + otherMagnetTransform);
    }

    void SetLatchBackReference(MagneticSnapper greaterMagnetScript)
    {
        greaterMagnetBackReference = greaterMagnetScript;
    }

    bool HasGreaterMagnetBackReference()
    {
        return greaterMagnetBackReference != null;
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
