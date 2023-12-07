using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    private PlayerSettings _settings;
    private AudioMixer Master => _settings.Mixer;

    private Toggle toggle;
    private GameObject offPart;
    
    private string Name = "MasterVolume";
    private bool Mode
    {
        get => SettingsModes.Volume;
        set => SettingsModes.Volume = value;
    }
    
    void Start()
    {
        _settings = Resources.Load<PlayerSettings>("SETTINGS/PlayerSettings");
        
        toggle = GetComponent<Toggle>();
        
        offPart = toggle.targetGraphic.gameObject;
        SetVolume(Mode);
    }
    
    public void SetVolume(bool value)
    {
        Mode = value;
        toggle.isOn = Mode;
        
        Master.SetFloat(Name, Mode ? 0f : -80f);
        
        if (!Mode)
        {
            offPart.SetActive(true);
        }
        else
        {
            offPart.SetActive(false);
        }
    }
}
