using UnityEngine;

public class ThrowingRock : MonoBehaviour
{
    [Header("Ability")]
    [SerializeField] private float rockSpeed = 10f;
    // [SerializeField] private float rockDamage = 10f;

    [Header("Prefabs")]
    [SerializeField] private GameObject fracturePrefab;

    private Vector3 playerDirection;
    private bool isThrow = false;

    void Update()
    {
        if (!isThrow)
        {
            return;
        }

        transform.Translate(playerDirection * rockSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right * 360 * Time.deltaTime);
    }

    // 충돌
    public void OnTriggerEnter(Collider other)
    {
        // Bullet, Boss 자신 등과는 충돌X -> Layer or Tag
        if (other.CompareTag("Boss"))
        {
            return;
        }

        // 화면을 넘어간 경우
        if (other.CompareTag("DestroyZone"))
        {
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player 명중");
            // playerHP -= rockDamage;
        }
        
        FractureRock();
    }

    public void Throw(Vector3 dir) // Player방향, Boss한테 받음
    {
        playerDirection = dir;
        isThrow = true;
        // Destroy -> DestroyZone or 좌표 or 시간
    }

    // 충돌 후 파괴
    void FractureRock()
    {
        GameObject fracture = Instantiate(fracturePrefab, transform.position, transform.rotation);
        Destroy(fracture, 3.0f); // 깨진 후 잔해
        Destroy(gameObject);     // 돌 원본
    }
}