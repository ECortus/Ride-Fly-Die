using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class MergeCell : MonoBehaviour
{
    public static event Action OnUpdateState;
        
    public static MergeCell SelectedCell { get; private set; }
    public Part Part { get; private set; }

    private int _id;
    private bool _showed;

    [SerializeField] private Image spriteShow;
    [SerializeField] private TextMeshProUGUI levelText;

    // private void Awake()
    // {
    //     if (!Part) SetUI(false);
    // }

    private void OnMouseOver()
    {
        SelectedCell = this;
    }

    private void OnMouseExit()
    {
        SelectedCell = null;
    }

    public void Registry(Part part)
    {
        Part = part;
        
        SetUI(true);
        
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistry()
    {
        Part = null;
        
        SetUI(false);
        
        OnUpdateState?.Invoke();
        Save();
    }

    private void Save()
    {
        SaveManager.Save($"MergeCell{_id}", Part);
    }

    public void SetUI(bool state)
    {
        // if(!_spriteShow) _spriteShow = GetComponentInChildren<Image>();
        // if(!_levelText) _levelText = GetComponentInChildren<TextMeshProUGUI>();

        if (Part && state)
        {
            spriteShow.gameObject.SetActive(true);
            levelText.transform.parent.gameObject.SetActive(true);

            spriteShow.sprite = Part.Sprite;
            levelText.text = Part.Level.ToString();
        }
        else
        {
            spriteShow.gameObject.SetActive(false);
            levelText.transform.parent.gameObject.SetActive(false);
        }
    }
        
    public void Load(int id)
    {
        if (transform.GetChild(0).childCount > 0)
        {
            foreach (Transform VARIABLE in transform.GetChild(0))
            {
                Destroy(VARIABLE.gameObject);
            }
        }
        
        _id = id;
        Part = SaveManager.Load($"MergeCell{_id}");

        if (Part == null)
        {
            SetUI(false);
            return;
        }
            
        MergeGrid.Instance.SpawnPartToCell(Part, this);
        OnUpdateState?.Invoke();
    }
}
