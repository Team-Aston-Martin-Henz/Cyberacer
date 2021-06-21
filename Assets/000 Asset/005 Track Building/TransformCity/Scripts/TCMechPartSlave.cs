using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gb = TCMechStatic;

//IL.ranch, ILonion32@gmail.com, 2020.
public class TCMechPartSlave : MonoBehaviour
{
    public TCMechInitState _TCMechInitState;
    Animator _Animator;
    bool NextState;
    bool PrevState;

    void Awake()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
    }

    void Start()
    {
        _Animator = GetComponent<Animator>();

        //initial state
        if (_TCMechInitState._StructureState == TCMechInitState.StructureState.Opened)
        {
            _Animator.Play("Base Layer.Back", 0, 1.0f); 
            NextState = true;
        }
        else
        {
            _Animator.Play("Base Layer.Move", 0, 1.0f);
            NextState = false;
        }
        PrevState = gb.ChangeCurrentState;
    }

    void Update()
    {
        if (gb.ChangeCurrentState != PrevState)
        {
            StopCoroutine("ChangeStateScatter");
            StartCoroutine("ChangeStateScatter");
            PrevState = gb.ChangeCurrentState;
            NextState = !NextState;
        }
    }

    IEnumerator ChangeStateScatter()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1.5f));
        if (!NextState)
        {
            _Animator.CrossFade("Base Layer.Move", 0.1f);
        }
        else
        {
            _Animator.CrossFade("Base Layer.Back", 0.1f);
        }
    }
}
