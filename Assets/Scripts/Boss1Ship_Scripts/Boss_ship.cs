using System.Collections;
using UnityEngine;

public class Boss_ship : MonoBehaviour
{
    // 보스 상태 정의
    public enum BossState { Idle, Pattern1, Pattern2, Pattern3, Pattern4, Pattern5, Pattern6 }

    // 위치 및 프리팹 참조
    public Transform player;
    public Transform fireLeft;
    public Transform fireRight;
    public Transform fireCenter;
    public GameObject bulletPrefab;
    public GameObject warningPrefab;
    public GameObject fireWallPrefab; // 불기둥(실린더) 프리팹 참조

    // 공통 설정
    public float zDirectionSign = -1f;
    public float patternGap = 0.6f;
    public bool loopPatterns = true;

    // 패턴 1 설정
    public float p1_fireRate = 0.12f;
    public float p1_bulletSpeed = 20f;
    public float p1_bulletLife = 3.5f;

    // 패턴 2 설정 (부채꼴 탄막)
    public float p2_preDelay = 1.0f;    // 부채꼴 패턴 시작 전 대기 시간
    public int p2_waves = 5;
    public float p2_waveInterval = 0.28f;
    public int p2_bulletsPerWave = 15;
    public float p2_arcDegrees = 100f;
    public float p2_bulletSpeed = 24f;
    public float p2_bulletLife = 3.2f;

    // 패턴 3 설정 (불기둥)
    public int p3_count = 6;
    public float p3_warnTime = 0.9f;
    public float p3_spawnHeight = 15f;
    public float p3_forwardOffset = 18f;
    public float p3_fallSpeed = 28f;
    public float p3_bombLife = 4f;
    public float p3_laneWidth = 6f;
    public float p3_randomJitter = 0.6f;
    public float p3_warningDuration = 2f; // 장판 지속시간
    public float p3_fireDelay = 1.5f; // 경고 후 불기둥 생성 대기시간
    public float p3_fireScale = 7f; // 불기둥 스케일
    public float p3_fireDuration = 1f; // 불기둥 지속시간 (1초)

    // 패턴 4 설정 (나선형 탄막)
    public float p4_duration = 3f;
    public float p4_angleSpeed = 180f; // 원래 회전 속도 유지
    public int p4_bulletsPerCircle = 4; // 수정: 한 서클당 4발로 간격 넓힘
    public float p4_bulletSpeed = 18f;
    public float p4_bulletLife = 4f;

    // 패턴 5 설정 (원형 폭발)
    public int p5_bulletCount = 20;
    public float p5_bulletSpeed = 25f;
    public float p5_bulletLife = 3.5f;

    BossState _state = BossState.Idle;
    Coroutine _mainLoop;
    Coroutine _p1Loop;
    Vector3 _spawnPos;
    bool _isRunning;

    void Awake()
    {
        _spawnPos = transform.position;
    }

    void OnEnable()
    {
        StartBoss();
    }

    void OnDisable()
    {
        StopBoss();
    }

    // 보스 동작 시작
    public void StartBoss()
    {
        StopBoss();
        _isRunning = true;
        _mainLoop = StartCoroutine(MainLoop());
    }

    // 보스 동작 정지
    public void StopBoss()
    {
        _isRunning = false;

        if (_p1Loop != null)
        {
            StopCoroutine(_p1Loop);
            _p1Loop = null;
        }

        if (_mainLoop != null)
        {
            StopCoroutine(_mainLoop);
            _mainLoop = null;
        }

        _state = BossState.Idle;
    }

