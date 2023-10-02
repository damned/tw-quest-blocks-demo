using System;
using System.Linq;

using UnityEngine;

public class Magnet : MonoBehaviour
{
    public Material shadowMaterial;
    public bool debugMode = false;

    private GameObject thisBlock;
    private Collider thisMagnetCollider;
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;
    private GameObject shadowBlock;
    private GameObject shadowBlockMagnet;

    private Transform otherMagnetTransform = null; // NB null if free or latched
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

        shadowBlockMagnet = shadowCreator.CreateShadowBlock(gameObject);
        shadowBlock = shadowBlockMagnet.transform.parent.gameObject;

        Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);
        shadowBlockMagnetAlignmentHandle = magnetAligner.CreateAlignmentHandle(shadowBlockMagnet);

        InitDebugMode();
        Debug.Log("Default material: " + defaultMaterial);
    }

    void Update()
    {
        if (IsSnapping())
        {
            MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Entered trigger: " + thisBlock.name);
        if (IsThisDecidingMagnet(gameObject, collider.gameObject))
        {
            Debug.Log("I am the decider: " + thisBlock.name);
            if (ShouldAttract(collider))
            {
                var otherMagnet = collider.gameObject;
                otherMagnetTransform = otherMagnet.transform;

                if (AtLeastOneBlockIsGrabbed(otherMagnet))
                {
                    Debug.Log("one or more blocks grabbed - start snapping");
                    StartSnapping(otherMagnet);
                }
                else
                {
                    Debug.Log("hands free attraction -> immediate latch");
                    ImmediateLatch(otherMagnet);
                }
            }
        }
    }

    private bool ShouldAttract(Collider collider)
    {
        var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
        Debug.Log("Signed angle: " + signedAngle);
        bool shouldAttract = Math.Abs(signedAngle) > 165f;
        return shouldAttract;
    }

    private void StartSnapping(GameObject otherMagnet)
    {
        otherMagnetTransform = otherMagnet.transform;
        ShowRealBlock(false);
        ShowShadowBlock(true);

        MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
    }


    private void ImmediateLatch(GameObject otherMagnet)
    {
        otherMagnetTransform = otherMagnet.transform;
        ShowRealBlock(true);
        ShowShadowBlock(false);
        SnapAndLatchToOtherBlock();
        Debug.Log("Latched");
    }

    private bool AtLeastOneBlockIsGrabbed(GameObject otherMagnet)
    {
        Debug.Log("other magnet: " + otherMagnet.name);
        var otherMagnetScript = MagnetScriptOf(otherMagnet);

        Debug.Log("other magnet script: " + otherMagnetScript);
        return IsBlockGrabbed() || otherMagnetScript.IsBlockGrabbed();
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");
        if (IsSnapping())
        {
            ShowRealBlock(true);
            ShowShadowBlock(false);
        }
        else
        {
            Debug.Log("Oh, i don't have a reference to other magnet");
        }
        otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
    }

    private bool IsSnapping()
    {
        return otherMagnetTransform != null;
    }

    public void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
        if (LatchEnd.IsLatched())
        {
            if (LatchEnd.IsOtherBlockGrabbed())
            {
                Debug.Log("Unlatching as other block is also currently grabbed");
                LatchEnd.Unlatch();
            }
            else
            {
                Debug.Log("Not unlatching as other block not currently grabbed");
            }
        }
    }

    public bool IsBlockGrabbed()
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
        if (IsSnapping())
        {
            Debug.Log("I do have a reference to other magnet: attempt latch and hide shadow block");
            ShowRealBlock(true);
            ShowShadowBlock(false);
            SnapAndLatchToOtherBlock();
        }
        else
        {
            Debug.Log("Not snapping so release without latching");
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

        LatchEnd.LatchTo(otherMagnetTransform.GetComponent<Magnet>().LatchEnd);

        Debug.Log("clearing otherMagnetTransform (snapping) after latch: " + thisBlock.name);
        otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
    }

    public void OnLatch()
    {
        Debug.Log("turning off magnet collider at " + transform.localPosition + " on " + thisBlock.name);
        thisMagnetCollider.enabled = false;
    }

    public void OnUnlatch()
    {
        Debug.Log("turning on magnet collider at " + transform.localPosition + " on " + thisBlock.name);
        thisMagnetCollider.enabled = true;
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
