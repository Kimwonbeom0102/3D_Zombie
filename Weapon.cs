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

    public Transform effectPos;  // �ѱ�(����Ʈ�� ���� ������)�� ���� ������ ������ݴϴ�. Transform
    
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
        Vector3 direction = targetCamera.transform.position - UIImage.position;   // ī�޶���� ���� ���
        direction.y = 0;   //Y �� ȸ���� �����Ͽ� ui�� �� �Ʒ���  �������� �ʵ��� ��
        Quaternion rotation = Quaternion.LookRotation(-direction); //UI�� ī�޶� �ٶ󺸵��� ȸ��
        UIImage.rotation = rotation;     // UIIMAGE ȸ��

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
