using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleCell : NetworkBehaviour
{
    public List<GameObject> toonsPending = new List<GameObject>();
    public List<GameObject> toonsPendingReady = new List<GameObject>();
    public List<GameObject> cogsPending = new List<GameObject>();
    public List<GameObject> cogsPendingReady = new List<GameObject>();
    public List<GameObject> toons = new List<GameObject>();
    public List<GameObject> cogs = new List<GameObject>();
    public readonly SyncList<uint> toonIDs = new SyncList<uint>();
    public readonly SyncList<uint> cogIDs = new SyncList<uint>();
    public List<Transform> toonPositions, toonPendingPositions, cogPositions, cogPendingPositions = new List<Transform>();

    [SyncVar]
    public BattleState battleState;
    public BoxCollider cellCollider;

    [SyncVar (hook = nameof(EnableCollider))]
    public bool colliderEnabled;
    public BattleCalculator battleCalculator;

    public override void OnStartClient()
    {
        if(!isClientOnly)
        {
            print("Is not client only");
            return;
        }

        SetUpLists();
    }

    public override void OnStartServer()
    {
        SetUpLists();
    }

    void SetUpLists()
    {
        toonIDs.Callback += OnToonListUpdated;

        cogIDs.Callback += OnCogListUpdated;

        for (int index = 0; index < toonIDs.Count; index++)
        {
            OnToonListUpdated(SyncList<uint>.Operation.OP_ADD, index, new uint(), toonIDs[index]);
        }

        for (int index = 0; index < cogIDs.Count; index++)
        {
            OnCogListUpdated(SyncList<uint>.Operation.OP_ADD, index, new uint(), cogIDs[index]);
        }
    }

    

    void OnToonListUpdated(SyncList<uint>.Operation op, int index, uint oldItem, uint newItem)
    {
        GameObject addedToon = null;
        GameObject removedToon = null;

        if(NetworkClient.spawned.TryGetValue(newItem, out NetworkIdentity id))
        {
            addedToon = id.gameObject;
        }

        if(NetworkClient.spawned.TryGetValue(oldItem, out NetworkIdentity netId))
        {
            removedToon = netId.gameObject;
        }

        switch (op)
        {
            case SyncList<uint>.Operation.OP_ADD:
                // index is where it was added into the list
                toons.Add(addedToon);
                // newItem is the new item
                break;
            case SyncList<uint>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<uint>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                toons.Remove(removedToon);
                // oldItem is the item that was removed
                break;
            case SyncList<uint>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<uint>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

    void OnCogListUpdated(SyncList<uint>.Operation op, int index, uint oldItem, uint newItem)
    {
        GameObject addedCog = null;
        GameObject removedCog = null;

        if(NetworkClient.spawned.TryGetValue(newItem, out NetworkIdentity id))
        {
            addedCog = id.gameObject;
        }

        if(NetworkClient.spawned.TryGetValue(oldItem, out NetworkIdentity netId))
        {
            removedCog = netId.gameObject;
        }

        switch (op)
        {
            case SyncList<uint>.Operation.OP_ADD:
                // index is where it was added into the list
                cogs.Add(addedCog);
                // newItem is the new item
                break;
            case SyncList<uint>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncList<uint>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                cogs.Remove(removedCog);
                // oldItem is the item that was removed
                break;
            case SyncList<uint>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<uint>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }

    [Command (requiresAuthority = false)]
    public void CmdAddToonPending(GameObject toon)
    {
        if(toonsPending.Count + toons.Count < 4)
        {
            toonsPending.Add(toon);
        }
        else
        {
            print("There are already 4 Toons in battle. Toon Cannot join.");
            return;
        }

        int count = toonsPending.Count - 1;

        TargetMoveToBattleCell(toon.GetComponent<NetworkIdentity>().connectionToClient, toon, toonPendingPositions[count].position, true);

        // StartCoroutine(MovePlayerToBattleCell(toon, toonPendingPositions[count].position, true, true));

        if(battleState == BattleState.IDLE) // if that was the first toon to join we start.
        {
            battleCalculator.StartBattle();
        }

    }

    [Command (requiresAuthority = false)]
    public void CmdAddCogPending(GameObject cog)
    {
        if(cogsPending.Count + cogs.Count < 4)
        {
            cogsPending.Add(cog);
        }
        else
        {
            print("There are already 4 Cogs in battle. Cog Cannot join.");
            return;
        }

        int count = cogsPending.Count - 1;

        cog.GetComponent<CogMove>().isBusy = true;
        cog.GetComponent<CogMove>().agent.enabled = false;

        StartCoroutine(MoveCogToBattleCell(cog, cogPendingPositions[count].position, true));
    }

    [Server]
    public void AddToons()
    {
        var tempList = new List<GameObject>(toonsPendingReady);

        foreach(GameObject g in tempList)
        {
            // toons.Add(g); // We have to use network ID to find player instead and the list will find the gameObject
            toonIDs.Add(g.GetComponent<NetworkIdentity>().netId);
            toonsPending.Remove(g);
            toonsPendingReady.Remove(g);
        }

        UpdatePositions(); 

        int index = 0;

        foreach(GameObject g in toons)
        {
            TargetMoveToBattleCell(g.GetComponent<NetworkIdentity>().connectionToClient, g, toonPositions[index].position, false);
            index++;
        }
    }

    [Server]
    public void AddCogs()
    {
        var tempList = new List<GameObject>(cogsPendingReady);

        foreach(GameObject g in tempList)
        {
            // cogs.Add(g);
            cogIDs.Add(g.GetComponent<NetworkIdentity>().netId);
            cogsPending.Remove(g);
            cogsPendingReady.Remove(g);
        }

        int index = 0;

        UpdatePositions(); 
        
        foreach(GameObject g in cogs)
        {
            StartCoroutine(MoveCogToBattleCell(g, cogPositions[index].position, false));
            index++;
        }
    }

    public void UpdatePositions() // Needs to run on local player since these gameObjects do not have Network Identities...
    {
        if(toons.Count == 1)
        {
            toonPositions[0].transform.localPosition = new Vector3(-5,0,0);
        }
        else if(toons.Count == 2)
        {
            toonPositions[0].transform.localPosition = new Vector3(-5,0,-1);
            toonPositions[1].transform.localPosition = new Vector3(-5,0,1);
        }
        else if(toons.Count == 3)
        {
            toonPositions[0].transform.localPosition = new Vector3(-5,0,-2);
            toonPositions[1].transform.localPosition = new Vector3(-5,0,0);
            toonPositions[2].transform.localPosition = new Vector3(-5,0,2);
        }
        else if(toons.Count == 4)
        {
            toonPositions[0].transform.localPosition = new Vector3(-5,0,-3);
            toonPositions[1].transform.localPosition = new Vector3(-5,0,-1);
            toonPositions[2].transform.localPosition = new Vector3(-5,0,1);
            toonPositions[3].transform.localPosition = new Vector3(-5,0,3);
        }

        if(cogs.Count == 1)
        {
            cogPositions[0].transform.localPosition = new Vector3(5,0,0);
        }
        else if(cogs.Count == 2)
        {
            cogPositions[0].transform.localPosition = new Vector3(5,0,-1);
            cogPositions[1].transform.localPosition = new Vector3(5,0,1);
        }
        else if(cogs.Count == 3)
        {
            cogPositions[0].transform.localPosition = new Vector3(5,0,-2);
            cogPositions[1].transform.localPosition = new Vector3(5,0,0);
            cogPositions[2].transform.localPosition = new Vector3(5,0,2);
        }
        else if(cogs.Count == 4)
        {
            cogPositions[0].transform.localPosition = new Vector3(5,0,-3);
            cogPositions[1].transform.localPosition = new Vector3(5,0,-1);
            cogPositions[2].transform.localPosition = new Vector3(5,0,1);
            cogPositions[3].transform.localPosition = new Vector3(5,0,3);
        }
    }

    [TargetRpc]
    void TargetMoveToBattleCell(NetworkConnection conn, GameObject player, Vector3 battlePos, bool isPending)
    {
        StartCoroutine(MoveToonToBattleCell(player, battlePos, isPending));
    }

    IEnumerator MoveToonToBattleCell(GameObject player, Vector3 battlePos, bool isPending)
    {
        var pos = new Vector3(battlePos.x, battlePos.y + 1, battlePos.z);
        float distance = Vector3.Distance(player.transform.position,pos);
        var step = (5) * Time.deltaTime;

        player.transform.LookAt(new Vector3(battlePos.x, player.transform.position.y, battlePos.z));

        while(distance > .25f)
        {
            distance = Vector3.Distance(player.transform.position, pos);
            player.transform.position = Vector3.MoveTowards(player.transform.position, pos, step);
            yield return null;
        }

        player.transform.LookAt(new Vector3(cogPendingPositions[0].position.x, player.transform.position.y,cogPendingPositions[0].position.z));
        player.transform.position = pos;

        CmdPlayerReady(player, isPending);

        if(isPending)
        {
            player.gameObject.GetComponent<PlayerBattle>().battleCell = this.gameObject;
        }
    }

    IEnumerator MoveCogToBattleCell(GameObject cog, Vector3 battlePos, bool isPending)
    {
        var pos = new Vector3(battlePos.x, battlePos.y + 1, battlePos.z);
        float distance = Vector3.Distance(cog.transform.position,pos);
        var step = (5) * Time.deltaTime;

        cog.transform.LookAt(new Vector3(battlePos.x, cog.transform.position.y, battlePos.z));

        while(distance > .25f)
        {
            distance = Vector3.Distance(cog.transform.position, pos);
            cog.transform.position = Vector3.MoveTowards(cog.transform.position, pos, step);
            yield return null;
        }

        cog.transform.localRotation = Quaternion.Euler(0,0,0);
        cog.transform.position = pos;

        cog.transform.LookAt(new Vector3(toonPendingPositions[0].position.x, cog.transform.position.y,cogPendingPositions[0].position.z));

        CogReady(cog, isPending);
    }

    [Command (requiresAuthority = false)]
    void CmdPlayerReady(GameObject player, bool isPending)
    {
        if(isPending)
        {
            print("Player is in the pending position.");

            toonsPendingReady.Add(player);

            if(battleState == BattleState.IDLE || battleState == BattleState.PLAYER_CHOOSE || battleState == BattleState.START)
            {
                AddToons();
            }
            
        }
        else // If we are not pending then we are just moving the players to the right grid. Or we can start the game when toon is first.
        {
            if(battleState == BattleState.START)
            {
               BattleStart(); // Check if battle can start.
            }
            else if(battleState == BattleState.PLAYER_CHOOSE)
            {
               battleCalculator.TargetPlayerChoose(player.GetComponent<NetworkIdentity>().connectionToClient); // update HP ui
            }

            
        }
    }

    [Server]
    void CogReady(GameObject cog, bool isPending)
    {
        if(isPending)
        {
            cogsPendingReady.Add(cog);

            if(battleState == BattleState.IDLE || battleState == BattleState.PLAYER_CHOOSE || battleState == BattleState.START)
            {
                AddCogs();
            }
        }
        else // If we are not pending then we are just moving the players to the right grid. Or we can start the game when toon is first.
        {
            if(battleState == BattleState.START)
            {
                BattleStart(); // Check if battle can start.
            }
        }
    }

    [Server]
    void BattleStart()
    {
        if(cogs.Count > 0 && toons.Count > 0)
        {
            print("Battle is starting.");

            colliderEnabled = true;

            battleCalculator.PlayerChoose();
        }
        else
        {
            print("Not enough Cogs or Toons to start battle.");
        }
    }

    void EnableCollider(bool _old, bool _new)
    {
        cellCollider.isTrigger = !_new;
    }
}
