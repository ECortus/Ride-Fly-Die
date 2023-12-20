using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create grids-uploads")]
public class GridsUploads : ScriptableObject
{
    public PartType Cabins, Grids;
    
    [Serializable]
    public struct UploadStat
    {
        public PartStat[] Parts;
        public int LevelsOf;

        [Space] 
        public float RequireDistance;
        public Sprite Sprite;
    }

    [Serializable]
    public struct PartStat
    {
        public PartType Type;
        public Vector2Int IndexOffset;
    }

    public UploadStat[] Stats;

    public UploadStat CurrentStat
    {
        get
        {
            float max = Records.MaxDistance;

            for(int i = Stats.Length - 1; i >= 0; i--)
            {
                if (max >= Stats[i].RequireDistance)
                {
                    return Stats[i];
                }
            }

            return new UploadStat();
        }
    }
}
