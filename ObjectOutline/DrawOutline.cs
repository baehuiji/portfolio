//아웃라인을 그리는 클래스입니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOutline : MonoBehaviour
{
    public List<EasyOutlineSystem> systems;

    public bool check = false;

    void Start()
    {
        systems = new List<EasyOutlineSystem>();
        EasyOutlineSystem[] tempSystems = FindObjectsOfType<EasyOutlineSystem>();
        for (int i = 0; i < tempSystems.Length; i++)
        {
            systems.Add(tempSystems[i]);
        }

        //systems.Sort(new SortComparerForOutline());
        systems.Sort(
            delegate (EasyOutlineSystem a, EasyOutlineSystem b) { return a.index.CompareTo(b.index); });

    }

    public void set_enabled(int index, bool enabled)
    {
        systems[index].enabled = enabled;
    }

    public void set_destroy(int index)
    {
        EasyOutlineSystem tempOutline = systems[index];
        systems.RemoveAt(index);

        Destroy(tempOutline);
    }

    public void set_check(bool b)
    {
        check = b;
    }
    public bool get_outline_okay() // 외곽선이 안띄워져있을때 false
    {
        return check;
    }
}
