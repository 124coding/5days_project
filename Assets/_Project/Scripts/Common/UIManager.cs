using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class UIManager : MonoBehaviour
{
    [Header("--- Player UI ---")]
    public Slider playerHpBar;

    [Header("--- Boss UI ---")]
    public Slider bossHpBar;
    public Slider bossStanceBar;

    [Header("--- Result UI ---")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    public CanvasGroup resultCanvasGroup;
    public float fadeDuration = 1.5f;

    // 플레이어 체력 업데이트
    public void UpdatePlayerHP(float currentHp, float maxHp)
    {
        if (playerHpBar != null)
        {
            playerHpBar.value = currentHp / maxHp;
        }
    }

    // 보스 체력 업데이트
    public void UpdateBossHP(float currentHp, float maxHp)
    {
        if (bossHpBar != null)
        {
            bossHpBar.value = currentHp / maxHp;
        }
    }

    // 보스 강인도 업데이트
    public void UpdateBossStance(float currentStance, float maxStance)
    {
        if (bossStanceBar != null)
        {
            bossStanceBar.value = currentStance / maxStance;
        }
    }

    // 승리 화면 띄우기
    public void ShowWinUI()
    {
        resultPanel.SetActive(true);
        resultText.text = "YOU WIN";
        resultText.color = Color.green;

        Time.timeScale = 0f;
        StartCoroutine(FadeInResultUI());
    }

    // 패배 화면 띄우기
    public void ShowLoseUI()
    {
        resultPanel.SetActive(true);
        resultText.text = "YOU LOSE";
        resultText.color = Color.red;

        Time.timeScale = 0f;
        StartCoroutine(FadeInResultUI());
    }

    private IEnumerator FadeInResultUI()
    {
        float currentTime = 0f;
        resultCanvasGroup.alpha = 0f; // 시작할 땐 완전 투명하게

        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;

            // 0에서 1까지 서서히 값을 올립니다.
            resultCanvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);

            yield return null;
        }

        resultCanvasGroup.alpha = 1f;
    }

    // 재시작 버튼 클릭 시
    public void ClickRestart()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 메인 메뉴 버튼 클릭 시
    public void ClickMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}
