using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CogBattle : NetworkBehaviour
{
    // [SyncVar]
    public int hp, maxHp, level;
    // [SyncVar]
    public bool isLured;
}
