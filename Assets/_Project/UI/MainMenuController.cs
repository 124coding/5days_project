using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // 게임 시작 버튼을 누를 때 실행될 함수
    public void ClickStartButton()
    {
        // 괄호 안에는 Build Settings에 등록한 게임 씬의 이름을 정확히 적어줍니다.
        SceneManager.LoadScene("MainGame");
    }
}
