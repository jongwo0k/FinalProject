using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class HumanoidBoss : Boss
{
    [SerializeField] private float patternCooltime = 5.0f;

    [Header("Throwing Pattern")]
    [SerializeField] private ThrowingRock rockPrefab;
    [SerializeField] private float throwAttackDamage = 30;
    [SerializeField] private float throwAttackSpeed = 10;
    [SerializeField] private Transform handPos; // 돌 생성 위치

    [Header("Roar Pattern")]
    [SerializeField] private float rockSpreadAngle = 3f;
    [SerializeField] private float roarAttackDamage = 10;
    [SerializeField] private float roarAttackSpeed = 20;
    [SerializeField] private Transform spawnPos; // 돌 생성 위치

    [Header("Jump Pattern")]
    [SerializeField] private float jumpAttackRadius = 2f; // 공격 범위
    [SerializeField] private float jumpAttackDamage = 20;
    [SerializeField] private GameObject redZonePrefab;
    [SerializeField] private GameObject redZoneEffect;
    private GameObject currentRedZone;
    private Vector3 jumpTarget;

    [Header("Magic Pattern")]
    [SerializeField] private int redZoneCount = 3;
    [SerializeField] private float magicAttackRadius = 1f;
    [SerializeField] private float magicAttackDamage = 10;
    [SerializeField] private float magicSpawnRange = 1f; // 생성 범위
    [SerializeField] private float magicSpawnInterval = 0.7f;
    [SerializeField] private float magicExplodeDelay = 0.5f;
    private Vector3 magicTarget;

    protected override void Awake()
    {
        base.Awake();
    }

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
            int patternIdx = Random.Range(0, 4);
            if (patternIdx == 0)
            {
                ThrowRockPattern();
            }
            else if (patternIdx == 1)
            {
                RoarPattern();
            }
            else if (patternIdx == 2)
            {
                JumpAttackPattern();
            }
            else // if (patternIdx == 3)
            {
                MagicPattern();
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

    // 점프 후 공격
    private void JumpAttackPattern()
    {
        if (player == null) return;

        jumpTarget = player.position;
        jumpTarget.y = 0.01f;
        currentRedZone = Instantiate(redZonePrefab, jumpTarget, Quaternion.identity);
        currentRedZone.transform.localScale = new Vector3(jumpAttackRadius * 2, 0.05f, jumpAttackRadius * 2);
        anim.SetTrigger("JumpAttack");
    }

    public void JumpAttackEvent()
    {
        ExplodeRedZone(currentRedZone, jumpTarget, jumpAttackRadius, jumpAttackDamage);
    }

    // 마법 공격 (RedZone 여러 개 소환)
    private void MagicPattern()
    {
        if (player == null) return;

        anim.SetTrigger("Magic");
    }

    public void MagicEvent()
    {
        StartCoroutine(SpawnMagicZone());
        
    }

    IEnumerator SpawnMagicZone()
    {
        for (int i = 0; i < redZoneCount; i++)
        {
            // 위치 지정
            magicTarget = player.position;
            magicTarget.x += Random.Range(-magicSpawnRange, magicSpawnRange);
            magicTarget.y = 0.01f;

            // 간격을 두고 생성
            StartCoroutine(HandleMagicZone(magicTarget));
            yield return new WaitForSeconds(magicSpawnInterval);
        }
    }

    IEnumerator HandleMagicZone(Vector3 magicTarget)
    {
        GameObject redZone = Instantiate(redZonePrefab, magicTarget, Quaternion.identity);
        redZone.transform.localScale = new Vector3(magicAttackRadius * 2, 0.05f, magicAttackRadius * 2);

        yield return new WaitForSeconds(magicExplodeDelay);
        ExplodeRedZone(redZone, magicTarget, magicAttackRadius, magicAttackDamage);
    }

    private void ExplodeRedZone(GameObject redZone, Vector3 targetPos, float radius, float damage)
    {
        // 레드존 제거
        if (redZone != null)
        {
            Destroy(redZone);
        }

        // 바닥 이펙트
        Instantiate(redZoneEffect, targetPos, Quaternion.identity);

        // 범위 내에 있으면 데미지
        Vector3 playerPosition = new Vector3(player.position.x, 0, player.position.z);
        Vector3 attackTarget = new Vector3(targetPos.x, 0, targetPos.z);

        float distance = Vector3.Distance(playerPosition, attackTarget);

        if (distance <= radius)
        {
            Debug.Log($"Player 회피 실패, 데미지: {damage}");
        }
    }
}