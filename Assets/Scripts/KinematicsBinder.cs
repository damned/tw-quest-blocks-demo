using UnityEngine;

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

        var compound = fromBlock.GetComponentInParent<KinematicsCompound>();
        if (compound != null)
        {
            compoundParent = compound.gameObject;
            compound.AddBlock(toBlock);
        }
        else
        {
            compound = toBlock.GetComponentInParent<KinematicsCompound>();
            if (compound != null)
            {
                compoundParent = compound.gameObject;
                compound.AddBlock(fromBlock);
            }
            else
            {
                compoundParent = new GameObject();
                compound = compoundParent.AddComponent<KinematicsCompound>();
                compound.Init();
                compound.AddBlock(fromBlock);
                compound.AddBlock(toBlock);
            }
        }
        compound.Commit();
    }

    public void Destroy()
    {
        Debug.Log("removing compound parent to unbind and remove latch...");
        UnbindFromCompound(fromBlock);
        UnbindFromCompound(toBlock);
        GameObject.Destroy(compoundParent);
    }

    // TODO move to compound, add back XR grab etc.
    private static void UnbindFromCompound(GameObject block)
    {
        block.transform.parent = null;
        var rigidbody = block.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }
}