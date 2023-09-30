using System;
using System.Linq;

using UnityEngine;

public class Magnet : MonoBehaviour
{
    public GameObject shadowBlock;
    public GameObject shadowBlockMagnet;
    public Material shadowMaterial;
    public bool debugMode = false;

    private GameObject thisBlock;
    private Collider thisMagnetCollider;
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;

    private Collider otherMagnetCollider = null;
    private GameObject otherMagnet = null;
    private Transform otherMagnetTransform = null; // NB null if free or latched
    private Magnet otherMagnetScript = null;
    private Magnet greaterMagnetBackReference = null;
    private MagneticBlock magneticBlock;

    private ShadowCreator shadowCreator = new ShadowCreator();
    private MagnetAligner magnetAligner = new MagnetAligner();
    
    private LatchEnd _latchEnd;
    private LatchEnd LatchEnd {
        get {
            return _latchEnd;
        }
    }

    public bool IsLatched { 
        get {
            return LatchEnd.IsLatched();
        } 
    }


    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }


    void Start()
    {
        _latchEnd = gameObject.AddComponent<LatchEnd>();

        Debug.Log("magnet instance id: " + gameObject.GetInstanceID());
        thisBlock = transform.parent.gameObject;
        magneticBlock = thisBlock.GetComponent<MagneticBlock>();
        if (magneticBlock == null)
        {
            throw new Exception("cannot Start magnet: magnetic block parent of magnets needs a MagneticBlock script");
        }

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
                shadowBlockMagnet = shadowCreator.CreateShadowBlock(gameObject);
            }
            shadowBlock = shadowBlockMagnet.transform.parent.gameObject;

            Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);
            shadowBlockMagnetAlignmentHandle = magnetAligner.CreateAlignmentHandle(shadowBlockMagnet);
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
        if (IsThisDecidingMagnet(gameObject, collider.gameObject))
        {
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

                if (IsNotGrabbedByBothHands(otherMagnet))
                {
                    Debug.Log("hands free or singled hand attraction -> immediate latch");
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
        Debug.Log("other magnet: " + otherMagnet.name);
        var otherMagnetScript = MagnetScriptOf(otherMagnet);

        Debug.Log("other magnet script: " + otherMagnetScript);
        return !(IsBlockGrabbed() && otherMagnetScript.IsBlockGrabbed());
    }


    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");

        if (IsThisDecidingMagnet(gameObject, collider.gameObject)) 
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

    public void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
        if (LatchEnd.IsLatched() && LatchEnd.IsInitiator())
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
            if (otherMagnetScript.IsBlockGrabbed())
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
                if (greaterMagnetBackReference.IsBlockGrabbed()) {
                    Debug.Log("Unlatching via greater as other block is also currently grabbed");
                    greaterMagnetBackReference.UnlatchOtherBlock();
                }
            }
        }
    }

    bool IsBlockGrabbed()
    {
        return magneticBlock.IsGrabbed();
    }


    Magnet MagnetScriptOf(GameObject otherMagnet)
    {
        return otherMagnet.GetComponent<Magnet>();
    }

    public void OnRelease()
    {
        Debug.Log("Released: " + thisBlock.name);
        if (otherMagnetTransform == null)
        {
            Debug.Log("Oh, i don't have a reference to other magnet - either free or latched");
        }
        else
        {
            Debug.Log("I do have a reference to other magnet: attempt latch and hide shadow block");
            ShowRealBlock(true);
            ShowShadowBlock(false);
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

    private void SnapAndLatchToOtherBlock()
    {
        Debug.Log("Latching to other block, otherMagnetTransform: " + otherMagnetTransform.gameObject.name);
        SnapThisBlockToOther(thisBlock, otherMagnetTransform);

        var otherBlock = otherMagnetTransform.parent.gameObject;

        // disable magnet colliders so don't keep on rehashing all this
        thisMagnetCollider.enabled = false;
        otherMagnetCollider.enabled = false;

        LatchThisBlockToOtherBlock(otherMagnetTransform.GetComponent<Magnet>().LatchEnd);

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

    void SetLatchBackReference(Magnet greaterMagnetScript)
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
        if (!LatchEnd.IsLatched())
        {
            Debug.Log("Cannot unlatch - latch end reckons not latched");
            return;
        }

        LatchEnd.Unlatch();

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

    void LatchThisBlockToOtherBlock(LatchEnd otherLatchEnd)
    {
        LatchEnd.LatchTo(otherLatchEnd);
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

    private static bool IsThisDecidingMagnet(GameObject thisMagnet, GameObject otherMagnet)
    {
        return thisMagnet.GetInstanceID() < otherMagnet.GetInstanceID();
    }
}