    // 메인 패턴 루프
    IEnumerator MainLoop()
    {
        yield return new WaitForSeconds(0.5f);
        do
        {
            // 패턴1: 기본 패턴은 Center에서 연속 발사
            _state = BossState.Pattern1;
            _p1Loop = StartCoroutine(Pattern1_CenterContinuous());
            yield return new WaitForSeconds(3.0f);

            if (_p1Loop != null)
            {
                StopCoroutine(_p1Loop);
                _p1Loop = null;
            }

            // 패턴 2~6 중 하나 선택 (2..6)
            int pick = UnityEngine.Random.Range(2, 7);

            switch (pick)
            {
                case 2:
                    _state = BossState.Pattern2;
                    yield return StartCoroutine(Pattern2_FanRapid());
                    break;

                case 3:
                    _state = BossState.Pattern3;
                    yield return StartCoroutine(Pattern3_Bombardment()); // 경고 -> 불기둥 패턴
                    break;

                case 4:
                    _state = BossState.Pattern4;
                    yield return StartCoroutine(Pattern4_Spiral());
                    break;

                case 5:
                    _state = BossState.Pattern5;
                    yield return StartCoroutine(Pattern5_RingBurst());
                    break;

                case 6:
                    _state = BossState.Pattern6;
                    yield return StartCoroutine(Pattern6_SideAlternating()); // 새로 추가된 좌우 발사 패턴
                    break;
            }

            yield return new WaitForSeconds(patternGap);

        } while (_isRunning && loopPatterns);

        if (_p1Loop != null)
        {
            StopCoroutine(_p1Loop);
            _p1Loop = null;
        }

        _state = BossState.Idle;
    }

    // 패턴 1: 기본 패턴은 Center에서 연속 발사
    IEnumerator Pattern1_CenterContinuous()
    {
        while (true)
        {
            FireBullet(fireCenter.position, Vector3.forward * zDirectionSign, p1_bulletSpeed, p1_bulletLife);
            yield return new WaitForSeconds(p1_fireRate);
        }
    }

    // 패턴 2: 부채꼴 연속 사격 (시작 전 대기)
    IEnumerator Pattern2_FanRapid()
    {
        yield return new WaitForSeconds(p2_preDelay);

        for (int w = 0; w < p2_waves; w++)
        {
            float arc = p2_arcDegrees;
            int count = Mathf.Max(1, p2_bulletsPerWave);
            float start = -arc * 0.5f;
            float step = (count <= 1) ? 0f : (arc / (count - 1));

            for (int i = 0; i < count; i++)
            {
                float angle = start + step * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * zDirectionSign;
                FireBullet(fireCenter.position, dir, p2_bulletSpeed, p2_bulletLife);
            }

            yield return new WaitForSeconds(p2_waveInterval);
        }
    }

    // 패턴 3: 경고 장판 생성 후 불기둥 생성 (장판은 플레이어 발밑에, y=0.01)
    IEnumerator Pattern3_Bombardment()
    {
        if (!player) yield break;

        GameObject[] warns = new GameObject[p3_count];

        for (int i = 0; i < p3_count; i++)
        {
            if (!warningPrefab) continue;
            Vector3 warnPos = new Vector3(player.position.x, 0.01f, player.position.z); // 플레이어 발밑, y=0.01
            var w = Instantiate(warningPrefab);
            w.transform.position = warnPos;
            warns[i] = w;
            StartCoroutine(WarningAndFireRoutine(w, warnPos, i));
        }

        // 패턴 전체 대기 시간: 경고가 뜨고 불기둥이 생성되고 지나갈 시간 확보
        yield return new WaitForSeconds(p3_warningDuration + p3_fireDuration);
    }

    // 장판을 띄우고 일정시간 후 사라지게 하고 불기둥 생성
    IEnumerator WarningAndFireRoutine(GameObject warnObj, Vector3 pos, int index)
    {
        if (warnObj)
        {
            // 장판은 p3_warningDuration 동안 유지되고 마지막에는 서서히 사라짐
            StartCoroutine(FadeAndDestroyRenderer(warnObj, p3_warningDuration, 0.5f));
        }

        // 경고 후 불기둥 생성 대기
        yield return new WaitForSeconds(p3_fireDelay);

        // 불기둥 생성
        if (fireWallPrefab != null)
        {
            var fw = Instantiate(fireWallPrefab);
            Vector3 targetScale = Vector3.one * p3_fireScale;
            fw.transform.localScale = targetScale;
            float halfHeight = targetScale.y * 0.5f;
            fw.transform.position = new Vector3(pos.x, pos.y + halfHeight, pos.z); // 장판 위에 불기둥 배치
            StartCoroutine(FireWallEffectRoutine(fw, p3_fireDuration));
        }
    }

