using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;

    [Serializable]
    public class WeaponSpawnPoint
    {
        public Weapon.WeaponType weaponType; //����Ÿ��
        public Transform spawnPoint; //�ش� ������ ���� ��ġ
    }

    public List<WeaponSpawnPoint> spawnPoints = new List<WeaponSpawnPoint>(); //�ν����Ϳ��� ������ ���� ����Ʈ
    private Dictionary<Weapon.WeaponType, Transform> weaponSpawnPoints = new Dictionary<Weapon.WeaponType, Transform>();

    public List<GameObject> weaponPrefabs; //���� ������ ����Ʈ
    private Dictionary<Weapon.WeaponType, GameObject> weaponIventory = new Dictionary<Weapon.WeaponType, GameObject>();

    private GameObject currentWeapon; // ���� ������ ����
    private Weapon.WeaponType currentWeaponType; //���� ���� Ÿ��
    private Weapon currenWeaponComponent; //���� ������ Weapon ������Ʈ(EffectPos�� �������� ���� ���)

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var spawnPoint in spawnPoints)
        {
            if (!weaponSpawnPoints.ContainsKey(spawnPoint.weaponType))
            {
                weaponSpawnPoints.Add(spawnPoint.weaponType, spawnPoint.spawnPoint);
            }
        }
    }

    public void EquipWeapon(Weapon.WeaponType weaponType)
    {
        if (!weaponIventory.ContainsKey(weaponType))
        {
            Debug.Log("���Ⱑ �κ��丮�� �����ϴ�.");
            return;
        }

        foreach (Transform child in weaponSpawnPoints[weaponType])
        {
            Destroy(child.gameObject);
        }

        GameObject newWeapon = Instantiate(weaponIventory[weaponType], weaponSpawnPoints[weaponType]);

        newWeapon.transform.localPosition = Vector3.zero;

        currentWeapon = newWeapon;
        currentWeaponType = weaponType;

        currenWeaponComponent = newWeapon.GetComponent<Weapon>();

        currentWeapon.SetActive(true);
        Debug.Log($"{weaponType} ���� ����");
    }

    public void AddWeapon(GameObject weapon)
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        SphereCollider sphereCollider = weaponComponent.GetComponent<SphereCollider>();


        if (weaponComponent != null & !weaponIventory.ContainsKey(weaponComponent.weaponType) && sphereCollider)
        {
            sphereCollider.enabled = false;
            weaponIventory.Add(weaponComponent.weaponType, weapon);
            Debug.Log($"{weaponComponent.weaponType} ���� ȹ��");
        }
    }

    public Weapon.WeaponType GetCurrentWeaponType()
    {
        return currentWeaponType; 
    }

    public Weapon GetCurrentWeaponComponent()
    {
        return currenWeaponComponent;
    }

}
