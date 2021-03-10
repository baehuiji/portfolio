//플레이어가 가리키고 있는 아이템의 아웃라인을 생성하는 클래스입니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineUpdate : MonoBehaviour
{
    public Camera Camera;

    private GameObject target = null;

    //public LayerMask layerMask;
    private int layermask;

    public DrawOutline_HJ OutlineController;
    public int index;
    private bool isPointing = false; //아이템을 마우스포인터로 가리키는 상태
    private bool isSelected = false; //아이템을 클릭한 상태

    void Start()
    {
        layermask = 1 << LayerMask.NameToLayer("Item");
    }

    void Update()
    {
        //가리킨 타겟 콜라이드
        if (TargetCollideTouchpoint())
            isPointing = true;
        else
        {
            if (isPointing)
            {
                OutlineController.set_enabled(index, false);
                isPointing = false;
                target = null;
            }
        }

        //클릭 상태
        if (Input.GetMouseButtonDown(0))
        {
            isSelected = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isSelected = false;
        }
    }

    // 아이템을 가리키고 있는지 Ray를 이용한 검사 
    public bool TargetCollideTouchpoint()
    {
        GameObject hitObj = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30, layermask))
        {
            hitObj = hit.collider.gameObject;

            if (!GameObject.Equals(target, hitObj))
            {
                target = hitObj;

                Item tempitem = target.GetComponent<Item>(); ;
                index = tempitem.itemCode;

                OutlineController.set_enabled(index, true); //가리키고 있는 상태
            }

            return true;
        }

        return false;
    }
}
