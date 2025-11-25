using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("HumanoidBoss"); // (임시) 나중에 FirstScene으로 수정
    }
}
