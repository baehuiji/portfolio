using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnEnter(WendyAI wendy);

    void Update();

    void OnExit();

    int GetStateNum(); //현재 상태를 넘겨줌
}
