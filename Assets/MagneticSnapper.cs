using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MagneticSnapper : MonoBehaviour
{
    public Material snappingMaterial;
    public GameObject shadowBlock;
    private GameObject thisBlock;
    private Collider thisMagnetCollider;
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;

    private Collider otherMagnetCollider = null;
    private GameObject otherMagnet = null;
    private Transform otherMagnetTransform = null;




    void Start()
    {
        thisBlock = transform.parent.gameObject;
        meshRenderer = thisBlock.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;
        thisMagnetCollider = GetComponent<Collider>();
        shadowBlockMagnetAlignmentHandle = shadowBlock.transform.parent.gameObject;
        Debug.Log("Default material: " + defaultMaterial);
    }

    void Update()
    {
        if (otherMagnetTransform != null)
        {
            MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        }
    }

    // here for reference
    void UnlinkBlocks(GameObject thisBlock, GameObject otherBlock)
    {
        var shadowRigidbody = shadowBlock.GetComponent<Rigidbody>();
        shadowRigidbody.useGravity = false;
        shadowBlock.GetComponent<Collider>().enabled = false;
        var thisFixedJoint = thisBlock.GetComponent<FixedJoint>(); // nb there will be one per magnet
        Destroy(thisFixedJoint);
    }

    void LinkThisBlockToOtherBlock(GameObject thisBlock, GameObject otherBlock)
    {
        var fixedJoint = otherBlock.AddComponent<FixedJoint>(); // nb there will be one per magnet
        var thisRigidbody = thisBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = thisRigidbody;
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
                meshRenderer.enabled = false;
                shadowBlock.GetComponent<Renderer>().enabled = true;

                MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
            }
        }
    }

    private void LatchToBlockRemovingShadow(GameObject otherMagnet, Transform otherMagnetTransform)
    {
        // release the grab on this block
        //
        // NB needs re-enabling later
        //
        thisBlock.GetComponent<XRGrabInteractable>().enabled = false;
        SnapThisBlockToOther(thisBlock, otherMagnetTransform);

        var otherBlock = otherMagnetTransform.parent.gameObject;

        // disable magnet colliders so don't keep on rehashing all this
        thisMagnetCollider.enabled = false;
        otherMagnet.GetComponent<Collider>().enabled = false;

        LinkThisBlockToOtherBlock(thisBlock, otherBlock);
        shadowBlock.SetActive(false);
    }


    void SnapThisBlockToOther(GameObject thisBlock, Transform otherMagnetTransform)
    {
        MoveShadowMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        thisBlock.transform.position = shadowBlock.transform.position;
        thisBlock.transform.rotation = shadowBlock.transform.rotation;
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");

        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) 
        {
            Debug.Log("I am still the greatest, but am leaving now...");
            if (otherMagnetTransform == null)
            {
                Debug.Log("Oh, i don't have a reference to other magnet");
            }
            else {
                Debug.Log("I do have a reference to other magnet");
                meshRenderer.enabled = true;
                LatchToBlockRemovingShadow(otherMagnet, otherMagnetTransform);
                otherMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
                otherMagnetCollider = null;
                otherMagnet = null;
            }
        }
    }

    void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
    }

    void OnRelease()
    {
        Debug.Log("Released: " + thisBlock.name);
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
}
