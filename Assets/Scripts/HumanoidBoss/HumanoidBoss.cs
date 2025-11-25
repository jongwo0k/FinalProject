using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidBoss : Boss
{
    [Header("Humanoid Boss")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform handPos; // 돌 생성 위치

    [SerializeField] private ThrowingRock rockPrefab;

    [SerializeField] private float patternCooltime = 5.0f;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(AttackRoutine());
    }

    // 공격 패턴
    IEnumerator AttackRoutine()
    {
        while (!isDead)
        {
            // 대기
            yield return new WaitForSeconds(patternCooltime);

            // 패턴 랜덤 실행
            int patternIdx = Random.Range(0, 2);
            if (patternIdx == 0)
            {
                ThrowRockPattern();
            }
            else // if (patternIdx == 1)
            {
                StartCoroutine(JumpAttackPattern());
            }
        }
    }

    // 돌 던지기 (-> ObjectPooling?)
    private void ThrowRockPattern()
    {
        anim.SetTrigger("Throw");
        // 던질 때 추가 효과?
    }

    public void ThrowEvent()
    {
        ThrowingRock rockInstance = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
        Vector3 targetPos = player.position + Vector3.up * 0.3f; // 머리를 조준
        Vector3 dir = (targetPos - handPos.position).normalized; // Player 방향
        rockInstance.Throw(dir);
    }

    // 점프 후 공격
    IEnumerator JumpAttackPattern()
    {
        anim.SetTrigger("JumpAttack");

        yield return new WaitForSeconds(1f);

        Debug.Log("JumpAttack");
    }
}
