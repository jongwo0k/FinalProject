using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    private StartButtonEffect startButtonEffect; //버튼 이펙트 오브젝트 참조
    public void OnClickStart()
    {
        if (startButtonEffect != null)
            startButtonEffect.StopAndReset(); //버튼 클릭 시 멈춤

        SceneManager.LoadScene("HumanoidBoss"); // (임시) 나중에 FirstScene으로 수정
    }
}
