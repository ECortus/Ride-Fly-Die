using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create buy-part types array")]
public class BuyPartParameters : ScriptableObject
{
    [field: SerializeField] public PartType[] Types { get; private set; }
    [field: SerializeField] public float DefaultPrice = 2f;
    [field: SerializeField] public float Multiple = 1.2f;
}
