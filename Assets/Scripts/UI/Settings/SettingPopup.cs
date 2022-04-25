using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider masterSFXSlider;

    public LoadoutState loadoutState;
    public DataDeleteConfirmation confirmationPopup;

    protected float m_MasterVolume;
    protected float m_MusicVolume;
    protected float m_MasterSFXVolume;

    protected const float k_MinVolume = -80f;
    protected const string k_MasterVolumeFloatName = "MasterVolume";
    protected const string k_MusicVolumeFloatName = "MusicVolume";
    protected const string k_MasterSFXVolumeFloatName = "MasterSFXVolume";
    
    public void Open()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void UpdateUI()
    {
		masterSlider.value = 1.0f - (m_MasterVolume / k_MinVolume);
        musicSlider.value = 1.0f - (m_MusicVolume / k_MinVolume);
        masterSFXSlider.value = 1.0f - (m_MasterSFXVolume / k_MinVolume);
    }

    public void DeleteData()
    {
        confirmationPopup.Open(loadoutState);
    }


    public void MasterVolumeChangeValue(float value)
    {
        m_MasterVolume = k_MinVolume * (1.0f - value);
       PlayerData.instance.masterVolume = m_MasterVolume;
    }

    public void MusicVolumeChangeValue(float value)
    {
        m_MusicVolume = k_MinVolume * (1.0f - value);
       PlayerData.instance.musicVolume = m_MusicVolume;
    }

    public void MasterSFXVolumeChangeValue(float value)
    {
        m_MasterSFXVolume = k_MinVolume * (1.0f - value);
      PlayerData.instance.masterSFXVolume = m_MasterSFXVolume;
    }
}