    // 불기둥 이펙트 간단 루틴: 등장 애니메이션과 펄스, 지속시간 후 제거
    IEnumerator FireWallEffectRoutine(GameObject fw, float duration)
    {
        if (fw == null) yield break;

        Vector3 targetScale = fw.transform.localScale;
        fw.transform.localScale = Vector3.zero;

        float growTime = 0.25f;
        float t = 0f;
        while (t < growTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / growTime);
            fw.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, k);
            yield return null;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(elapsed * 10f) * 0.05f;
            fw.transform.localScale = targetScale * pulse;
            yield return null;
        }

        float shrinkTime = 0.35f;
        t = 0f;
        Vector3 startScale = fw.transform.localScale;
        while (t < shrinkTime)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.SmoothStep(0f, 1f, t / shrinkTime);
            fw.transform.localScale = startScale * k;
            yield return null;
        }

        Destroy(fw);
    }

    // 경고 장판 및 하위 렌더러 페이드 후 Destroy
    IEnumerator FadeAndDestroyRenderer(GameObject go, float holdTime, float fadeTime)
    {
        if (go == null) yield break;

        yield return new WaitForSeconds(holdTime - fadeTime);
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        float t = 0f;
        Material[] mats = new Material[rends.Length];
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != null && rends[i].material != null)
            {
                mats[i] = rends[i].material;
            }
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(1f - (t / fadeTime));
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] == null || mats[i] == null) continue;
                if (mats[i].HasProperty("_Color"))
                {
                    Color c = mats[i].color;
                    c.a = a;
                    mats[i].color = c;
                }
            }
            yield return null;
        }

        Destroy(go);
    }

    // 패턴 4: 나선형 공격 
    IEnumerator Pattern4_Spiral()
    {
        yield return new WaitForSeconds(1.7f);

        float elapsed = 0f;
        float angle = 0f;

        while (elapsed < p4_duration)
        {
            for (int i = 0; i < p4_bulletsPerCircle; i++)
            {
                float a = angle + (360f / p4_bulletsPerCircle) * i;
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward * zDirectionSign;
                FireBullet(fireCenter.position, dir, p4_bulletSpeed, p4_bulletLife);
            }

            angle += p4_angleSpeed * Time.deltaTime; // 원래 회전 증분 사용
            elapsed += Time.deltaTime;

            yield return null;
        }
    }

    // 패턴 5: 전체 방향으로 원형 폭발
    IEnumerator Pattern5_RingBurst()
    {
        for (int i = 0; i < p5_bulletCount; i++)
        {
            float ang = (360f / p5_bulletCount) * i;
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
            FireBullet(fireCenter.position, dir, p5_bulletSpeed, p5_bulletLife);
        }

        yield return null;
    }

    // 패턴 6: 좌우 교차 연속 발사 
    IEnumerator Pattern6_SideAlternating()
    {
        float duration = 3.0f; // 이 패턴의 지속시간
        float elapsed = 0f;
        bool leftNext = true;
        while (elapsed < duration)
        {
            if (leftNext)
            {
                FireBullet(fireLeft.position, Vector3.forward * zDirectionSign, p1_bulletSpeed, p1_bulletLife);
            }
            else
            {
                FireBullet(fireRight.position, Vector3.forward * zDirectionSign, p1_bulletSpeed, p1_bulletLife);
            }
            leftNext = !leftNext;
            elapsed += p1_fireRate;
            yield return new WaitForSeconds(p1_fireRate);
        }
    }

    // 총알 생성 함수 
    void FireBullet(Vector3 pos, Vector3 dir, float speed, float life)
    {
        if (!bulletPrefab) return;

        var go = Instantiate(bulletPrefab, pos, Quaternion.LookRotation(dir));
        var sp = go.GetComponent<SimpleProjectile>() ?? go.AddComponent<SimpleProjectile>();
        sp.Init(dir.normalized, speed, life);
        Destroy(go, 3f); // 모든 투사체는 3초 후 제거
    }
}
