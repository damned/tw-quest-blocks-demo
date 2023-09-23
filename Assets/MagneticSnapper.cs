using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MagneticSnapper : MonoBehaviour
{
    public Material snappingMaterial;
    public GameObject shadowBlock;
    private Material defaultMaterial;
    private Renderer meshRenderer;


    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = transform.parent.gameObject.GetComponent<Renderer>();
        defaultMaterial = meshRenderer.material;
        Debug.Log("Default material: " + defaultMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Entered trigger");
        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) {
            Debug.Log("I am the greatest");
            var signedAngle = Vector3.SignedAngle(transform.forward, collider.transform.forward, Vector3.up);
            Debug.Log("Signed angle: " + signedAngle);
            if (Math.Abs(signedAngle) > 165f) {
                SetMaterial(snappingMaterial);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log("Exited trigger");
        if (gameObject.GetInstanceID() > collider.gameObject.GetInstanceID()) 
        {
            Debug.Log("I am the greatest");
            SetMaterial(defaultMaterial);        
        }
    }

    private void SetMaterial(Material material)
    {
        var materialsCopy = meshRenderer.materials;
        materialsCopy[0] = material;
        meshRenderer.materials = materialsCopy;
    }
}
