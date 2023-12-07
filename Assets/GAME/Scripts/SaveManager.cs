using System.Linq;
using UnityEngine;
using Zenject;

[DefaultExecutionOrder(-10)]
public class SaveManager : MonoBehaviour
{
    private static SaveManager Instance { get; set; }

    public static Part Load(string key)
    {
        string type = PlayerPrefs.GetString(key);
        
        if (string.IsNullOrEmpty(type)) return null;
        
        int level = PlayerPrefs.GetInt($"Level{key}");
        return Instance.partTypes.First(t => t.name == type).PartLevels[level];
    }

    public static void Save(string key, Part part)
    {
        if (part == null)
        {
            PlayerPrefs.SetString(key, "");
            return;
        }
        PlayerPrefs.SetString(key, part.Type.name);
        PlayerPrefs.SetInt($"Level{key}", part.Level);
    }

    [SerializeField] private PartType[] partTypes;
    
    [Inject] private void Awake()
    {
        Instance = this;
    }
}