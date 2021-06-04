using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
    public AudioSource collisionSFX;
    public int groundLayerNo = 6;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        // collision with the ground is not valid collision, should not play any sound
        if (other.gameObject.layer != groundLayerNo) 
        {
            collisionSFX.Stop();
            collisionSFX.pitch = Random.Range(0.6f, 1.2f);
            collisionSFX.Play();
        }
    }
}
