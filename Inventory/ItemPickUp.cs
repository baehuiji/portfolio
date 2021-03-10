using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Item item;
    public int outlineIndex; // *아웃라인에 필요

    public int get_itemCode()
    {
        return item.itemCode;
    }

    // *3스테이지 퍼즐에 필요
    public int get_layoutIndex()
    {
        return item.layoutIndex;
    }
}
