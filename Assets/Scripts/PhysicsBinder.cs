using UnityEngine;

public class PhysicsBinder : LatchBinder
{
    private GameObject fromBlock;
    private GameObject toBlock;
    private FixedJoint fixedJoint;

    public void Apply(LatchEnd fromEnd, LatchEnd toEnd)
    {
        fromBlock = fromEnd.Block;
        toBlock = toEnd.Block;
        Debug.Log("adding fixedjoint to bind objects for latch...");
        fixedJoint = fromBlock.AddComponent<FixedJoint>();
        var toRigidbody = toBlock.GetComponent<Rigidbody>();
        fixedJoint.connectedBody = toRigidbody;
    }

    public void Destroy()
    {
        Debug.Log("destroying fixedjoint to unbind and remove latch...");
        GameObject.Destroy(fixedJoint);
        fixedJoint = null;
    }
}