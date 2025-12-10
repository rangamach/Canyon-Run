using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get;private set; }

    //Scriptable Objects:
    [SerializeField] private PlayerSO playerSO;


    protected override void Awake()
    {
        base.Awake();

        //CreateServices();
    }
    private void Start()
    {
        CreateServices();
    }

    private void CreateServices()
    {
        this.PlayerService = new PlayerService(playerSO);
    }
}
