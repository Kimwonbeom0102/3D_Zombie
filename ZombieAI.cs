using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    // 첫째로 해주어야 할 것들  => 좀비의 상태, 특성 정리
    public enum ZombieState // 좀비의 상태를 관리
    {
        Patrol, Chase, Attack, Idle, Die
    }

    public ZombieState currentState; // 현재의 좀비상태를 줄 것

    public enum ZombieType  // 좀비의 타입을 정해줌 => 좀비가 아닌 다른 매개체로 구분이 가능합니다. (일반 시민, 지나가는 경찰 등)
    {
        ZombieType1, ZombieType2, ZombieType3
    }

    public ZombieType zombieType;
    public float hp = 10; // 기본체력( 변수로 만들어둔 ZombieType마다 다르게 생성해 줄 변수)
    public int damage = 10;

    public Transform[] patrolPoints; // 순찰 지점들
    private Transform player; // 플레이어의 Transform
    public float detectionRange = 10f; // AI가 플레이어를 감지하는 범위

    public float attackRange = 2.0f; // 공격범위  < 여러가지 방법. 1.cast를 사용한다(계속 쏴주어야하는 단점) 2.앞쪽에 collision을 놓아준다 3.코드를짜준다.
    public Transform handTransform; // 좀비의 손 위치 (공격) 여러가지 방법으로 플레이어를 공격하게 유도
    private bool isAttacking; // 공격 중인지를 나타내는 변수
    public float sphereCastRadius = 0.5f; // cast반경 (collide)

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private Animator animator;

    public LayerMask attackLayerMask;  // 공격 대상 레이어 설정
    private bool patrollingForward = true; // 순찰지점에 왕복을 위한 방향 관리
    public float idleTime = 2.0f; // 순찰지점에서 대기하는 시간 (정해주지않으면 오류가 발생 할 수 있음)
    private float idleTimer = 0;  // Idle 애니메이션 대기 시간
    private float attackCooldown = 0f;  // 공격 대기 시간

    private bool isPlayingSound;

    private bool isJumping = false;
    public float jumpHight = 2.0f;
    public float jumpDuration = 1.0f;
    private Rigidbody rb;
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();  // 컴포넌트 가져오기
        animator = GetComponent<Animator>();   // 컴포넌트 가져오기
        player = PlayerManager.Instance.transform;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsByType<NavMeshLink>(FindObjectsSortMode.InstanceID);

        switch (zombieType)
        {
            case ZombieType.ZombieType1:
                agent.speed = 1f;
                attackCooldown = 1f;
                damage = 5;
                hp = 30;
                idleTimer = 1f;
                break;
            case ZombieType.ZombieType2:
                agent.speed = 2.5f;
                attackCooldown = 1.5f;
                damage = 10;
                hp = 60;
                idleTimer = 2f;
                break;
            case ZombieType.ZombieType3:
                agent.speed = 4f;
                attackCooldown = 2.1f;
                damage = 20;
                hp = 100;
                idleTimer = 3f;
                break;
            default: break;
        }

        currentState = ZombieState.Patrol; // 초기 상태: 순찰

        StartCoroutine(PlaySoundLoop());
    }

    IEnumerator PlaySoundLoop()
    {
        while (true)
        {
            isPlayingSound = false;
            
            PlaySound("ZombieIdle");
            yield return new WaitForSeconds(7f);
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);  // 플레이어의 거리

        // 플레이어가 좀비의 감지범위 안에 있고 공격중이 아닐때 추적
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase;  // 현재상태 = 좀비는 플레이어를 추적
        }

        // 플레이어가 공격 범위 안에 있고 공격중이 아닐때 공격
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //딜레이를 고려하여 공격할 수 있는 상태가 아니면 함수가 실행되지 않도록 조건을 추가

            if (!isAttacking)
            {
                currentState = ZombieState.Attack;
            }
        }

        switch (currentState)
        {
            case ZombieState.Patrol:
                Patrol();
                break;
            case ZombieState.Chase:
                ChasePlayer();
                break;
            case ZombieState.Attack:
                AttackPlayer();
                PlaySound("ZombieAttack");
                break;
            case ZombieState.Idle:
                Idle();
                PlaySound("ZombieIdle");
                break;
            case ZombieState.Die:
                Die();
                PlaySound("ZombieDie");
                break;
               
        }
        
    }

    void PlaySound(string name)
    {
        if (isPlayingSound) return;
        isPlayingSound = true;
        SoundManager.instance.PlaySFX(name);
        Debug.Log($"PlayZombieSound: {name}");
    }
    void Idle()
    {
        
        agent.isStopped = true;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", true);

        idleTimer += Time.deltaTime;  // 순찰지점에 도착하면 대기하는 변수
        if (idleTimer >= idleTime)
        {
            idleTimer = 0;
            MoveToNextPatrolPoint(); // Idle이 끝나고나서 재실행하여 재순찰
        }

    }

    void MoveToNextPatrolPoint()  // 순찰 -> 대기 후에 재순찰하는 함수  < Idle이 끝난 후에 함수가 실행됨
    {
        if (patrollingForward) // 다음 순찰 지점으로 인덱스를 이동
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2;  // 마지막 지점에서 돌아감
                patrollingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1; // 첫 지점에서 다시 전진
                patrollingForward = true;

            }
        }

        currentState = ZombieState.Patrol;  //  다시 순찰 상태 전환
    }
    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            return;
        }

        agent.isStopped = false;
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalking", true);

        agent.destination = patrolPoints[currentPatrolIndex].position; // destination: 에이전트의 이동 목적 좌표

        // 장애물이 있거나 NavMeshLink에 가까워지면 점프
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAccrossLink());
        }

        // 순찰 지점에 도착했을 때, Idle 상태로 전환                      remainingDistance: destination까지 남아 있는 거리
        if (agent.remainingDistance < 0.5f && !agent.pathPending) // pathPending: 경로가 계산중인지 여부
        {
            currentState = ZombieState.Idle;
        }
    }

    void ChasePlayer()  // 움직이다가 플레이어를 쫓는 메서드
    {
        agent.isStopped = false;   
        agent.destination = player.position;  // destination : 에이전트가 이동할 목표 위치 지정 속성

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);

        //Debug.Log("플레이어발견, 추적시작");

    }

    void AttackPlayer()  // 좀비가 공격하는 메서드
    {
        if (isAttacking) return;  // 공격중이라면 다시 공격하지 않음

        isAttacking = true;
        agent.isStopped = true; // 공격중에는 멈춤
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        //플레이어 방향으로 회전

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRoation = Quaternion.LookRotation(new Vector3(direction.x,0,direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRoation, Time.deltaTime * 5f);
        //Quaternion.Slerp(); 두개의 쿼터니언을 부드럽게 보강하는 함수  

        //Debug.Log("플레이어 공격개시");

        PerformAttack();

        StartCoroutine(AttackCooldown());

        
        //EndAttack();


    }

    public void PerformAttack() // 애니메이션에 따라 안 맞을 수도 있다
    {

        Debug.Log("Player Attack!");
        Vector3 attackDirection = player.position - handTransform.position;
        attackDirection.Normalize();

        RaycastHit hit;
        float sphereRadius = 1f;
        float castDistance = attackRange;

        if (Physics.SphereCast(handTransform.position, sphereRadius, attackDirection, out hit, castDistance, attackLayerMask))
        {
            Debug.Log("SphereCast hit: " + hit.collider.name);

            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Hit");
                PlayerManager.Instance.TakeDamage(damage);
            }
        }

        Debug.DrawRay(handTransform.position, attackDirection * castDistance, Color.red, 1f);
    }

    public void EndAttack()  //  공격이벤트(애니메이션)가 끝나면 애니메이션 이벤트로 호출되는 메서드
    {
        Debug.Log("EndAttack");
        isAttacking = false;  // 공격상태 해제
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if(distanceToPlayer <= attackRange)   //  공격 범위 내에 있다면 다시 공격 시작 실행
        {
            isPlayingSound = false;
            AttackPlayer();
        }
        else
        {
            if(distanceToPlayer <= detectionRange)  // 공격범위에 벗어났고 추적 또는 순찰모드 시작
            {
                currentState = ZombieState.Chase;
            }
            else
            {
                currentState = ZombieState.Idle;
            }
            animator.SetBool("IsWalking", true);
        }

    }

    IEnumerator AttackCooldown()
    {
        Debug.Log("AttackCooldown");
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;

        
    }

    IEnumerator JumpAccrossLink()
    {
        Debug.Log("Zombie Jump");

        isJumping = true;
        agent.isStopped = true;

        //NavMeshLink 시작과 끝 좌표 가져오기
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        // 점프 경로 계산(포물선을 그리며 점프)
        float elapsedTime = 0;
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHight; // 포물선 경로
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 도착점에 위치
        transform.position = endPos;

        // NavMeshAgent 경로 재개
        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
    }

    public void TakeDamage(float amount, string hitpartName, Vector3 hitPosition)
    {
        
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition, Quaternion.identity); // 피튀김
        Destroy(bloodEffect, 2.0f); // 일정 시간이 지나면 자동으로 삭제

        // 맞은 부위에 따른 데미지 배율과 애니메이션 설정
        if (hitpartName == "Head")
        {
            Debug.Log("Zombie Head Hit");
            amount *= 2.0f;
            animator.SetTrigger("HeadHit");
        }
        else if (hitpartName == "Body")
        {
            Debug.Log("Zombie Body Hit");
            amount *= 1.2f;
            animator.SetTrigger("BodyHit");
        }
        else
        {
            amount *= 1;
            Debug.Log("Zombie Hit");
            animator.SetTrigger("Hit");
        }

        // 좀비 체력 감소 및 체력 체크
        hp -= amount;
        hp = Mathf.Max(hp, 0);
        Debug.Log("현재 Zombie HP :" + hp);

        // 체력이 0이 되었을 때 사망 처리
        if (hp <= 0)
        {
            Die();
            
        }
    }

    void Die()
    {
        GameObject.Destroy(gameObject);
        Debug.Log("Zombie Die");
        animator.SetTrigger("IsDie");
        PlaySound("ZombieDie");
    }

    public GameObject bloodEffectPrefab;  // 피튀김 프리팹을 Inspector에 연결합니다.

    // 좀비가 총에 맞았을 때 호출될 함수


}
