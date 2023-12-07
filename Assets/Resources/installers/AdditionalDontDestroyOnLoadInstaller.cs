using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class AdditionalDontDestroyOnLoadInstaller : MonoInstaller
{
    [SerializeField] private GameObject[] toInstall;

    public override void InstallBindings()
    {
        foreach(var installable in toInstall)
        {
            DontDestroyOnLoad(Instantiate(installable));
        }
    }
}
