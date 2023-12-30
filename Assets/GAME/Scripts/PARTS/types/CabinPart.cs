using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinPart : Part
{
    [SerializeField] private PlayerChelDoll doll;
    
    public override void CrashPart()
    {
        base.CrashPart();
        if (doll)
        {
            doll.transform.SetParent(null);
            doll.TurnRag(2000f);
        }
    }

    public override void RepairPart()
    {
        base.RepairPart();
        if (doll)
        {
            doll.transform.SetParent(transform);
            doll.SetDefault();
        }
    }

    public override void DestroyPart()
    {
        Destroy(doll.gameObject);
        base.DestroyPart();
    }
    
    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Default,
            0,
            Vector3.zero, 
            transform.localPosition,
            Mass
        );

        return modif;
    }

    public override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hit"))
        {
            GameManager.FinishGame();
        }
    }
}
