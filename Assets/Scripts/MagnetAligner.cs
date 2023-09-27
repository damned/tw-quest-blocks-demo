using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetAligner : MonoBehaviour
{

    void Start()
    {

    }

    void Update()
    {

    }
    
    public GameObject CreateAlignmentHandle(GameObject magnet)
    {
        var handle = new GameObject();
        var blockTransform = magnet.transform.parent;
        
        handle.transform.position = magnet.transform.position;
        handle.transform.rotation = magnet.transform.rotation;

        blockTransform.parent = handle.transform;

        return handle;
    }
}
