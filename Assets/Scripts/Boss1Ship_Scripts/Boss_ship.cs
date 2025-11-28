using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss_ship : MonoBehaviour
{
    public enum BossState { Idle, Pattern1, Pattern2, Pattern3, Pattern4, Pattern5, Pattern6, Pattern7 }

    [Header("Refs")]
    public Transform player;
    public Transform fireLeft;
    public Transform fireRight;
    public Transform fireCenter;
    public GameObject bulletPrefab;
    public GameObject bombPrefab;
    public GameObject warningPrefab;

    [Header("Common")]
    public float zDirectionSign = -1f;
    public float patternGap = 0.6f;
    public bool loopPatterns = true;

    [Header("Pattern1 - Continuous Side Fire")]
    public float p1_fireRate = 0.12f;
    public float p1_bulletSpeed = 20f;
    public float p1_bulletLife = 3.5f;

    [Header("Pattern2 - Fan Spread")]
    public float p2_preDelay = 2.0f;
    public int p2_waves = 4;
    public float p2_waveInterval = 0.35f;
    public int p2_bulletsPerWave = 11;
    public float p2_arcDegrees = 70f;
    public float p2_bulletSpeed = 24f;
    public float p2_bulletLife = 3.2f;

    [Header("Pattern3 - Bombardment with Telegraph")]
    public int p3_count = 6;
    public float p3_warnTime = 0.9f;
    public float p3_spawnHeight = 15f;
    public float p3_forwardOffset = 18f;
    public float p3_fallSpeed = 28f;
    public float p3_bombLife = 4f;
    public float p3_laneWidth = 6f;
    public float p3_randomJitter = 0.6f;

    [Header("Pattern4 - Charge (Z -5) and Return")]
    public float p4_anticipation = 0.5f;
    public float p4_chargeDuration = 0.45f;
    public float p4_returnDuration = 0.6f;
    public AnimationCurve p4_chargeEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve p4_returnEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Pattern5 - Spiral (NEW)")]
    public float p5_duration = 3f;
    public float p5_angleSpeed = 180f;
    public int p5_bulletsPerCircle = 6;
    public float p5_bulletSpeed = 18f;
    public float p5_bulletLife = 4f;

    [Header("Pattern6 - Homing Shot (NEW)")]
    public int p6_count = 4;
    public float p6_delay = 0.3f;
    public float p6_turnSpeed = 4f;
    public float p6_bulletSpeed = 10f;
    public float p6_bulletLife = 6f;

    [Header("Pattern7 - Ring Burst (NEW)")]
    public int p7_bulletCount = 20;
    public float p7_bulletSpeed = 25f;
    public float p7_bulletLife = 3.5f;



    BossState _state = BossState.Idle;
    Coroutine _mainLoop;
    Coroutine _p1Loop;
    Vector3 _spawnPos;
    bool _isRunning;

    void Awake() { _spawnPos = transform.position; }
    void OnEnable() => StartBoss();
    void OnDisable() => StopBoss();

    public void StartBoss()
    {
        StopBoss();
        _isRunning = true;
        _mainLoop = StartCoroutine(MainLoop());
    }

    public void StopBoss()
    {
        _isRunning = false;
        if (_p1Loop != null) { StopCoroutine(_p1Loop); _p1Loop = null; }
        if (_mainLoop != null) { StopCoroutine(_mainLoop); _mainLoop = null; }
        _state = BossState.Idle;
    }

    IEnumerator MainLoop()
    {
        yield return new WaitForSeconds(0.5f);

        do
        {
            // 1. 기본 연사
            _state = BossState.Pattern1;
            _p1Loop = StartCoroutine(Pattern1_SideContinuous());
            yield return new WaitForSeconds(3.0f);

            if (_p1Loop != null) { StopCoroutine(_p1Loop); _p1Loop = null; }

            // 2~8 중 하나 랜덤
            int pick = Random.Range(2, 9); // 2~8
            switch (pick)
            {
                case 2: _state = BossState.Pattern2; yield return StartCoroutine(Pattern2_FanSpread()); break;
                case 3: _state = BossState.Pattern3; yield return StartCoroutine(Pattern3_Bombardment()); break;
                case 4: _state = BossState.Pattern4; yield return StartCoroutine(Pattern4_ChargeAndReturn()); break;
                case 5: _state = BossState.Pattern5; yield return StartCoroutine(Pattern5_Spiral()); break;
                case 6: _state = BossState.Pattern6; yield return StartCoroutine(Pattern6_Homing()); break;
                case 7: _state = BossState.Pattern7; yield return StartCoroutine(Pattern7_RingBurst()); break;
                case 8: _state = BossState.Pattern2; yield return StartCoroutine(Pattern8_FanRapid()); break;
            }


            yield return new WaitForSeconds(patternGap);

        } while (_isRunning && loopPatterns);

        if (_p1Loop != null) { StopCoroutine(_p1Loop); _p1Loop = null; }
        _state = BossState.Idle;
    }

    IEnumerator Pattern1_SideContinuous()
    {
        while (true)
        {
            FireBullet(fireLeft.position, Vector3.forward * zDirectionSign, p1_bulletSpeed, p1_bulletLife);
            FireBullet(fireRight.position, Vector3.forward * zDirectionSign, p1_bulletSpeed, p1_bulletLife);
            yield return new WaitForSeconds(p1_fireRate);
        }
    }

    IEnumerator Pattern2_FanSpread()
    {
        yield return new WaitForSeconds(p2_preDelay);

        for (int w = 0; w < p2_waves; w++)
        {
            float start = -p2_arcDegrees * 0.5f;
            float step = (p2_bulletsPerWave <= 1) ? 0f : (p2_arcDegrees / (p2_bulletsPerWave - 1));
            for (int i = 0; i < p2_bulletsPerWave; i++)
            {
                float ang = start + step * i;
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * zDirectionSign;
                FireBullet(fireCenter.position, dir, p2_bulletSpeed, p2_bulletLife);
            }
            yield return new WaitForSeconds(p2_waveInterval);
        }
    }

    IEnumerator Pattern3_Bombardment()
    {
        if (!player) yield break;

        float[] xs = new float[p3_count];
        for (int i = 0; i < p3_count; i++)
        {
            xs[i] = Random.Range(-p3_laneWidth, p3_laneWidth);
            xs[i] += Random.Range(-p3_randomJitter, p3_randomJitter);
        }

        GameObject[] warns = new GameObject[p3_count];
        for (int i = 0; i < p3_count; i++)
        {
            if (!warningPrefab) continue;
            var w = Instantiate(warningPrefab);
            w.transform.position = new Vector3(xs[i], player.position.y, player.position.z);
            warns[i] = w;
        }

        yield return new WaitForSeconds(p3_warnTime);

        for (int i = 0; i < p3_count; i++)
        {
            if (warns[i]) Destroy(warns[i]);
            Vector3 spawn = new Vector3(xs[i], player.position.y + p3_spawnHeight, player.position.z + p3_forwardOffset);
            Vector3 dir = (Vector3.down + Vector3.back * 0.35f).normalized * Mathf.Sign(zDirectionSign);
            FireBomb(spawn, dir, p3_fallSpeed, p3_bombLife);
        }
    }

    IEnumerator Pattern4_ChargeAndReturn()
    {
        yield return new WaitForSeconds(p4_anticipation);

        Vector3 start = transform.position;
        Vector3 target = new Vector3(start.x, start.y, start.z - 5f);

        float t = 0f;
        while (t < p4_chargeDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / p4_chargeDuration);
            transform.position = Vector3.Lerp(start, target, p4_chargeEase.Evaluate(u));
            yield return null;
        }

        t = 0f;
        Vector3 leave = transform.position;
        while (t < p4_returnDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / p4_returnDuration);
            transform.position = Vector3.Lerp(leave, _spawnPos, p4_returnEase.Evaluate(u));
            yield return null;
        }

        transform.position = _spawnPos;
    }

    // === NEW PATTERN 5: SPIRAL ===
    IEnumerator Pattern5_Spiral()
    {
        float elapsed = 0f;
        float angle = 0f;
        while (elapsed < p5_duration)
        {
            for (int i = 0; i < p5_bulletsPerCircle; i++)
            {
                float a = angle + (360f / p5_bulletsPerCircle) * i;
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward * zDirectionSign;
                FireBullet(fireCenter.position, dir, p5_bulletSpeed, p5_bulletLife);
            }
            angle += p5_angleSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // === NEW PATTERN 6: HOMING ===
    IEnumerator Pattern6_Homing()
    {
        for (int i = 0; i < p6_count; i++)
        {
            Vector3 dir = (player ? (player.position - fireCenter.position).normalized : Vector3.forward * zDirectionSign);
            GameObject b = Instantiate(bulletPrefab, fireCenter.position, Quaternion.LookRotation(dir));
            SimpleProjectile sp = b.GetComponent<SimpleProjectile>() ?? b.AddComponent<SimpleProjectile>();
            StartCoroutine(HomingRoutine(sp, player, p6_turnSpeed));
            sp.Init(dir, p6_bulletSpeed, p6_bulletLife);
            yield return new WaitForSeconds(p6_delay);
        }
    }

    IEnumerator HomingRoutine(SimpleProjectile sp, Transform target, float turnSpeed)
    {
        if (sp == null || target == null) yield break;
        float timer = 0f;
        while (timer < 2.5f && sp != null)
        {
            timer += Time.deltaTime;
            Vector3 toTarget = (target.position - sp.transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(sp.transform.forward, toTarget, turnSpeed * Time.deltaTime, 0f);
            sp.transform.rotation = Quaternion.LookRotation(newDir);
            yield return null;
        }
    }

    // === NEW PATTERN 7: RING BURST ===
    IEnumerator Pattern7_RingBurst()
    {
        for (int i = 0; i < p7_bulletCount; i++)
        {
            float ang = (360f / p7_bulletCount) * i;
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
            FireBullet(fireCenter.position, dir, p7_bulletSpeed, p7_bulletLife);
        }
        yield return null;
    }
    // === NEW PATTERN 8: Fan Spread (Wide + Rapid) ===
    IEnumerator Pattern8_FanRapid()
    {
        int waves = 6;
        float waveInterval = 0.25f;
        int bulletsPerWave = 15;
        float arc = 100f; // 더 넓은 부채꼴
        float bulletSpeed = 22f;
        float bulletLife = 3.0f;

        for (int w = 0; w < waves; w++)
        {
            float start = -arc * 0.5f;
            float step = (bulletsPerWave <= 1) ? 0f : (arc / (bulletsPerWave - 1));

            for (int i = 0; i < bulletsPerWave; i++)
            {
                float angle = start + step * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * zDirectionSign;
                FireBullet(fireCenter.position, dir, bulletSpeed, bulletLife);
            }

            yield return new WaitForSeconds(waveInterval);
        }
    }

    // === Common fire helpers ===
    void FireBullet(Vector3 pos, Vector3 dir, float speed, float life)
    {
        if (!bulletPrefab) return;
        var go = Instantiate(bulletPrefab, pos, Quaternion.LookRotation(dir));
        var sp = go.GetComponent<SimpleProjectile>() ?? go.AddComponent<SimpleProjectile>();
        sp.Init(dir.normalized, speed, life);
    }

    void FireBomb(Vector3 pos, Vector3 dir, float speed, float life)
    {
        if (!bombPrefab) return;
        var go = Instantiate(bombPrefab, pos, Quaternion.LookRotation(dir));
        var sp = go.GetComponent<SimpleProjectile>() ?? go.AddComponent<SimpleProjectile>();
        sp.Init(dir.normalized, speed, life);
    }
}
