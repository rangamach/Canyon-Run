using UnityEngine;

public class PlayerService
{
    private PlayerController playerController;
    public PlayerService(PlayerSO playerSO)
    {
        this.playerController = new PlayerController(playerSO.Prefab);
    }
    public void RestartPlayer() => playerController.RestartPlayer();
    public void SetHasDied(bool toggle) => playerController.SetHasDied(toggle);
    public AudioSource PlayerAudio() => playerController.PlayerAudio();
}
