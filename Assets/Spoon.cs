using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spoon : MonoBehaviour
{
    public AudioSource audioSource;

    private System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Pot")
        {
            audioSource.PlayOneShot(audioSource.clip, 0.2f + random.Next(80) * 0.01f);
        }
    }
}
