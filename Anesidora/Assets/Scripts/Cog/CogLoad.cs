using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CogLoad : NetworkBehaviour
{
    [SyncVar]
    public Cog cog;

    [SyncVar]
    public int level;
}
