using UnityEngine;

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
    public void RestartPlayer() => playerView.RestartPlayer();
    public void SetHasDied(bool toggle) => playerView.hasDied = toggle;
    public AudioSource PlayerAudio() => playerView.PlayerAudio();
}
