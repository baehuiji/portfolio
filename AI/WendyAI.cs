//유한상태기계를 이용한 클래스입니다.
//유니티의 네비메쉬를 이용해서 이동의 제한을 두었으며, 플레이어를 따라다니는 AI입니다.
//Wendy의 상태는 Idle, Move, Play가 있습니다.
//Play는 단순히 애니메이션만 반복하는 상태이며, 주로 Idle과 Move 간의 상태전이가 일어납니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WendyAI : MonoBehaviour
{
    public GameObject _modeling;
    private Animator _animator;

    //Wendy 설정값
    public Transform _playerTrans;
    private Vector3 _offset;
    private Vector3 _spherePos;
    public float _radius;
    private Vector3 _rot_dir; //이동할때의 회전 방향
    public float speed;
    Vector3 curPos;
    float footSpeed = 2;

    private NavMeshAgent _agent;
    private IState _current_state; //현재상태
    int cost;                      //이동 코스트

    [SerializeField]
    private bool _contact = false;       //플레이어와 접촉
    private bool _activewendy = false;   //플레이어와 최초 접촉 검사

    private Coroutine idleCoroutine;
    bool _giveIdleCommand = false; //코루틴 한번만 호출하기 위함
    
    private Coroutine ChangeFromMoveState;
    bool _giveMoveCommand = false;

    private void Awake()
    {
        _animator = _modeling.GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _rot_dir = Vector3.zero;

        //웬디의 상태 초기화 : 지하실에서 놀고 있는 애니메이션
        SetState(new Wendy_PlayState());
    }

    private void Update()
    {
        if (_activewendy)
            _current_state.Update();
    }

    // 실질적으로 상태를 변화시키는 함수
    public void SetState(IState nextState)
    {
        if (_current_state != null)
        {
            _current_state.OnExit();
        }

        _current_state = nextState;
        _current_state.OnEnter(this);
    }

    // 애니메이션 처리
    public void SetClearAni() //2스테이지를 클리어했을때
    {
        _animator.SetBool("Clear2Stage", true); //상태 Play->Idle
    }
    public void SetIdleAni()
    {
        _animator.SetBool("IsWalking", false);
    }
    public void SetWalkAni()
    {
        _animator.SetBool("IsWalking", true);
    }

    // 2스테이지 배치퍼즐 클리어 직후, Play -> Idle로 상태 첫 변화
    //  : 웬디는 플레이어의 인식범위 밖에 있음 (지하실)
    public void ClearLayoutPuzzle()
    {
        if (_current_state.GetStateNum() == WendyState.Play)
        {
            SetClearAni();
            SetState(new Wendy_IdleState());
        }
    }

    public void SetContactWithPlayer(bool b)
    {
        _contact = b;
    }
    public bool GetContact()
    {
        return _contact;
    }

    // 플레이어의 인식범위를 트리거로 검색, 근처에 있는 상태(Contct)를 변경함
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SetContactWithPlayer(true); //플레이어가 접촉, 근처에 있는 상태

            if (!_activewendy) //최초로 플레이어와 닿았을때
            {
                _activewendy = true; //활성화
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_activewendy)
                SetContactWithPlayer(false);
        }
    }

    // Idle -> Move로 상태 변화 
    //  : 플레이어의 인식범위에 Wendy가 들어감
    IEnumerator ChangeFromIdleState()
    {
        _giveIdleCommand = true;

        yield return new WaitForSeconds(3f);

        SetState(new Wendy_MoveState());
        _giveIdleCommand = false;
        idleCoroutine = null;
    }
    public void StartIdleCoroutine()
    {
        if (_giveIdleCommand) return;

        idleCoroutine = StartCoroutine(ChangeFromIdleState());
    }
    public bool GetIdleCommandState()
    {
        return _giveIdleCommand;
    }
    public void StopIdleCoroutine()
    {
        if (_giveIdleCommand)
        {
            StopCoroutine(idleCoroutine);

            _giveIdleCommand = false;
            idleCoroutine = null;

            SetState(new Wendy_MoveState());
        }
    }

    // 플레이어의 근방 랜덤한 위치를 지정해서 Wendy의 목적지로 잡음
    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 tempDir = Random.insideUnitSphere;
        float range = Random.Range(1, dist);
        Vector3 randDirection = tempDir.normalized * range;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, 10, layermask);
        //cost = 1; //IndexFromMask(navHit.mask);

        return navHit.position;
    }

    // 목적지와 Wendy 위치 범위 오차
    private bool WithinRange(Vector3 to, Vector3 from) 
    {
        float distanceToTarget = Vector3.Distance(to, from);
        float distanceThreshold = 0.1f;
        if (distanceToTarget <= distanceThreshold)
        {
            return true;
        }
        return false;
    }

    public void StartMovemntCoroutine()
    {
        if (_giveMoveCommand) return;

        ChangeFromMoveState = StartCoroutine(ChangeFromMoveState());
    }

    // Wendy의 이동을 멈춤
    public void StopMovemntCoroutine()
    {
        if (_giveMoveCommand)
        {
            StopCoroutine(ChangeFromMoveState);

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;

            _giveMoveCommand = false;
            ChangeFromMoveState = null;

            SetState(new Wendy_IdleState());
        }
    }

    // 플레이어에게 웬디가 다가가면 움직임을 멈춤
    public void CollideWithPlayer()
    {
        if (_current_state.GetStateNum() == WendyState.Move)
        {
            StopMovemntCoroutine();
        }
    }

    // Move -> Idle로 상태가 변화
    //  : WithinRange함수를 이용해 일정수준 이하로 목적지와 가까워지면 변화
    IEnumerator ChangeFromMoveState() //ChangeFromMoveState
    {
        _giveMoveCommand = true;

        gameObject.GetComponent<NavMeshAgent>().enabled = true;
        _agent.isStopped = false;
        Vector3 unpausedSpeed = Vector3.zero;
        curPos = transform.position;

        _spherePos = _playerTrans.position;
        Vector3 newPos = RandomNavSphere(_spherePos, _radius, -1);

        float step = speed * Time.deltaTime; //회전
        
        _agent.SetDestination(newPos); //도착지

        // TO DO : 오브젝트 사이 끼이는 버그가 있음, 해결 필요
        // > 한지역에 일정시간 머물때 초기화시킴
        float trappedTime = 0.0f;
        Vector3 prePos = Vector3.zero;
        prePos = transform.position;

        while (!WithinRange(newPos, curPos))
        {
            //Wendy의 회전
            if (_agent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                Vector3 temp = _agent.velocity.normalized;
                _rot_dir = new Vector3(temp.x, 0f, temp.z);
                transform.rotation = Quaternion.LookRotation(
                                     Vector3.RotateTowards(transform.forward, _rot_dir, step, 0.0f));
            }

            curPos = transform.position;

            //Wendy의 이동 속도
            _agent.speed = footSpeed; //GetNavMeshCost();
            unpausedSpeed = _agent.velocity;

            if (transform.position == prePos)
            {
                trappedTime += Time.deltaTime;
                if (trappedTime > 1f)
                {
                    break;
                }
            }
            else
            {
                trappedTime = 0f;
            }
            prePos = transform.position;

            yield return null;
        }

        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        _giveMoveCommand = false;
        movementCoroutine = null;

        SetState(new Wendy_IdleState());
    }
    
    float GetNavMeshCost()
    {
        return cost;
    }

    int IndexFromMask(int mask)
    {
        for (int i = 0; i < 32; ++i)
        {
            if ((1 << i & mask) != 0)
            {
                return i;
            }
        }
        return -1;
    }
}
