using System.Collections;
using UnityEngine;

public class HumanoidBoss : Boss
{
    [SerializeField] private float patternCooltime = 5.0f;

    [Header("Throwing Pattern")]
    [SerializeField] private ThrowingRock rockPrefab;
    [SerializeField] private float throwAttackDamage = 30;
    [SerializeField] private float throwAttackSpeed = 10;
    [SerializeField] private Transform player;
    [SerializeField] private Transform handPos; // 돌 생성 위치

    [Header("Jump Pattern")]
    [SerializeField] private float attackRadius = 2f; // 공격 범위
    [SerializeField] private float jumpAttackDamage = 20;
    [SerializeField] private GameObject redZonePrefab;
    [SerializeField] private GameObject jumpAttackEffect;
    private GameObject currentRedZone;
    private Vector3 jumpTarget;

    [Header("Roar Pattern")]
    [SerializeField] private float rockSpreadAngle = 3f;
    [SerializeField] private float roarAttackDamage = 10;
    [SerializeField] private float roarAttackSpeed = 20;
    [SerializeField] private Transform spawnPos; // 돌 생성 위치

    protected override void Start()
    {
        base.Start();
    }

    // 공격 패턴
    protected override IEnumerator AttackRoutine()
    {
        while (!isDead)
        {
            // 대기
            yield return new WaitForSeconds(patternCooltime);

            // 패턴 랜덤 실행
            int patternIdx = Random.Range(0, 3);
            if (patternIdx == 0)
            {
                ThrowRockPattern();
            }
            else if (patternIdx == 1)
            {
                JumpAttackPattern();
            }
            else // if (patternIdx == 2)
            {
                RoarPattern();
            }
        }
    }

    // 돌 던지기 (-> ObjectPooling?)
    private void ThrowRockPattern()
    {
        if (player == null) return;

        anim.SetTrigger("Throw");
        // 던질 때 추가 효과?
    }

    public void ThrowEvent()
    {
        Vector3 targetPos = player.position + Vector3.up * 0.3f; // 머리를 조준
        Vector3 dir = (targetPos - handPos.position).normalized; // Player 방향

        ThrowingRock rockInstance = Instantiate(rockPrefab, handPos.position, Quaternion.identity);

        rockInstance.Throw(dir, throwAttackSpeed, throwAttackDamage);
    }

    // 점프 후 공격
    private void JumpAttackPattern()
    {
        if (player == null) return;

        jumpTarget = player.position;
        jumpTarget.y = 0.01f;
        currentRedZone = Instantiate(redZonePrefab, jumpTarget, Quaternion.identity);
        currentRedZone.transform.localScale = new Vector3(attackRadius * 2, 0.05f, attackRadius * 2);
        anim.SetTrigger("JumpAttack");
    }

    public void JumpAttackEvent()
    {
        // 레드존 제거
        if (currentRedZone != null)
        {
            Destroy(currentRedZone);
        }

        // 바닥 이펙트
        Instantiate(jumpAttackEffect, jumpTarget, Quaternion.identity);

        // 범위 내에 있으면 데미지
        float distance = Vector3.Distance(player.position, jumpTarget);
        if (distance <= attackRadius)
        {
            Debug.Log("Player 회피 실패");
            // jumpAttackDamage를 Player쪽 TakeDamage에 전달;
        }
    }

    // 포효하기 (좌우로 돌 소환)
    private void RoarPattern()
    {
        if (player == null) return;

        anim.SetTrigger("Roar");
    }

    public void SpawnRockEvent()
    {
        Vector3 spawnPoint = spawnPos.position;
        Vector3 dir = (player.position - spawnPoint).normalized;

        Vector3 leftDir = Quaternion.Euler(0, -rockSpreadAngle, 0) * dir;
        Vector3 rightDir = Quaternion.Euler(0, rockSpreadAngle, 0) * dir;

        ThrowingRock leftRock = Instantiate(rockPrefab, spawnPoint, Quaternion.identity);
        ThrowingRock rightRock = Instantiate(rockPrefab, spawnPoint, Quaternion.identity);

        leftRock.Throw(leftDir, roarAttackSpeed, roarAttackDamage);
        rightRock.Throw(rightDir, roarAttackSpeed, roarAttackDamage);
    }
}