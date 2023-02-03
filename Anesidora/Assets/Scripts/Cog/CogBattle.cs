using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CogBattle : NetworkBehaviour
{
    [SyncVar]
    public int hp, maxHp, level, luredRounds;
    [SyncVar]
    public bool isLured, isTrapped, isDead;
}
