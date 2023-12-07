using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    public void PlusGold()
    {
        Gold.Plus(500);
    }
    
    public void PlusGem()
    {
        Gem.Plus(500);
    }
}
