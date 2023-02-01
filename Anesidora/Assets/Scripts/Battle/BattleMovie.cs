using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleMovie : NetworkBehaviour
{
    private BattleCell battleCell;
    public int moviesRemaining; // does NOT need to be synced, will be told by the Server when needed.
    public int clientsDone;
    private ThrowMovie throwMovie;

    void Start()
    {
        battleCell = GetComponent<BattleCell>();
        throwMovie = GetComponent<ThrowMovie>();
    }

    public void MovieFinished()
    {
        moviesRemaining -= 1;

        if(moviesRemaining <= 0)
        {
            if(isClientOnly)
            {
                if(battleCell.toons.Contains(NetworkClient.localPlayer.gameObject)) // if the client that completed the movie is in the battle
                {
                    CmdClientDone();
                }   
            }
            else if(isServer)
            {
                StartCoroutine(ServerWaitingForOtherPlayers());
            }
        }
    }

    private IEnumerator ServerWaitingForOtherPlayers()
    {
        float waitingTime = 2f; // gives 2 seconds for other players to catch up.

        yield return new WaitForSeconds(.25f);

        if(clientsDone >= battleCell.toons.Count)
        {
            waitingTime = 0;
            print("All clients are done with movie.");
        }
        else
        {
            print("Not all clients are done with movie.");
        }
        yield return new WaitForSeconds(waitingTime);

        print("Now we are ready to move on.");
    }

    [Command (requiresAuthority = false)]
    void CmdClientDone()
    {
        clientsDone++;
        print("Client is done with movie");

        if(clientsDone >= battleCell.toons.Count)
        {
            StopCoroutine("ServerWaitingForOtherPlayers");
            print("Command cancelled CO because are are done.");
        }
    }

    [Server]
    public void SendThrowMovies(List<BattleCalculation> battleCalculations)
    {
        clientsDone = 0;
        moviesRemaining = battleCalculations.Count; // sets movies remaining on server 

        throwMovie.StartThrowMovies(battleCalculations); // Starts CO on server

        RpcThrowMovies(battleCalculations); // Sends CO to clients
    }

    [ClientRpc]
    void RpcThrowMovies(List<BattleCalculation> battleCalculations)
    {
        moviesRemaining = battleCalculations.Count; // sets movies remaining on server 

        throwMovie.StartThrowMovies(battleCalculations); // Starts CO on clients

    }
}
