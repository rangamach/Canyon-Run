public class PlayerService
{
    private PlayerController playerController;
    public PlayerService(PlayerSO playerSO)
    {
        this.playerController = new PlayerController(playerSO.Prefab);
    }
}
