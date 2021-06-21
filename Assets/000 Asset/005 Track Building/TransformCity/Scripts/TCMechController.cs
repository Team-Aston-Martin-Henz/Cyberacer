using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//IL.ranch, ILonion32@gmail.com, 2020.
public class TCMechController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 150, 50), "Make Transformation"))
        {
            TCMechStatic.ChangeCurrentState = !TCMechStatic.ChangeCurrentState;
            Debug.Log("Transformer City transformation...");
        }
        if (GUI.Button(new Rect(50, 120, 150, 50), "Open Documentation"))
        {
            Application.OpenURL("https://ilonion.com/transcity_documentation");
        }
    }
}
