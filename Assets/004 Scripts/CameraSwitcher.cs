using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // the array of cameras to switch in between
    public GameObject[] cameras;
    private int currentCam;

    void Start()
    {
        
    }


    void Update()
    {   
        // press `C` to switch camera
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameras[currentCam].SetActive(false);
            currentCam = ++currentCam % cameras.Length;
            cameras[currentCam].SetActive(true);
        }
    }
}