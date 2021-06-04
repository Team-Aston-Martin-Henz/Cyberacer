using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    //  an array of cameras
    public GameObject[] cameras;
    private int currentCam;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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
