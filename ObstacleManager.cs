using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Moving,       //이동 장애물
    Rotating,     //회전 장애물
    Visibility,   //투명 장애물
    ShrinkingPlatform, //크기가 점점 줄어드는 장애물
    DamagingPlatform //HP가 감소하는 장애물
}


public class ObstacleManager : MonoBehaviour
{
    public ObstacleType obstacleType;

    public List<Transform> points; //이동 경로 리스트
    public float moveSpeed = 2.0f; //이동 속도
    private int currenPointIndex = 0; //현재 목표 지점 인덱스

    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 50f;

    private Renderer objectRenderer;
    public bool isVisible = true;

    public float shrinkRate = 0.1f; //매 프레임 크기 감소 비율
    private Vector3 initialScale; //초기 크기

    public float damageRate = 10.0f;

    void Start()
    {

        objectRenderer = GetComponent<Renderer>();

        if (points.Count > 0)
        {
            transform.position = points[0].position;
        }

        initialScale = transform.localScale;
    }

        void Update()
        {
        switch (obstacleType)
        {
            case ObstacleType.Moving:
                Movement();
                break;
            case ObstacleType.Rotating:
                Rotation();
                break;
            case ObstacleType.Visibility:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            switch (obstacleType)
            {
                case ObstacleType.ShrinkingPlatform:
                    ShrinkingPlatform();
                    break;
                case ObstacleType.DamagingPlatform:
                    DamagingPlatform();
                    break;
            }
        }
    }

    void Movement()
    {
        if (points.Count == 0) return;

        //현재 목표 지점
        Transform targetPoint = points[currenPointIndex];

        // 현재 위치에서 목표 지점으로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        //목표 지점에 도달하면 다음 지점으로 인덱스를 순환
        if (transform.position == targetPoint.position)
        {
            currenPointIndex = (currenPointIndex + 1) % points.Count;
            Debug.Log(currenPointIndex);
        }
    }

    void Rotation()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    public void Visibleility()
    {
        objectRenderer.enabled = isVisible;
        gameObject.SetActive(isVisible);
    }

    void ShrinkingPlatform()
    {
        if (transform.localScale.x > 0.1f)
        {
            transform.localScale -= Vector3.one * shrinkRate;
        }
    }

    void DamagingPlatform()
    {
        //PlayerManager.Instance.TakeDamage(damageRate);
    }
}
