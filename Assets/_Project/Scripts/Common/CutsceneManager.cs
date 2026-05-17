using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Characters")]
    public PlayableDirector director; // 타임라인 컴포넌트
    public PlayerController player;      // 플레이어 컨트롤러
    public BossController boss;        // 보스 컨트롤러

    [Header("UI & Camera")]
    public GameObject gameplayUI;         // 체력바 등이 들어있는 UI 캔버스 오브젝트
    public CameraController customCamera; // 카메라 추적 스크립트
    public CinemachineBrain cineBrain;    // 메인 카메라에 자동으로 붙어있는 시네머신 브레인

    void Start()
    {
        // 게임이 시작되자마자 조작과 AI를 막기
        if (player != null) player.enabled = false;
        if (boss != null) boss.enabled = false;

        // 컷씬 중에는 체력바 UI를 숨기기
        if (gameplayUI != null) gameplayUI.SetActive(false);

        // 컷씬 중에는 커스텀 카메라 스크립트 끄기
        if (customCamera != null) customCamera.enabled = false;

        // 타임라인 종료 이벤트 구독
        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector pd)
    {
        // 조작 복구
        if (player != null) player.enabled = true;
        if (boss != null) boss.enabled = true;

        // 컷씬이 끝나면 UI 다시 보여주기
        if (gameplayUI != null) gameplayUI.SetActive(true);

        // 카메라 주도권 교대: 시네머신을 끄고, 커스텀 카메라
        if (cineBrain != null) cineBrain.enabled = false;
        if (customCamera != null) customCamera.enabled = true;

        director.stopped -= OnCutsceneEnd;
    }
}
