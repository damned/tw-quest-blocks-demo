using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class KinematicsBinder : LatchBinder
{
    private GameObject fromBlock;
    private GameObject toBlock;
    private GameObject compoundParent;

    public void Apply(LatchEnd fromEnd, LatchEnd toEnd)
    {
        Debug.Log("creating kinematics binder: creating kinematics compound...");
        fromBlock = fromEnd.Block;
        toBlock = toEnd.Block;

        compoundParent = new GameObject();
        var compound = compoundParent.AddComponent<KinematicsCompound>();
        compound.Init();

        compound.AddBlock(fromBlock);
        compound.AddBlock(toBlock);

        compound.CommitUpdates();
    }

    public void Destroy()
    {
        Debug.Log("removing compound parent to unbind and remove latch...");
        UnbindFromCompound(fromBlock);
        UnbindFromCompound(toBlock);
        GameObject.Destroy(compoundParent);
    }

    private static void UnbindFromCompound(GameObject block)
    {
        block.transform.parent = null;
        var rigidbody = block.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }
}