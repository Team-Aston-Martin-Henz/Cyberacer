using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
    public AudioSource collisionSound;
    public int groundLayerNo = 6;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        // collision with the ground should not play any sound
        if (other.gameObject.layer != groundLayerNo) 
        {
            collisionSound.Stop();
            collisionSound.pitch = Random.Range(0.6f, 1.2f);
            collisionSound.Play();
        }
    }
}
