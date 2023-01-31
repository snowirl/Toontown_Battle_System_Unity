using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBattle : NetworkBehaviour
{
    [SyncVar]
    public int hp, maxHp;
}
