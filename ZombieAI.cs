using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    // ù°�� ���־�� �� �͵�  => ������ ����, Ư�� ����
    public enum ZombieState // ������ ���¸� ����
    {
        Patrol, Chase, Attack, Idle, Die
    }

    public ZombieState currentState; // ������ ������¸� �� ��

    public enum ZombieType  // ������ Ÿ���� ������ => ���� �ƴ� �ٸ� �Ű�ü�� ������ �����մϴ�. (�Ϲ� �ù�, �������� ���� ��)
    {
        ZombieType1, ZombieType2, ZombieType3
    }

    public ZombieType zombieType;
    public float hp = 10; // �⺻ü��( ������ ������ ZombieType���� �ٸ��� ������ �� ����)
    public int damage = 10;

    public Transform[] patrolPoints; // ���� ������
    private Transform player; // �÷��̾��� Transform
    public float detectionRange = 10f; // AI�� �÷��̾ �����ϴ� ����

    public float attackRange = 2.0f; // ���ݹ���  < �������� ���. 1.cast�� ����Ѵ�(��� ���־���ϴ� ����) 2.���ʿ� collision�� �����ش� 3.�ڵ带¥�ش�.
    public Transform handTransform; // ������ �� ��ġ (����) �������� ������� �÷��̾ �����ϰ� ����
    private bool isAttacking; // ���� �������� ��Ÿ���� ����
    public float sphereCastRadius = 0.5f; // cast�ݰ� (collide)

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private Animator animator;

    public LayerMask attackLayerMask;  // ���� ��� ���̾� ����
    private bool patrollingForward = true; // ���������� �պ��� ���� ���� ����
    public float idleTime = 2.0f; // ������������ ����ϴ� �ð� (�������������� ������ �߻� �� �� ����)
    private float idleTimer = 0;  // Idle �ִϸ��̼� ��� �ð�
    private float attackCooldown = 0f;  // ���� ��� �ð�

    private bool isPlayingSound;

    private bool isJumping = false;
    public float jumpHight = 2.0f;
    public float jumpDuration = 1.0f;
    private Rigidbody rb;
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();  // ������Ʈ ��������
        animator = GetComponent<Animator>();   // ������Ʈ ��������
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

        currentState = ZombieState.Patrol; // �ʱ� ����: ����

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
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);  // �÷��̾��� �Ÿ�

        // �÷��̾ ������ �������� �ȿ� �ְ� �������� �ƴҶ� ����
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase;  // ������� = ����� �÷��̾ ����
        }

        // �÷��̾ ���� ���� �ȿ� �ְ� �������� �ƴҶ� ����
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //�����̸� ����Ͽ� ������ �� �ִ� ���°� �ƴϸ� �Լ��� ������� �ʵ��� ������ �߰�

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

        idleTimer += Time.deltaTime;  // ���������� �����ϸ� ����ϴ� ����
        if (idleTimer >= idleTime)
        {
            idleTimer = 0;
            MoveToNextPatrolPoint(); // Idle�� �������� ������Ͽ� �����
        }

    }

    void MoveToNextPatrolPoint()  // ���� -> ��� �Ŀ� ������ϴ� �Լ�  < Idle�� ���� �Ŀ� �Լ��� �����
    {
        if (patrollingForward) // ���� ���� �������� �ε����� �̵�
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2;  // ������ �������� ���ư�
                patrollingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1; // ù �������� �ٽ� ����
                patrollingForward = true;

            }
        }

        currentState = ZombieState.Patrol;  //  �ٽ� ���� ���� ��ȯ
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

        agent.destination = patrolPoints[currentPatrolIndex].position; // destination: ������Ʈ�� �̵� ���� ��ǥ

        // ��ֹ��� �ְų� NavMeshLink�� ��������� ����
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAccrossLink());
        }

        // ���� ������ �������� ��, Idle ���·� ��ȯ                      remainingDistance: destination���� ���� �ִ� �Ÿ�
        if (agent.remainingDistance < 0.5f && !agent.pathPending) // pathPending: ��ΰ� ��������� ����
        {
            currentState = ZombieState.Idle;
        }
    }

    void ChasePlayer()  // �����̴ٰ� �÷��̾ �Ѵ� �޼���
    {
        agent.isStopped = false;   
        agent.destination = player.position;  // destination : ������Ʈ�� �̵��� ��ǥ ��ġ ���� �Ӽ�

        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);

        //Debug.Log("�÷��̾�߰�, ��������");

    }

    void AttackPlayer()  // ���� �����ϴ� �޼���
    {
        if (isAttacking) return;  // �������̶�� �ٽ� �������� ����

        isAttacking = true;
        agent.isStopped = true; // �����߿��� ����
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        //�÷��̾� �������� ȸ��

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRoation = Quaternion.LookRotation(new Vector3(direction.x,0,direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRoation, Time.deltaTime * 5f);
        //Quaternion.Slerp(); �ΰ��� ���ʹϾ��� �ε巴�� �����ϴ� �Լ�  

        //Debug.Log("�÷��̾� ���ݰ���");

        PerformAttack();

        StartCoroutine(AttackCooldown());

        
        //EndAttack();


    }

    public void PerformAttack() // �ִϸ��̼ǿ� ���� �� ���� ���� �ִ�
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

    public void EndAttack()  //  �����̺�Ʈ(�ִϸ��̼�)�� ������ �ִϸ��̼� �̺�Ʈ�� ȣ��Ǵ� �޼���
    {
        Debug.Log("EndAttack");
        isAttacking = false;  // ���ݻ��� ����
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if(distanceToPlayer <= attackRange)   //  ���� ���� ���� �ִٸ� �ٽ� ���� ���� ����
        {
            isPlayingSound = false;
            AttackPlayer();
        }
        else
        {
            if(distanceToPlayer <= detectionRange)  // ���ݹ����� ����� ���� �Ǵ� ������� ����
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

        //NavMeshLink ���۰� �� ��ǥ ��������
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        // ���� ��� ���(�������� �׸��� ����)
        float elapsedTime = 0;
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHight; // ������ ���
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �������� ��ġ
        transform.position = endPos;

        // NavMeshAgent ��� �簳
        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
    }

    public void TakeDamage(float amount, string hitpartName, Vector3 hitPosition)
    {
        
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition, Quaternion.identity); // ��Ƣ��
        Destroy(bloodEffect, 2.0f); // ���� �ð��� ������ �ڵ����� ����

        // ���� ������ ���� ������ ������ �ִϸ��̼� ����
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

        // ���� ü�� ���� �� ü�� üũ
        hp -= amount;
        hp = Mathf.Max(hp, 0);
        Debug.Log("���� Zombie HP :" + hp);

        // ü���� 0�� �Ǿ��� �� ��� ó��
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

    public GameObject bloodEffectPrefab;  // ��Ƣ�� �������� Inspector�� �����մϴ�.

    // ���� �ѿ� �¾��� �� ȣ��� �Լ�


}
