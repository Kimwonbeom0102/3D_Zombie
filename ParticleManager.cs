using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance { get; private set; }

    public enum ParticleType
    {
        PistolEffect,
        ShotGunEffect,
        RifleEffect,
        SMGEffect,
        BloodEffect,
        BrickImpact

    }

    public Dictionary<ParticleType, GameObject> particleDic = new Dictionary<ParticleType, GameObject>();

    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public GameObject pistolEffect;
    public GameObject shotGunEffect;
    public GameObject RifleEffect;
    public GameObject SMGEffect;
    public GameObject bloodEffect;
    public GameObject BrickImpact;
    GameObject particleObj;
    public int poolSize = 20;

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

        particleDic.Add(ParticleType.PistolEffect, pistolEffect);
        particleDic.Add(ParticleType.ShotGunEffect, shotGunEffect);
        //particleDic.Add(ParticleType.RifleEffect, RifleEffect);
        //particleDic.Add(ParticleType.SMGEffect, SMGEffect);
        //particleDic.Add(ParticleType.BloodEffect, bloodEffect);
        //particleDic.Add(ParticleType.BrickImpact, BrickImpact);

        pistolEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        BrickImpact.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    private void Start()
    {
        foreach (var particleType in particleDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>(); //Queue : FIFO �����͸� ó���ϴ� �ڷᱸ��
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleDic[particleType]);
                obj.SetActive(false);
                pool.Enqueue(obj); //Enqueue : Queue�� �߰��ϴ� �Լ�
            }
            particlePools.Add(particleType, pool);
        }
    }

    public void PlayParticle(ParticleType type, Vector3 position)
    {
        if (particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if (particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem == null)
                {
                    return;
                }


                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //��ƼŬ �ý��� ������ �����ϰ�, ������ ����� ��� ��ƼŬ�� �����մϴ�.

                }
                particleObj.SetActive(true);
                particleSystem.Play();

                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj); //Enqueue() : �����͸� Queue�� �߰��ϴ� �Լ� ���ο� ��Ҹ� ���� �߰�
    }
}
