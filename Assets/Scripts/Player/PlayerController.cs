using System.Collections;
using UnityEngine;

public class PlayerController
{
    private PlayerView playerView;
    private PlayerModel playerModel;
    public PlayerController(PlayerView prefab)
    {
        playerModel = new PlayerModel(3);
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
    public int GetCurrentLives() => playerModel.CurrentLives;
    public void SetCurrentLives(int lives)
    {
        playerModel.SetCurrentLives(lives);

        if(IsDead())
        {
            PlayerDied();
        }
    }
    private bool IsDead() => playerModel.CurrentLives <= 0;
    private void PlayerDied()
    {
        if (!playerView.hasDied)
        {
            playerView.hasDied = true;
            playerView.rb.linearVelocity = Vector3.zero;
            playerView.audioSource.Pause();
            playerView.anim.SetTrigger("Death");
            GameService.Instance.SoundService.PlaySFX(SoundTypes.Death);
        }

        if (playerView.distanceTravelled > PlayerPrefs.GetFloat("Best"))
        {
            PlayerPrefs.SetFloat("Best", playerView.distanceTravelled);
            PlayerPrefs.Save();
        }

        GameService.Instance.UIService().ShowGameOverCoroutine();

        //GameService.Instance.UIService().GameOverUI();
    }
    public void ResetCurrentLives() => playerModel.ResetCurrentLives();
}
