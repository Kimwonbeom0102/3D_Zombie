using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        //None
        Pistol, ShotGun, Rifle, SMG 
    }

 

    //public WeaponType weapon;

    public Camera targetCamera;
    public Transform UIImage;
    public WeaponType weaponType;

    public Transform effectPos;  // 총구(이펙트가 나올 포지션)에 넣을 변수를 만들어줍니다. Transform
    
    void Start()
    {
        if(targetCamera == null)
        {
            targetCamera = Camera.main;
        }
       UIImage.gameObject.SetActive(false);

        if(effectPos == null)
        {
            effectPos = transform.Find("EffectPos");
        }
    }

  
    void Update()
    {
        Vector3 direction = targetCamera.transform.position - UIImage.position;   // 카메라와의 방향 계산
        direction.y = 0;   //Y 축 회전을 고정하여 ui가 위 아래로  기울어지지 않도록 함
        Quaternion rotation = Quaternion.LookRotation(-direction); //UI가 카메라를 바라보도록 회전
        UIImage.rotation = rotation;     // UIIMAGE 회전

        return;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        UIImage.gameObject.SetActive(true);
    }
    private void OnTriggerStay(Collider other)
    {
        UIImage.gameObject.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        UIImage.gameObject.SetActive(false);
    }
}
