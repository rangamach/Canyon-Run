public class PlayerController
{
    private PlayerView playerView;
    public PlayerController(PlayerView prefab)
    {
        InitializePlayerView(prefab);
    }
    private void InitializePlayerView(PlayerView prefab)
    {
        playerView = UnityEngine.Object.Instantiate(prefab);
        playerView.SetController(this);
    }
}
