using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class BattleMovie : NetworkBehaviour
{
    private BattleCell battleCell;
    public int moviesRemaining; // does NOT need to be synced, will be told by the Server when needed.
    public int clientsDone;
    private ThrowMovie throwMovie;
    private BattleCalculator battleCalculator;
    List<BattleCalculation> battleCalcList = new List<BattleCalculation>();
    List<BattleCalculationCog> battleCalcCogList = new List<BattleCalculationCog>();
    public GameObject battleCamera;
    public List<GameObject> toonCameras = new List<GameObject>();
    public List<GameObject> toonGroupCameras = new List<GameObject>();
    public List<GameObject> cogCameras = new List<GameObject>();
    public List<GameObject> cogGroupCameras = new List<GameObject>();

    void Start()
    {
        battleCalculator = GetComponent<BattleCalculator>();
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
        float waitingTime = .5f; // gives 2 seconds for other players to catch up.

        // PLEASE UPDATE BACK TO 2 SECONDS AFTER TESTING!

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
        ExecuteCalculations();
    }

    [Command (requiresAuthority = false)]
    void CmdClientDone()
    {
        clientsDone++;
        print("Client is done with movie");

        if(clientsDone >= battleCell.toons.Count)
        {
            StopCoroutine("ServerWaitingForOtherPlayers");
            print("Command cancelled CO because Clients are done.");
            ExecuteCalculations();
        }
    }

    [Server]
    void ExecuteCalculations()
    {
        if(battleCalculator.track == GagTrack.THROW)
        {
            battleCalculator.ExecuteCalcThrow(battleCalcList);
        }
    }

    [Server]
    public void SendThrowMovies(List<BattleCalculation> battleCalculations)
    {
        battleCalcList = battleCalculations; // Give the server the battle calculations it needs to send back

        clientsDone = 0;
        moviesRemaining = battleCalculations.Count; // sets movies remaining on server 

        throwMovie.StartThrowMovies(battleCalculations); // Starts CO on server

        RpcThrowMovies(battleCalculations); // Sends CO to clients
    }

    [ClientRpc]
    void RpcThrowMovies(List<BattleCalculation> battleCalculations)
    {
        if(!isClientOnly) {return;} // Don't run on Host 

        moviesRemaining = battleCalculations.Count; // sets movies remaining on server 

        throwMovie.StartThrowMovies(battleCalculations); // Starts CO on clients

    }

    public GameObject GetToonFromIndex(int index)
    {
        int toonIndex = battleCell.toonIDs.IndexOf((uint)index);

        return battleCell.toons[toonIndex];
    }

    public GameObject GetCogFromIndex(int index)
    {
        return battleCell.cogs[index];
    }

    public GameObject GetRandomToonCamera(bool isMultiple, int toonIndex)
    {
        if(isMultiple)
        {
            return toonGroupCameras[Random.Range(0, toonGroupCameras.Count)];
        }
        else
        {
            int rand = Random.Range(0,2);

            if(rand == 0)
            {
                return toonGroupCameras[Random.Range(0, toonGroupCameras.Count)];
            }
            else
            {
                GameObject newCam = toonCameras[Random.Range(0, toonCameras.Count)];

                newCam.GetComponent<CinemachineVirtualCamera>().LookAt = GetToonFromIndex(toonIndex).transform;
                newCam.GetComponent<CinemachineVirtualCamera>().Follow = GetToonFromIndex(toonIndex).transform;

                return newCam;
            }
            
            
        }
    }

    public GameObject GetRandomCogCamera(bool isMultiple, int cogIndex)
    {
        if(isMultiple)
        {
            return cogGroupCameras[Random.Range(0, cogGroupCameras.Count)];
        }
        else
        {
            int rand = Random.Range(0,2);

            if(rand == 0)
            {
                return cogGroupCameras[Random.Range(0, cogGroupCameras.Count)];
            }
            else
            {
                GameObject newCam = cogCameras[Random.Range(0, cogCameras.Count)];

                newCam.GetComponent<CinemachineVirtualCamera>().LookAt = GetCogFromIndex(cogIndex).transform;
                newCam.GetComponent<CinemachineVirtualCamera>().Follow = GetCogFromIndex(cogIndex).transform;

                return newCam;
            }
            
        }
    }

    public void SwitchCamera(GameObject newCam)
    {
        foreach(GameObject g in toonCameras)
        {
            g.SetActive(false);
        }

        foreach(GameObject g in toonGroupCameras)
        {
            g.SetActive(false);
        }

        foreach(GameObject g in cogCameras)
        {
            g.SetActive(false);
        }

        foreach(GameObject g in cogGroupCameras)
        {
            g.SetActive(false);
        }

        newCam.SetActive(true);
    }
}
