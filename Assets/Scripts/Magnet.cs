using System;
using System.Linq;

using UnityEngine;

public class Magnet : MonoBehaviour
{
    struct Shadow
    {
        public Shadow(GameObject shadowBlockMagnet, GameObject shadowBlockMagnetAlignmentHandle)
        {
            if(shadowBlockMagnet.transform.parent.gameObject == null || shadowBlockMagnetAlignmentHandle == null){
                Debug.LogError("Shadow should not have null references");
            }
            magnet = shadowBlockMagnet;
            block = magnet.transform.parent.gameObject;
            magnetAlignmentHandle = shadowBlockMagnetAlignmentHandle;
        }

        public GameObject magnetAlignmentHandle { get; }
        public GameObject block { get; }
        public GameObject magnet { get; }
    }

    public Material shadowMaterial;
    public bool debugMode = false;

    private GameObject thisBlock;
    private Collider thisMagnetCollider;

    private Collider MagnetCollider {
        get {
            if(thisMagnetCollider == null){
                thisMagnetCollider = GetComponent<Collider>();
            }
            return thisMagnetCollider;
        }
    }

    private Material defaultMaterial;
    private Renderer meshRenderer;
    private Shadow? shadow;

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

    private T GetOrAddComponent<T>() where T: Component {
        var comp = gameObject.GetComponent<T>();
        if(comp == null){
            comp = gameObject.AddComponent<T>();
        }
        return comp;
    }

    void Start()
    {
        MagnetCollider.isTrigger = true;

        _latchEnd = GetOrAddComponent<LatchEnd>();
        gameObject.layer = LayerMask.NameToLayer("Magnets");

        DebugLog("magnet instance id: {0}", gameObject.GetInstanceID());
        thisBlock = transform.parent.gameObject;
        magneticBlock = thisBlock.GetComponent<MagneticBlock>();
        if (magneticBlock == null)
        {
            throw new Exception("cannot Start magnet: magnetic block parent of magnets needs a MagneticBlock script");
        }

        meshRenderer = thisBlock.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;

        DebugLog("Default material: {0}", defaultMaterial);
    }

    private void DebugLog(string format, params object[] args)
    {
        if (debugMode)
        {
            Debug.Log(string.Format(format, args), this);
        }
    }

    private Shadow InitShadowBlock()
    {
        var shadowBlockMagnet = shadowCreator.CreateShadowBlock(gameObject);

        Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);
        var shadowBlockMagnetAlignmentHandle = magnetAligner.CreateAlignmentHandle(shadowBlockMagnet);

