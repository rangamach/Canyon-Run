using System.Collections;
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
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumePercentText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Image[] hearts;
    [SerializeField] private RectTransform pcControls;
    [SerializeField] private RectTransform mobileControls;

    private SoundService soundService;
    private PlayerService playerService;

    private bool isPausedBySystem;


    private void Awake()
    {
        bestDistanceStartText.text = $"Best Distance: {PlayerPrefs.GetFloat("Best")} m";
        startMenuRT.gameObject.SetActive(true);
    }

    private void Start()
    {
        ResetHearts();

        pauseButton.gameObject.GetComponent<Button>().onClick.AddListener(OnPauseClicked);
        resumeButton.gameObject.GetComponent<Button>().onClick.AddListener(OnResumeClicked);
        replayButton.gameObject.GetComponent<Button>().onClick.AddListener(RestartButton);
        playButton.gameObject.GetComponent<Button>().onClick.AddListener(RestartButton);
        exitButton.gameObject.GetComponent<Button>().onClick.AddListener(BackToStart);
        exitButtonGO.gameObject.GetComponent<Button>().onClick.AddListener(BackToStart);
        exitButtonStart.GetComponent<Button>().onClick.AddListener(StartExit);
        controlsButton.GetComponent<Button>().onClick.AddListener(ControlsButton);
        controlsBackButton.GetComponent<Button>().onClick.AddListener(ShowStartMenu);

        this.soundService = GameService.Instance.SoundService;
        this.playerService = GameService.Instance.PlayerService;

        volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChange);

        volumeSlider.value = 1f;

        ChooseControls();
    }
    private void ChooseControls()
    {
        if(GameService.Instance.onMobile)
        {
            mobileControls.gameObject.SetActive(true);
            pcControls.gameObject.SetActive(false);


            jumpButton.gameObject.SetActive(true);
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else
        {
            mobileControls.gameObject.SetActive(false);
            pcControls.gameObject.SetActive(true);

            jumpButton.gameObject.SetActive(false);
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            PauseFromSystem();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PauseFromSystem();
        }
    }

    private void PauseFromSystem()
    {
        // Already paused ? do nothing
        if (isPausedBySystem) return;

        // Don't pause in start menu or game over
        if (startMenuRT.gameObject.activeInHierarchy) return;
        if (gameOverRT.gameObject.activeInHierarchy) return;

        isPausedBySystem = true;

        // Pause audio & time
        playerService.PlayerAudio().Pause();
        Time.timeScale = 0f;

        // Show pause UI
        pauseRT.gameObject.SetActive(true);
    }
    public void ResetHearts()
    {
        foreach (Image img in hearts)
        {
            img.gameObject.SetActive(true);
        }
    }
    public bool IsLeftPressed() => leftButton.IsPressed;
    public bool IsRightPressed() => rightButton.IsPressed;
    public bool IsJumpPressed() => jumpButton.IsPressed;
    private void OnPauseClicked()
    {
        GameService.Instance.SoundService.PlaySFX(SoundTypes.ButtonClick);
        PauseFromSystem();
    }
    private void OnResumeClicked()
    {
        if (!isPausedBySystem) return;

        isPausedBySystem = false;

        playerService.PlayerAudio().Play();
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
    private void OnVolumeSliderValueChange(float vol)
    {
        volumePercentText.text = $"{vol * 100f:F2}%";

        soundService.UpdateAudioVolumes(vol);
        playerService.PlayerAudio().volume = 0.25f * vol;
    }
    public void LoseHeart()
    {
        int currentLives = GameService.Instance.PlayerService.GetCurrentLives();

        for(int i = 0;i<hearts.Length;i++)
        {
            if(i>currentLives - 1)
            {
                hearts[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowGameOverCoroutine() => StartCoroutine(ShowGameOverUI());

    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(3f);

        GameOverUI();
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
