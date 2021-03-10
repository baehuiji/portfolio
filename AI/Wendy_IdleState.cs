using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wendy_IdleState : IState
{
    private WendyAI _wendy;
    private int _state_num;

    private bool _contact;
    private bool _start_coroutine;

    void IState.OnEnter(WendyAI wendy)
    {
        //wendy 프로퍼티 초기화
        this._wendy = wendy;

        _state_num = WendyState.Idle;
        _contact = _wendy.GetContact();
        _start_coroutine = false;

        _wendy.SetIdleAni();

        if (!_contact)
        {
            _wendy.StartIdleCoroutine();
            _start_coroutine = true;
        }
    }

    void IState.Update()
    {
        _contact = _wendy.GetContact();
        if (!_contact)
        {
            _wendy.StartIdleCoroutine(); //플레이어와 멀리 떨어져있을때 움직임
            _start_coroutine = true;
        }
        else //다시 플레이어가 근처였을때 코루틴 정지
        {
            _wendy.StopIdleCoroutine();
            _start_coroutine = false;
        }
    }

    void IState.OnExit()
    {
        _wendy.StopIdleCoroutine();
        _start_coroutine = false;
    }

    int IState.GetStateNum()
    {
        return _state_num;
    }
}