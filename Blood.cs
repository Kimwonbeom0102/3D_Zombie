using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour
{
    public GameObject bloodEffectPrefab;  // ��Ƣ�� �������� Inspector�� �����մϴ�.

    // ���� �ѿ� �¾��� �� ȣ��� �Լ�
    public void TakeDamage(Vector3 hitPosition)
    {
        // ��Ƣ�� ȿ�� ����
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition, Quaternion.identity);
        Destroy(bloodEffect, 1.5f); // ���� �ð��� ������ �ڵ����� ����
    }
}
