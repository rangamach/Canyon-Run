using UnityEngine;

public enum SoundTypes
{
    ButtonClick,
    Background,
    Running,
    Jump,
    Death,
}
public class SoundService
{
    private SoundSO SO;
    private AudioSource bgAS;
    private AudioSource sfxAS;
    private float bgMax;
    private float sfxMax;
    public SoundService(SoundSO soundSO, AudioSource bg,AudioSource sfx)
    {
        this.SO = soundSO;
        this.bgAS = bg;
        this.sfxAS = sfx;

        bgMax = bgAS.volume;
        sfxMax = sfxAS.volume;

        PlayBG();
    }
    private void PlayBG()
    {
        AudioClip clip = GetAudioClip(SoundTypes.Background);

        bgAS.clip = clip;
        bgAS.Play();
    }
    public void PlaySFX(SoundTypes type)
    {
        AudioClip clip = GetAudioClip(type);

        if(clip)
        {
            sfxAS.PlayOneShot(clip);
        }
        else
        {
            Debug.Log(type + " audio not found...");
        }
    }
    public AudioClip GetClip(SoundTypes type)
    {
        return GetAudioClip(type);
    }
    private AudioClip GetAudioClip(SoundTypes type)
    {
        foreach (Sounds sound in SO.SoundsList)
        {
            if(sound.Type == type)
            {
                return sound.Clip;
            }
        }
        return null;
    }
    public void UpdateAudioVolumes(float vol)
    {
        bgAS.volume = vol * bgMax;
        sfxAS.volume = vol * sfxMax;
    }
}
