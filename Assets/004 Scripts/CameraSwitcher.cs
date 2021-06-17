using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher instance;
    
    // the array of cameras to switch in between
    public GameObject[] cameras;
    private int currentCam;

    public CameraController topDownCam;
    public Cinemachine.CinemachineVirtualCamera cineCam;

    private void Awake()
    {
        instance = this;
    }


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

    public void SetTarget(CarController playerCar)
    {
        topDownCam.target = playerCar;
        cineCam.m_Follow = playerCar.transform;
        cineCam.m_LookAt = playerCar.transform;
    }
}