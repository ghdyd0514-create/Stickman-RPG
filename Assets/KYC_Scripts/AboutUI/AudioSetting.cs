using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    [Header("슬라이더 0.5일 때 기준 볼륨")]
    [SerializeField] float bgmBaseVolume = -15f;
    [SerializeField] float sfxBaseVolume = -3f;

    [Header("0.5 -> 1.0일 때 얼마나 더 키울지")]
    [SerializeField] float maxBoostDB = 10f;

    void Start()
    {
        bgmSlider.SetValueWithoutNotify(
            Managers.instance.audioManager.bgmSliderValue
        );

        sfxSlider.SetValueWithoutNotify(
            Managers.instance.audioManager.sfxSliderValue
        );
    }

    public void SetBGMVolume(float value)
    {
        Managers.instance.audioManager.bgmSliderValue = value;

        if (value <= 0.001f)
        {
            audioMixer.SetFloat("BGMVolume", -80f);
            return;
        }

        float dbOffset = GetDBOffset(value);

        audioMixer.SetFloat(
            "BGMVolume",
            bgmBaseVolume + dbOffset
        );
    }

    public void SetSFXVolume(float value)
    {
        Managers.instance.audioManager.sfxSliderValue = value;
        
        if (value <= 0.001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
            return;
        }

        float dbOffset = GetDBOffset(value);

        audioMixer.SetFloat(
            "SFXVolume",
            sfxBaseVolume + dbOffset
        );
    }

    float GetDBOffset(float value)
    {
        // 0.5가 기준점
        if (value < 0.5f)
        {
            // 0 ~ 0.5
            // 기준보다 점점 작아짐
            float normalized = value / 0.5f;

            return Mathf.Lerp(-30f, 0f, normalized);
        }
        else
        {
            // 0.5 ~ 1
            // 기준보다 점점 커짐
            float normalized = (value - 0.5f) / 0.5f;

            return Mathf.Lerp(0f, maxBoostDB, normalized);
        }
    }
}