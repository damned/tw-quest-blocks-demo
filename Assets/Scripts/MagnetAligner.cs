using UnityEngine;

public class MagnetAligner
{
    public GameObject CreateAlignmentHandle(GameObject magnet)
    {
        var handle = new GameObject();
        var blockTransform = magnet.transform.parent;
        handle.name = "HandleFor" + blockTransform.name;
        
        handle.transform.position = magnet.transform.position;
        handle.transform.rotation = magnet.transform.rotation;

        blockTransform.parent = handle.transform;

        return handle;
    }
}
