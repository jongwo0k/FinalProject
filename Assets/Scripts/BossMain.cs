using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] protected float HP = 100; // 임의
    protected float currentHP;
    protected bool isDead = false;

    [Header("Component")]
    protected Animator anim;
    protected Collider col;

    /*
    [SerializeField] protected Transform player; // 플레이어 위치

    // 코루틴 공통 관리
    protected Coroutine attackRoutine;

    protected virtual void OnEnable()
    {
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    protected virtual void OnDisable()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }

    protected abstract IEnumerator AttackRoutine();
    */

    protected virtual void Start()
    {
        isDead = false;
        currentHP = HP;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider>();

        // UI 초기화
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

        GameManager.Instance.StageClear();
    }

    // Event에서
    public void DestroyBoss()
    {
        Destroy(gameObject);
    }
}