        return new Shadow(shadowBlockMagnet, shadowBlockMagnetAlignmentHandle);
    }

    [SerializeField] private bool _isOverlapping;
    private bool _lastOverlappingStatus;



    void Update()
    {
        if (IsSnapping())
        {
            MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        }
    }

    void FixedUpdate()
    {
        if(!_isOverlapping && _lastOverlappingStatus)
        {
            Debug.Log("Exited trigger");
            if (IsSnapping())
            {
                Debug.Log("stopping snapping");
                ShowRealBlock(true);
                ShowShadowBlock(false);
            }
            else
            {
                Debug.Log("Oh, i don't have a reference to other magnet");
            }
            otherMagnetTransform = null;
        }
        _lastOverlappingStatus = _isOverlapping;
        _isOverlapping = false;
    }

    void OnTriggerStay(Collider collider)
    {
        _isOverlapping = true;
        if (IsThisDecidingMagnet(gameObject, collider.gameObject))
        {
            DebugLog("trigger stay: I am the decider: {0}", thisBlock.name);
            if (IsBlockInACompound() && IsBlockInACompound(collider.gameObject))
            {
                DebugLog("trigger stay: bailing as we're both compounds and that is not supported");
                return;
            }

            if (ShouldAttract(collider))
            {
                var otherMagnetObject = collider.gameObject;
                var (fromMagnet, toMagnet) = DetermineFromToMagnets(otherMagnetObject);

                if (fromMagnet.AtLeastOneBlockIsGrabbed(toMagnet.gameObject))
                {
                    DebugLog("one or more blocks grabbed - start snapping");
                    fromMagnet.StartSnapping(toMagnet.gameObject);
                }
                else
                {
                    DebugLog("hands free attraction -> immediate latch");
                    fromMagnet.ImmediateLatch(toMagnet.gameObject);
                }
            }
        }
    }

    private (Magnet fromMagnet, Magnet toMagnet) DetermineFromToMagnets(GameObject otherMagnetObject)
    {
        Magnet otherMagnet = otherMagnetObject.GetComponent<Magnet>();
        if (IsBlockInACompound())
        {
            return (otherMagnet, this);
        }
        return (this, otherMagnet);
    }

    private bool IsBlockInACompound()
    {
        return magneticBlock.IsInACompound();
    }

    private static bool IsBlockInACompound(GameObject otherMagnetGameObject)
    {
        var otherMagnet = otherMagnetGameObject.GetComponent<Magnet>();
        if (otherMagnet == null)
        {
            return false;
        }
        return otherMagnet.IsBlockInACompound();
    }

    private bool IsBlockALeafInACompound()
    {
        return magneticBlock.IsALeafInACompound();
    }

    private bool ShouldAttract(Collider collider)
    {
        // var crossProduct = Vector3.Cross(transform.forward, collider.transform.forward);
        var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
        //Debug.Log("Signed angle: " + signedAngle);
        bool shouldAttract = Math.Abs(signedAngle) > 165f;
        // bool shouldAttract = (1f - Math.Abs(crossProduct.y)) < 0.2f;
        return shouldAttract;
    }

    private void StartSnapping(GameObject otherMagnet)
    {
        if (otherMagnetTransform == null)
        {
            PlaySnapSound();
        }
        otherMagnetTransform = otherMagnet.transform;
        ShowRealBlock(false);
        ShowShadowBlock(true);

        MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
    }


    public void ImmediateLatch(GameObject otherMagnet)
    {
        otherMagnetTransform = otherMagnet.transform;
        ShowRealBlock(true);
        ShowShadowBlock(false);
        SnapAndLatchToOtherBlock();
        //Debug.Log("Latched");
    }

    public bool AtLeastOneBlockIsGrabbed(GameObject otherMagnet)
    {
        //Debug.Log("other magnet: " + otherMagnet.name);
        var otherMagnetScript = MagnetScriptOf(otherMagnet);

        //Debug.Log("other magnet script: " + otherMagnetScript);
        return IsBlockGrabbed() || otherMagnetScript.IsBlockGrabbed();
    }

    private bool IsSnapping()
    {
        return otherMagnetTransform != null;
    }

    public void OnGrab()
    {
        //Debug.Log("Grabbed: " + thisBlock.name);
        if (LatchEnd.IsLatched())
        {
            if (LatchEnd.IsOtherBlockGrabbed())
            {
                //Debug.Log("Unlatching as other block is also currently grabbed");
                LatchEnd.Unlatch();
            }
            else
            {
                //Debug.Log("Not unlatching as other block not currently grabbed");
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
        EnsureShadowExists();
        DebugLog("Making shadow block {0} visible", shadow.Value.block);
        var renderers = shadow.Value.block.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = showBlock;
        }
    }

    private void ShowRealBlock(bool showBlock)
    {
        meshRenderer.enabled = showBlock;
    }

    private void SnapAndLatchToOtherBlock()
    {
        DebugLog("Latching to other block, otherMagnetTransform: {0}", otherMagnetTransform.gameObject.name);
        SnapThisBlockToOther(thisBlock, otherMagnetTransform);

        LatchEnd.LatchTo(otherMagnetTransform.GetComponent<Magnet>().LatchEnd);

        DebugLog("clearing otherMagnetTransform (snapping) after latch: {0}", thisBlock.name);
        otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
    }

    public void OnLatch()
    {
        Debug.Log("turning off magnet collider at " + transform.localPosition + " on " + thisBlock.name);
        MagnetCollider.enabled = false;
    }

    private void PlaySnapSound()
    {
        var audioSource = GetComponent<AudioSource>();
        if (audioSource)
        {
            audioSource.PlayOneShot(audioSource.clip, 0.5f);
        }
    }

    public void OnUnlatch()
    {
        Debug.Log("turning on magnet collider at " + transform.localPosition + " on " + thisBlock.name);
        MagnetCollider.enabled = true;
    }

    void SnapThisBlockToOther(GameObject thisBlock, Transform otherMagnetTransform)
    {
        MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        EnsureShadowExists();
        thisBlock.transform.position = shadow.Value.block.transform.position;
        thisBlock.transform.rotation = shadow.Value.block.transform.rotation;
    }

    void EnsureShadowExists()
    {
        if (!shadow.HasValue)
        {
            var shadowBlockMagnet = shadowCreator.CreateShadowBlock(gameObject);
            Debug.Log("Dynamically rigging shadow block alignment handle for: " + thisBlock.name);
            var shadowBlockMagnetAlignmentHandle = magnetAligner.CreateAlignmentHandle(shadowBlockMagnet);
            shadow = new Shadow(shadowBlockMagnet, shadowBlockMagnetAlignmentHandle);
            InitDebugMode(shadow.Value, debugMode);
        }
    }

    private void MoveShadowMagnetAlignmentHandleToFaceMagnet(Transform magnetTransform)
    {
        EnsureShadowExists();
        shadow.Value.magnetAlignmentHandle.transform.position = magnetTransform.position;
        RotateTransformToFaceReferenceTransform(shadow.Value.magnetAlignmentHandle.transform, magnetTransform);
    }

    private void RotateTransformToFaceReferenceTransform(Transform transform, Transform referenceTransform)
    {
        transform.rotation = referenceTransform.rotation;
        transform.Rotate(0, 180f, 0);
    }

    private static void InitDebugMode(Shadow shadow, bool debugMode)
    {
        shadow.magnetAlignmentHandle.GetComponentsInChildren<MeshRenderer>()
            .ToList().ForEach(r => r.enabled = debugMode);
    }

    private static bool IsThisDecidingMagnet(GameObject thisMagnet, GameObject otherMagnet)
    {
        return thisMagnet.GetInstanceID() < otherMagnet.GetInstanceID();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.01f);
    }

}
