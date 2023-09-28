using UnityEngine;

public class MagneticBlock : MonoBehaviour
{
    private bool isGrabbed = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGrab()
    {
        isGrabbed = true;
    }

    public void OnRelease()
    {
        isGrabbed = false;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }
}
