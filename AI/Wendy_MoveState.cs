using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wendy_MoveState : IState
{
    private WendyAI _wendy;
    private int _state_num;

    private bool _contact;
    private bool _start_coroutine;

    void IState.OnEnter(WendyAI wendy)
    {
        //wendy 프로퍼티 초기화
        this._wendy = wendy;

        _state_num = WendyState.Move;
        _contact = _wendy.GetContact();
        _start_coroutine = false;

        if (_contact)
        {
            _wendy.SetState(new Wendy_IdleState()); //플레이어가 근처에 있으면 움직일 필요 없음
            return;
        }
        else
        {
            _wendy.SetWalkAni();       
            _wendy.StartMovemntCoroutine(); //플레이어가 멀리 있으면, 이동할 위치를 지정함
        }
    }

    void IState.Update()
    {

    }

    void IState.OnExit()
    {
        _wendy.StopMovemntCoroutine(); //StopCDCoroutine();
        _wendy.SetIdleAni();
    }

    int IState.GetStateNum()
    {
        return _state_num;
    }
}
