//현재 선택한 슬롯의 정보를 가진 클래스입니다.
//이전에 가리켰던 슬롯의 위치, 현재의 위치를 가지고 있습니다.
//마우스휠을 이용해 위아래로 움직이며, 움직임을 위해 프로그레스를 사용합니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SelectSlot : MonoBehaviour
{
    private Vector3 lastTargetPosition = Vector3.zero;
    private Vector3 currentTargetPosition = Vector3.zero;
    private float currentLerpDistance = 0.0f; //progress, 진행백분율

    public Transform[] slotPos;
    public int cur_index = 0;
    public int last_index = 0;

    //스크롤 상태
    public bool scrollEnd = true;
    public bool scrollStart = false;

    private float movingSpeedOrTime = 0.2f; //프로그레스에 쓰임

    public bool select_EndSlot = false;

    //휠 오브젝트
    public GameObject Up_Wheel; 
    public GameObject Down_Wheel;

    //휠의 변화될 이미지
    public Sprite Upsprite;
    public Sprite UPLsprite;
    public Sprite Downsprite;
    public Sprite DownLsprite;

    void Start()
    {
        lastTargetPosition = slotPos[cur_index].position;     
        currentTargetPosition = slotPos[last_index].position;
        currentLerpDistance = 0.0f;
    }

    void LateUpdate() 
    {
        if (scrollEnd)
            return;

        // 위치 업데이트
        transform.position = Vector3.Lerp(slotPos[last_index].position, slotPos[cur_index].position, currentLerpDistance);
        currentLerpDistance += Time.deltaTime / movingSpeedOrTime;
        if (currentLerpDistance > 1.2f && !select_EndSlot)
        {
            reset_slotPos();
        }

        // 이미지 변경
        if (last_index < cur_index)
        {
            Down_Wheel.GetComponent<Image>().sprite = DownLsprite;
        }
        if (cur_index < last_index)
        {
            Up_Wheel.GetComponent<Image>().sprite = UPLsprite;
        }
    }

    // 움직일 방향을 받아 선택슬롯의 애니메이션을 실행
    public void Set_slotPos_index(int dir)
    {
        if (!scrollStart || select_EndSlot)
        {
            scrollStart = true;
            scrollEnd = false;

            //지난 인덱스 위치 업데이트
            last_index = cur_index;
            lastTargetPosition = currentTargetPosition;

            //현재 인덱스 위치 업데이트
            select_EndSlot = false;
            cur_index += dir;

            //인덱스 제한 (0~10)
            if (cur_index < 0)
            {
                select_EndSlot = true;
                cur_index = 0;
                currentLerpDistance = 1.0f;
            }
            else if (cur_index > 10)
            {
                select_EndSlot = true;
                cur_index = 10;
                currentLerpDistance = 1.0f;
            }

            //움직임을 위해 프로그레스 퍼센트를 0으로 지정
            if (last_index != cur_index)
            {
                currentLerpDistance = 0.0f;
            }

            //타겟이 되는 위치 지정
            currentTargetPosition = slotPos[cur_index].position;
        }
    }

    // 초기화
    public void reset_slotPos()
    {
        scrollStart = false;
        scrollEnd = true;
        //select_EndSlot = false;

        Up_Wheel.GetComponent<Image>().sprite = Upsprite;
        Down_Wheel.GetComponent<Image>().sprite = Downsprite;
    }

    // 현재 가리키고 있는 인덱스 위치를 전달
    public int get_index()
    {
        return cur_index;
    }
}
