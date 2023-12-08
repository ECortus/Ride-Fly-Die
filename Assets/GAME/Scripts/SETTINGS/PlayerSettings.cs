using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "sett", menuName = "Player Settings/Object")]
public class PlayerSettings : ScriptableObject
{
    [field: SerializeField] public AudioMixer Mixer { get; private set; }

    [field: SerializeField] public GridMode Mod { get; private set; }
}

public enum GridMode
{
    Model, Sprite
}
