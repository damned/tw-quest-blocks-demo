using Codice.CM.Client.Differences.Graphic;
using UnityEngine;

public class ShadowCreator
{
    public GameObject CreateShadowBlock(GameObject magnet)
    {
        var block = magnet.transform.parent.gameObject;
     
        Debug.Log("Copying block to use as shadow block: " + block.name);
     
        var shadowBlock = GameObject.Instantiate(block);

        shadowBlock.name = block.name + " shadow";

        Debug.Log("Created shadow block at: " + shadowBlock.transform.position);

        var magnetScript = magnet.GetComponent<MagneticSnapper>();

        var shadowRenderer = shadowBlock.GetComponent<MeshRenderer>();
        if (magnetScript.shadowMaterial != null)
        {
            shadowRenderer.material = magnetScript.shadowMaterial;
        }
        shadowRenderer.enabled = magnetScript.debugMode;
        shadowBlock.GetComponent<Collider>().enabled = false;
        shadowBlock.GetComponent<Rigidbody>().useGravity = false;

        var shadowMagnetScripts = shadowBlock.GetComponentsInChildren<MagneticSnapper>();
        if (shadowMagnetScripts.Length > 1) {
            Debug.LogError("multiple magnets dynamic rigging not yet supported");
        }

        var shadowMagnetScript = shadowMagnetScripts[0];
        shadowMagnetScript.enabled = false; // don't make this active or we'll continually start more and more shadow blocks!

        var shadowMagnet = shadowMagnetScript.gameObject;
        shadowMagnet.GetComponent<Collider>().enabled = false;

        return shadowMagnet;
    }
}
