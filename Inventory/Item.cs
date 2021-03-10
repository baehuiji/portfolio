//아이템 정보 클래스입니다.
//중복되지 않는 아이템코드를 가집니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New item", menuName = "New item/item")]
public class Item : ScriptableObject
{
    public int itemCode;            //아이템 식별코드
    public int layoutIndex;         //*3스테이지 퍼즐에 필요한 인덱스
    public string itemName;
    public ItemType itemType;
    public Sprite itemImage;
    public GameObject itemPrefab; 

    public enum ItemType
    {
        Equipment,  //손전등
        Used,
        Read,       //쪽지(힌트,스토리)       
        Puzzle,     
        Doll,
        Onetime,   //1회성아이템
    }
}




