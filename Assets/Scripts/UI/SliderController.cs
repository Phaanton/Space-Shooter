using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SliderController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Text valueText;
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider mySlider;

    void Start()
    {
        // Initialize the slider value and text
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    public void SetSFXVolume()
    {
        valueText.text = Mathf.Clamp(Mathf.RoundToInt(mySlider.value * 100), 0, 100) + "%";
        float sliderValue = mySlider.value;
        float volumeDb = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        myMixer.SetFloat("SFXVolume", volumeDb);
    }

    public void SetMusicVolume()
    {
        valueText.text = Mathf.Clamp(Mathf.RoundToInt(mySlider.value * 100), 0, 100) + "%";
        float sliderValue = mySlider.value;
        float volumeDb = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        myMixer.SetFloat("MusicVolume", volumeDb);
    }

    public void SetMasterVolume()
    {
        valueText.text = Mathf.Clamp(Mathf.RoundToInt(mySlider.value * 100), 0, 100) + "%";
        float sliderValue = mySlider.value;
        float volumeDb = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        myMixer.SetFloat("MasterVolume", volumeDb);
    }
}
