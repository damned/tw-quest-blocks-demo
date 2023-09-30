using UnityEngine;

public class ShadowCreator
{
    private const float FLOAT_TOLERANCE = 0.001f;


    public GameObject CreateShadowBlock(GameObject magnet)
    {
        var block = magnet.transform.parent.gameObject;
     
        Debug.Log("Copying block to use as shadow block: " + block.name);
     
        var shadowBlock = GameObject.Instantiate(block);

        shadowBlock.name = block.name + " shadow";

        Debug.Log("Created shadow block at: " + shadowBlock.transform.position);

        var magnetScript = magnet.GetComponent<Magnet>();

        var shadowRenderer = shadowBlock.GetComponent<MeshRenderer>();
        if (magnetScript.shadowMaterial != null)
        {
            shadowRenderer.material = magnetScript.shadowMaterial;
        }
        shadowRenderer.enabled = magnetScript.debugMode;
        shadowBlock.GetComponent<Collider>().enabled = false;
        shadowBlock.GetComponent<Rigidbody>().useGravity = false;

        Magnet shadowMagnetScript = null;
        foreach (var anyShadowMagnetScript in shadowBlock.GetComponentsInChildren<Magnet>())
        {
            anyShadowMagnetScript.enabled = false; // disable scripts or we'll continually start more and more shadow blocks!
            anyShadowMagnetScript.GetComponent<Collider>().enabled = false;
            if (Vector3.Distance(magnet.transform.localPosition, anyShadowMagnetScript.transform.localPosition) < FLOAT_TOLERANCE)
            {
                if (shadowMagnetScript == null)
                {
                    Debug.Log("Cool found a magnet in shadow block that matches original magnet position");
                    shadowMagnetScript = anyShadowMagnetScript;
                }
                else
                {
                    Debug.LogWarning("Found more than one magnet at same position - duplicate magnets in block?");
                }
            }
        }

        var shadowMagnet = shadowMagnetScript.gameObject;
        return shadowMagnet;
    }
}
