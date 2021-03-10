//인벤토리 정보 클래스입니다.
//슬롯을 관리하며, 인덱스로 관리하여 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    static public Inventory instance;

    [SerializeField]
    private GameObject go_InventoryBase;

    [SerializeField]
    private GameObject go_SlotsParent;

    private Slot[] slots;

    private bool remove_count = false;
    public bool Remove_Count { get { return remove_count; } } 

    void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>();
    }

    // 인벤토리에 습득이 가능한 아이템이면, 슬롯에 추가
    public bool AcquireItem(Item _item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Item.ItemType.Read != _item.itemType && Item.ItemType.Equipment != _item.itemType)
            {
                if (slots[i].item == null)
                {
                    slots[i].AddItem(_item);
                    return true;
                }
            }
        }
        return false;
    }

    // 아이템 사용시, 슬롯에 있던 아이템정보를 잃음
    public void UseupItem(Item _item)
    {        
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                if (slots[i].item == _item)
                {
                    slots[i].RemoveItem();
                    remove_count = true;
                    return;
                }
                else
                {
                    remove_count = false;
                }
            }
            remove_count = false;
        }

    }

    // 아이템 사용시, 필요한 아이템의 정보를 전달
    public GameObject get_Item(int index)
    {
        return slots[index].item.itemPrefab;
    }
    public Item get_ItemInfo(int index) 
    {
        return slots[index].item;
    }
    public int get_ItemCode(int index) 
    {
        if (slots[index].item == null)
            return -1;
        return slots[index].item.itemCode;
    }

    // 지정 슬롯 초기화
    public void clear_Slot(int index) 
    {
        slots[index].RemoveItem();
    }

    // 슬롯의 상태 전달
    public bool IsVoid_Slot(int index)
    {
        return slots[index].IsVoid();
    }

    // *3스테이지 퍼즐에 필요 
    public void set_Item_layoutIndex(int index, int layout) // 아이템 장식장 위치 설정하기
    {
        slots[index].set_layoutIndex(layout);
    }
}








