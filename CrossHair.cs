using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    private RectTransform crossHair;

    private float crossHairDefaultSize = 50;  // 기본사이즈
    private float crossHairSize = 100;  // 변경할 crosshair size
    private float crossHairSpeed = 50;  //crosshair 확대속도
    public float crossHairMaxSize = 150f; // crosshair 최대사이즈

    void Start()
    {
        crossHair = GetComponent<RectTransform>();
    }

  
    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 총을 쏘면 조건
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairMaxSize, Time.deltaTime * crossHairSpeed);
        }
        else
        {
            crossHairSize = Mathf.Lerp(crossHairSize, crossHairDefaultSize, Time.deltaTime * 2);
        }

        crossHair.sizeDelta = new Vector2(crossHairSize, crossHairSize); // 크기 적용
    }
}
