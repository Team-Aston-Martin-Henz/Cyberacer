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
        //  when pressed on the key letter "C"
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentCam++;

            if (currentCam >= 2)
            {
                currentCam = 0;
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                if (i == currentCam)
                {
                    cameras[i].SetActive(true);
                }
                else
                {
                    cameras[i].SetActive(false);
                }
            }
        }
    }
}
