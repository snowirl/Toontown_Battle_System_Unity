using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cog", menuName = "Toontown/Cog", order = 1)]
public class Cog : ScriptableObject
{
    public string cogName;
    public CogType cogType;
    public int minCogLevel, maxCogLevel;
    public List<CogAttack> cogAttacks = new List<CogAttack>();
}

public enum CogType {Sellbot, Cashbot, Lawbot, Bossbot}
