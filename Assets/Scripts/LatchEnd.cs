using UnityEngine;

public class LatchEnd : MonoBehaviour
{
    public GameObject Block
    {
        get {
            return block;
        }
    }

    private Latch latch = null;
    private bool isInitiator = false;
    private GameObject block;

    void Start()
    {
        block = transform.parent.gameObject;
    }

    void Update()
    {
        
    }

    public bool IsLatched()
    {
        if (latch != null)
        {
            return true;
        }
        return false;
    }

    public void LatchTo(LatchEnd other)
    {
        isInitiator = true;
        other.isInitiator = false;

        latch = PhysicsLatch.LatchBetween(this, other);
        other.latch = latch;
    }

    public void Unlatch()
    {
        latch.Destroy();
    }

    public void OnUnlatch()
    {
        latch = null;
    }

    public bool IsInitiator()
    {
        return isInitiator;
    }

}
