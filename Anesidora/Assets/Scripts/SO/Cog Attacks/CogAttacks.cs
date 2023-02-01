using UnityEngine;

[CreateAssetMenu(fileName = "CogAttack", menuName = "Toontown/CogAttack", order = 1)]
public class CogAttacks : ScriptableObject
{
    public CogAttack cogAttack;
    public int[] damages = new int [5];
    public int[] accuracy = new int [5];
    public int[] frequency = new int [5];

}

public enum CogAttack {Canned, Dowsize, PinkSlip, FreezeAssets}
