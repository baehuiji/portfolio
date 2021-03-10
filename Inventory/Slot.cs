//인벤토리의 슬롯 클래스입니다.
//습득한 아이템을 저장하거나, 슬롯의 아이템을 없애는 역할을 합니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Item item;
    public Image itemImage;

    private bool voidState = true; //슬롯의 상태
    public bool IsVoid() { return voidState; }

    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // 슬롯에 아이템 추가
    public void AddItem(Item _item)
    {
        item = _item;
        itemImage.sprite = item.itemImage;
        SetColor(1);
        voidState = false;
    }

    // 슬롯을 초기화
    public void RemoveItem()
    {
        item = null;
        itemImage.sprite = null;
        SetColor(0);
        voidState = true;
    }

    // *3스테이지 퍼즐에 필요
    public void set_layoutIndex(int index)
    {
        item.layoutIndex = index;
    }
}

