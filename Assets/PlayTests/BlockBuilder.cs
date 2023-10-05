using UnityEngine;

public class BlockBuilder
{
    public static GameObject CreateMagnetAndParentBlock()
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var rigidbody = block.AddComponent<Rigidbody>();
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        block.AddComponent<MagneticBlock>();

        var magnet = CreateMagnetOn(block);
        magnet.transform.Translate(magnet.transform.forward * 0.5f);

        return magnet;
    }

    public static GameObject CreateMagnetOn(GameObject block)
    {
        var magnet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        magnet.layer = LayerMask.NameToLayer("Magnets");
        magnet.GetComponent<Collider>().isTrigger = true;

        magnet.AddComponent<Magnet>();
        magnet.transform.parent = block.transform;
        magnet.transform.localScale = Vector3.one * 0.3f;
        return magnet;
    }
}