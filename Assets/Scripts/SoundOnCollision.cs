using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
    public AudioSource soundToPlay;
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
        //  play collision effect is allowed if player is not on ground
        if (other.gameObject.layer != groundLayerNo) 
        {
            soundToPlay.Stop();
            soundToPlay.pitch = Random.Range(0.6f, 1.2f);
            soundToPlay.Play();
        }
    }
}
