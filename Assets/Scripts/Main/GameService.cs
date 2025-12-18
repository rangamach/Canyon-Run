using UnityEngine;
using static UnityEngine.AudioSettings;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get;private set; }
    public SoundService SoundService { get; private set; }

    [Header("Services:")]
    [SerializeField] private UIService uiService;
    public UIService UIService() => this.uiService;

    //Scriptable Objects:
    [Header("Scriptable Objects:")]
    [SerializeField] private PlayerSO playerSO;
    [SerializeField] private SoundSO soundSO;

    [SerializeField] private AudioSource bgAudio;
    [SerializeField] private AudioSource sfxAudio;

    public bool onMobile { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        SetInputPlatform();
    }
    private void Start()
    {
        CreateServices();
    }

    private void CreateServices()
    {
        this.PlayerService = new PlayerService(playerSO);
        this.SoundService = new SoundService(soundSO,bgAudio,sfxAudio);
    }
    private void SetInputPlatform()
    {
        onMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }
}
