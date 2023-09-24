using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MagneticSnapper : MonoBehaviour
{
    public Material snappingMaterial;
    public GameObject shadowBlock;
    private GameObject thisBlock;
    private Material defaultMaterial;
    private Renderer meshRenderer;
    private GameObject shadowBlockMagnetAlignmentHandle;
    private Transform oppositeMagnetTransform = null;


    void Start()
    {
        thisBlock = transform.parent.gameObject;
        meshRenderer = thisBlock.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;
        shadowBlockMagnetAlignmentHandle = shadowBlock.transform.parent.gameObject;
        Debug.Log("Default material: " + defaultMaterial);

        LinkBlocksTogether(thisBlock, shadowBlock);
    }

    void Update()
    {
        if (oppositeMagnetTransform != null)
        {
            MoveMagnetAlignmentHandleToFaceMagnet(oppositeMagnetTransform);
        }
    }

    void LinkBlocksTogether(GameObject thisBlock, GameObject otherBlock)
    {
        var thisFixedJoint = thisBlock.GetComponent<FixedJoint>();
        var shadowRigidbody = shadowBlock.GetComponent<Rigidbody>();
        thisFixedJoint.connectedBody = shadowRigidbody;
        shadowRigidbody.useGravity = true;
        shadowBlock.GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Entered trigger");
        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) {
            Debug.Log("I am the greatest");
            var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
            Debug.Log("Signed angle: " + signedAngle);
            if (Math.Abs(signedAngle) > 165f) {
                oppositeMagnetTransform = collider.gameObject.transform;
                transform.parent.gameObject.GetComponent<Renderer>().enabled = false;
                shadowBlock.GetComponent<Renderer>().enabled = true;
                SetMaterial(snappingMaterial);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");
        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) 
        {
            Debug.Log("I am still the greatest, but am leaving now...");
            oppositeMagnetTransform = null; // NB could theoretically get multiple collisions... but not if blocks and magnets physically prevent it
            transform.parent.gameObject.GetComponent<Renderer>().enabled = true;
            shadowBlock.GetComponent<Renderer>().enabled = false;
            SetMaterial(defaultMaterial);
        }
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
