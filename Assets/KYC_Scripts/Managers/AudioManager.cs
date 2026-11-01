using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;

    [Header("bgm목록")]
    [SerializeField] AudioClip titleBGM;
    [SerializeField] AudioClip villageBGM;
    [SerializeField] AudioClip stageBGM;
    [SerializeField] AudioClip bossBGM;
    [SerializeField] AudioClip dogBossBGM;

    public float bgmSliderValue = 0.5f;
    public float sfxSliderValue = 0.5f;


    public void PlaySFX(AudioClip audioClip, float volume)
    {
        sfxSource.PlayOneShot(audioClip, volume);
    }

    public void PlayBGM(AudioClip audioClip, float volume = 1f)
    {
        // 이미 같은 BGM 재생 중이면 다시 시작 안 함
        if (bgmSource.clip == audioClip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();
        bgmSource.clip = audioClip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM);
    }

    public void PlayVillageBGM()
    {
        PlayBGM(villageBGM);
    }

    public void PlayStageBGM()
    {
        PlayBGM(stageBGM);
    }

    public void PlayBossBGM(float volume)
    {
        PlayBGM(bossBGM, volume);
    }

    public void PlayDogBossBGM()
    {
        PlayBGM(dogBossBGM);
    }
}