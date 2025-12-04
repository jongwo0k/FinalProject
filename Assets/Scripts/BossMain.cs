using UnityEngine;
using System.Collections;

public abstract class Boss : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] protected float HP = 100; // 임의
    protected float currentHP;
    protected bool isDead = false;

    [Header("Component")]
    protected Animator anim;
    protected Collider col;

    [SerializeField] protected Transform player; // Player 현재 위치


    // 코루틴 공통 관리
    protected Coroutine attackRoutine;

    protected virtual void OnEnable()
    {
        if (attackRoutine == null)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    protected virtual void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    protected abstract IEnumerator AttackRoutine();
    

    protected virtual void Awake()
    {
        isDead = false;
        currentHP = HP;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        UpdateUI();
    }

    // Bullet에 맞으면 HP 감소
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            if (!isDead)
            {
                float bulletDamage = 1; // (1=임시) Player의 Bullet에서 데미지 가져옴
                TakeDamage(bulletDamage);
            }
            Destroy(other.transform.parent.gameObject); // Empty안에 Bullet(Capsule)이 들어 있을 경우 전부 삭제, 부모 Empty없으면 Destroy(other.gameObject);
        }
    }

    // 피격 처리
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        UpdateUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // UI 업데이트 (HP, ...)
    protected virtual void UpdateUI()
    {

    }

    // 사망 처리
    protected virtual void Die()
    {
        isDead = true;
        anim.SetBool("isDead", true);
        col.enabled = false;
    }

    // Event에서
    public void DestroyBoss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StageClear();
        }
        Destroy(gameObject);
    }
}