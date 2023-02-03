using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCalculationCog
{
    public CogAttack cogAttack;
    public int dmg;
    public int whichCog;
    public int whichTarget;
    public List<bool> didHitList = new List<bool>();
}
