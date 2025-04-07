using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour
{
    public GameObject bloodEffectPrefab;  // 피튀김 프리팹을 Inspector에 연결합니다.

    // 좀비가 총에 맞았을 때 호출될 함수
    public void TakeDamage(Vector3 hitPosition)
    {
        // 피튀김 효과 생성
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition, Quaternion.identity);
        Destroy(bloodEffect, 1.5f); // 일정 시간이 지나면 자동으로 삭제
    }
}
