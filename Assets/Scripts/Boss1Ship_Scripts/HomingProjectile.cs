using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private float _life;
    private float _elapsed;
    public float turnSpeed = 180f; // degree per second
    private Vector3 _dir;

    public void Init(Transform target, float speed, float life)
    {
        _target = target;
        _speed = speed;
        _life = life;
        _elapsed = 0f;
        _dir = transform.forward;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= _life)
        {
            Destroy(gameObject);
            return;
        }

        if (_target != null)
        {
            Vector3 toTarget = (_target.position - transform.position).normalized;
            float maxRotate = turnSpeed * Time.deltaTime;
            _dir = Vector3.RotateTowards(_dir, toTarget, Mathf.Deg2Rad * maxRotate, 0f).normalized;
        }

        transform.position += _dir * _speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_dir);
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌 처리 예: 플레이어에 데미지
        if (other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>()?.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
