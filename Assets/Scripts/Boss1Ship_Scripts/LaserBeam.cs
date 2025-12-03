using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    LineRenderer _lr;
    Transform _boss;
    float _duration;
    float _timer;
    float _startAngle;
    float _endAngle;

    // Init: bossTransform used as sweep center and forward basis
    public void Init(Transform boss, float startAngleDeg, float endAngleDeg, float duration)
    {
        _lr = GetComponent<LineRenderer>();
        _boss = boss;
        _startAngle = startAngleDeg;
        _endAngle = endAngleDeg;
        _duration = duration;
        _timer = 0f;
        // initial setup
        if (_lr != null)
        {
            _lr.positionCount = 2;
        }
    }

    void Update()
    {
        if (_boss == null) { Destroy(gameObject); return; }
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / _duration);
        float ang = Mathf.Lerp(_startAngle, _endAngle, t);
        Vector3 dir = Quaternion.Euler(0f, ang, 0f) * _boss.forward;
        Vector3 origin = _boss.position;
        float length = 40f;
        Vector3 end = origin + dir.normalized * length;

        if (_lr != null)
        {
            _lr.SetPosition(0, origin);
            _lr.SetPosition(1, end);
        }

        // Raycast damage
        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, length))
        {
            var go = hit.collider.gameObject;
            if (go.CompareTag("Player"))
            {
                // go.GetComponent<PlayerHealth>()?.TakeDamage(1);
            }
        }

        if (_timer >= _duration)
        {
            Destroy(gameObject);
        }
    }
}
