using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gag", menuName = "Toontown/Gag", order = 1)]
public class Gag : ScriptableObject
{
    public GagTrack gagTrack;
    public int gagLevel, power, acc;
    public bool attackAll;
    
}

public enum GagTrack {TOON_UP, TRAP, LURE, SOUND, THROW, SQUIRT, DROP}
