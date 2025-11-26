using UnityEngine;
using UnityEngine.SceneManagement;

// 모든 Scene에 유지
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 시작
    void Start()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Game Start");
    }

    // 스테이지 클리어 (보스 처치 성공)
    // Scene 자동 전환? or 클리어 UI 표시 후 Next 버튼으로 전환
    public void StageClear()
    {
        ClearProjectiles();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex < SceneManager.sceneCountInBuildSettings - 1) // 다음 씬 (Build Settings)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            // 전체 Game Clear
        }
    }

    // 남은 투사체 제거
    public void ClearProjectiles()
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile"); // Tag 확인, projectileManager따로? ObjectPooling? Event?
        foreach (GameObject p in projectiles)
        {
            Destroy(p);
        }
    }
}