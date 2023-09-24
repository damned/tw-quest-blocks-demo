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
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;
    private Transform otherMagnetTransform = null;
    private Collider thisMagnetCollider;


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
            MoveMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        }
    }

    void UnlinkBlocks(GameObject thisBlock, GameObject otherBlock)
    {
        var shadowRigidbody = shadowBlock.GetComponent<Rigidbody>();
        shadowRigidbody.useGravity = false;
        shadowBlock.GetComponent<Collider>().enabled = false;
        var thisFixedJoint = thisBlock.GetComponent<FixedJoint>(); // nb there will be one per magnet
        Destroy(thisFixedJoint);
    }

    void LinkBlocks(GameObject thisBlock, GameObject otherBlock)
    {
        var thisFixedJoint = thisBlock.AddComponent<FixedJoint>(); // nb there will be one per magnet
        var shadowRigidbody = shadowBlock.GetComponent<Rigidbody>();
        thisFixedJoint.connectedBody = shadowRigidbody;
        shadowRigidbody.useGravity = true;
        shadowBlock.GetComponent<Collider>().enabled = true;
    }

    void LinkShadowBlockToOtherBlock(GameObject shadowBlock, GameObject otherBlock)
    {
        var fixedJoint = otherBlock.AddComponent<FixedJoint>(); // nb there will be one per magnet
        var shadowRigidbody = shadowBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = shadowRigidbody;
        shadowRigidbody.useGravity = true;
        shadowBlock.GetComponent<Collider>().enabled = true;
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
            if (Math.Abs(signedAngle) > 165f) {
                var otherMagnet = collider.gameObject;
                var otherMagnetTransform = otherMagnet.transform;

                // this essentially sets state to attracting
                // this.oppositeMagnetTransform = oppositeMagnetTransform;

                // make shadow block visible instead of this block
                // thisBlock.GetComponent<Renderer>().enabled = false;
                // shadowBlock.GetComponent<Renderer>().enabled = true;

                // usurp this block
                // thisBlock.SetActive(false);

                // drop this block
                thisBlock.GetComponent<XRGrabInteractable>().enabled = false;
                SnapThisBlockToOther(thisBlock, otherMagnetTransform);

                var otherBlock = otherMagnetTransform.parent.gameObject;

                // disable magnet colliders so don't keep on rehashing all this
                thisMagnetCollider.enabled = false;
                otherMagnet.GetComponent<Collider>().enabled = false;

                LinkThisBlockToOtherBlock(thisBlock, otherBlock);
                shadowBlock.SetActive(false);

                // debug
                // SetMaterial(snappingMaterial);
            }
        }
    }

    void SnapThisBlockToOther(GameObject thisBlock, Transform otherMagnetTransform)
    {
        MoveMagnetAlignmentHandleToFaceMagnet(otherMagnetTransform);
        thisBlock.transform.position = shadowBlock.transform.position;
        thisBlock.transform.rotation = shadowBlock.transform.rotation;
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");

        // disabled as just doing one-shot
        // if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) 
        // {
        //     Debug.Log("I am still the greatest, but am leaving now...");
        //     oppositeMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
        //     transform.parent.gameObject.GetComponent<Renderer>().enabled = true;
        //     shadowBlock.GetComponent<Renderer>().enabled = false;
        //     SetMaterial(defaultMaterial);
        // }
    }

    void OnGrab()
    {
        Debug.Log("Grabbed: " + thisBlock.name);
    }

    void OnRelease()
    {
        Debug.Log("Released: " + thisBlock.name);
    }

    private void SetMaterial(Material material)
    {
        var materialsCopy = meshRenderer.materials;
        materialsCopy[0] = material;
        meshRenderer.materials = materialsCopy;
    }

    private void MoveMagnetAlignmentHandleToFaceMagnet(Transform magnetTransform)
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
