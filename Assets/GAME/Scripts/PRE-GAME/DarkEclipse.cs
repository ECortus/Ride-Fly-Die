using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DarkEclipse : MonoBehaviour
{
    private static DarkEclipse Instance { get; set; }
    void Awake() => Instance = this;

    [SerializeField] private Image image;

    public static UniTask Play() => Instance.PlayAnim();
    public static UniTask PlayReverse() => Instance.PlayAnimReverse();
    
    private async UniTask PlayAnim()
    {
        image.color = new Color(0.25f, 0.25f,0.25f,1f);
        await image.DOColor(new Color(0.25f, 0.25f, 0.25f, 0f), 0.3f).AsyncWaitForCompletion();
    }
    
    private async UniTask PlayAnimReverse()
    {
        image.color = new Color(0.25f, 0.25f,0.25f,0f);
        await image.DOColor(new Color(0.25f, 0.25f, 0.25f, 1f), 0.3f).AsyncWaitForCompletion();
    }
}
