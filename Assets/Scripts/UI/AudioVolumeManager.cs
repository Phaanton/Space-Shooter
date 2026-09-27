using UnityEngine;
using UnityEngine.Audio;

public class AudioVolumeManager : MonoBehaviour
{
    // Singleton instance
    public static AudioVolumeManager Instance { get; private set; }

    [SerializeField] private AudioMixer myMixer;

    void Awake()
    {
        // Implementação do Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAndApplyVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadAndApplyVolumes()
    {
        // Carrega os valores salvos (ou usa padrão 0.75 = 75%)
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        SetVolume("MasterVolume", masterVolume);
        SetVolume("MusicVolume", musicVolume);
        SetVolume("SFXVolume", sfxVolume);
    }

    public void SetVolume(string parameter, float linearValue)
    {
        if (myMixer == null)
        {
            Debug.LogWarning("AudioMixer não atribuído no AudioVolumeManager!");
            return;
        }

        float volumeDb = Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20;
        myMixer.SetFloat(parameter, volumeDb);
    }

    // Métodos opcionais para facilitar o uso
    public void SetMasterVolume(float value)
    {
        SetVolume("MasterVolume", value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        SetVolume("MusicVolume", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        SetVolume("SFXVolume", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // Método para obter volumes salvos (útil para inicializar sliders)
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", 0.75f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 0.75f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.75f);
    }
}