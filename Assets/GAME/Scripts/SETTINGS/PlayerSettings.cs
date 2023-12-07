using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "sett", menuName = "Player Settings/Object")]
public class PlayerSettings : ScriptableObject
{
    public AudioMixer Mixer;
}
