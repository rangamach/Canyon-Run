using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIService : MonoBehaviour
{
    [Header("GameplayUI")]
    [SerializeField] private RectTransform gameplayRT;
    [SerializeField] private ButtonState leftButton; 
    [SerializeField] private ButtonState rightButton;
    [SerializeField] private ButtonState jumpButton;
    [SerializeField] private ButtonState pauseButton;
    [SerializeField] private RectTransform pauseRT;
    [SerializeField] private ButtonState resumeButton;
    [SerializeField] private ButtonState exitButton;
    [SerializeField] private RectTransform gameOverRT;
    [SerializeField] private ButtonState replayButton;
    [SerializeField] private ButtonState exitButtonGO;
    [SerializeField] private RectTransform startMenuRT;
    [SerializeField] private ButtonState playButton;
    [SerializeField] private ButtonState exitButtonStart;
    [SerializeField] private TextMeshProUGUI bestDistanceStartText;
    [SerializeField] private TextMeshProUGUI bestDistanceGameOverText;
    [SerializeField] private ButtonState controlsButton;
    [SerializeField] private ButtonState controlsBackButton;
    [SerializeField] private RectTransform controlsRT;

    [SerializeField] private TextMeshProUGUI distanceText;

    private float deltaTime = 0f;

    private void Awake()
    {
        bestDistanceStartText.text = $"Best Distance: {PlayerPrefs.GetFloat("Best")} m";
        startMenuRT.gameObject.SetActive(true);
    }

    private void Start()
    {
        pauseButton.gameObject.GetComponent<Button>().onClick.AddListener(OnPauseClicked);
        resumeButton.gameObject.GetComponent<Button>().onClick.AddListener(OnResumeClicked);
        replayButton.gameObject.GetComponent<Button>().onClick.AddListener(RestartButton);
        playButton.gameObject.GetComponent<Button>().onClick.AddListener(RestartButton);
        exitButton.gameObject.GetComponent<Button>().onClick.AddListener(BackToStart);
        exitButtonGO.gameObject.GetComponent<Button>().onClick.AddListener(BackToStart);
        exitButtonStart.GetComponent<Button>().onClick.AddListener(StartExit);
        controlsButton.GetComponent<Button>().onClick.AddListener(ControlsButton);
        controlsBackButton.GetComponent<Button>().onClick.AddListener(ShowStartMenu);
    }
    public bool IsLeftPressed() => leftButton.IsPressed;
    public bool IsRightPressed() => rightButton.IsPressed;
    public bool IsJumpPressed() => jumpButton.IsPressed;
    private void OnPauseClicked()
    {
        GameService.Instance.PlayerService.PlayerAudio().Pause();

        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);
        if (gameOverRT.gameObject.activeInHierarchy) return;

        Time.timeScale = 0f;

        pauseRT.gameObject.SetActive(true);
    }
    private void OnResumeClicked()
    {
        GameService.Instance.PlayerService.PlayerAudio().Play();

        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);

        pauseRT.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }
    public void UpdateDistanceText(float distance)
    {
        if(distanceText)
        {
            distanceText.text = $"{distance:F1} m";
        }
    }
    private void RestartButton()
    {
        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);

        gameOverRT.gameObject.SetActive(false);
        startMenuRT.gameObject.SetActive(false);
        GameService.Instance.PlayerService.RestartPlayer();

        bestDistanceStartText.text = $"Best Distance: {PlayerPrefs.GetFloat("Best"):F1} m";

        gameplayRT.gameObject.SetActive(true);

        Time.timeScale = 1f;
    }
    public void GameOverUI()
    {
        bestDistanceGameOverText.text = $"Best Distance: {PlayerPrefs.GetFloat("Best"):F1} m";

        gameOverRT.gameObject.SetActive(true);
    }
    private void BackToStart()
    {
        GameService.Instance.PlayerService.RestartPlayer();
        GameService.Instance.PlayerService.SetHasDied(true);

        bestDistanceStartText.text = $"Best Distance: {PlayerPrefs.GetFloat("Best"):F1} m";

        ShowStartMenu();
    }
    private void ControlsButton()
    {
        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);

        startMenuRT.gameObject.SetActive(false);
        controlsRT.gameObject.SetActive(true);
    }
    private void ShowStartMenu()
    {
        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);

        gameOverRT.gameObject.SetActive(false);
        pauseRT.gameObject.SetActive(false);
        controlsRT.gameObject.SetActive(false);
        gameplayRT.gameObject.SetActive(false);

        startMenuRT.gameObject.SetActive(true);
    }
    private void StartExit()
    {
        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
