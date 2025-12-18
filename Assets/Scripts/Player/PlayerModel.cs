public class PlayerModel
{
    public int CurrentLives { get; private set; }
    private int maxLives;
    
    public PlayerModel(int m_Lives)
    {
        this.maxLives = m_Lives;

        SetCurrentLives(maxLives);
    }
    public void SetCurrentLives(int lives) => this.CurrentLives = lives;
    public void ResetCurrentLives() => this.CurrentLives = maxLives;
}
