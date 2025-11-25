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

    void Start()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Game Start");
    }
}