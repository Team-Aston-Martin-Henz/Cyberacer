using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//IL.ranch, ILonion32@gmail.com, 2020.
public class TCMechInitState : MonoBehaviour
{
    public enum StructureState
    {
        Opened = 0,
        Closed = 1,
    }
    [Header("Start State of Structure:")]
    public StructureState _StructureState = StructureState.Opened;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
