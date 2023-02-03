using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CogAttack", menuName = "Toontown/CogAttack", order = 1)]
public class CogAttack : ScriptableObject
{
    public AttackName attackName;
    public int[] damages = new int [5];
    public int[] accuracy = new int [5];
    public int[] frequency = new int [5];
    public bool areaOfEffect;

}

public enum AttackName {Canned, Dowsize, PinkSlip, FreezeAssets, PoundKey}
