using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TrapMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    private ExplodeMovie explodeMovie;
    bool showedToonsCamera, showedCogsCamera;
    List<BattleCalculation> battleCalculations = new List<BattleCalculation>();
    public List<GameObject> trapProps = new List<GameObject>();
    public GameObject[] trapsSpawned = new GameObject[4];
    public GagData[] trapsSpawnedInfo = new GagData[4];
    List<GameObject> trapsOneCog = new List<GameObject>(); // just lists to find all gameobjects that were added if more than one trap was used on a single cog
    List<GameObject> trapsTwoCog = new List<GameObject>();
    List<GameObject> trapsThreeCog = new List<GameObject>();
    List<GameObject> trapsFourCog = new List<GameObject>();

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
        explodeMovie = GetComponent<ExplodeMovie>();
    }

    public void StartTrapMovies(List<BattleCalculation> battleCalculations)
    {
        this.battleCalculations = battleCalculations;

        showedToonsCamera = false;
        showedCogsCamera = false;

        foreach(BattleCalculation b in battleCalculations)
        {
            StartCoroutine(StartTrapMovie(b));
        }
    }

    public void StartTrapMoviesEnd(List<GagData> trappedList)
    {
        showedToonsCamera = false;
        showedCogsCamera = false;

        foreach(GagData g in trappedList)
        {
            StartCoroutine(StartTrapMovieEnd(g));
        }
    }

    IEnumerator StartTrapMovie(BattleCalculation battleCalculation)
    {
        int numOfGags = battleCalculation.gagDataList.Count - 1;

        yield return new WaitForSeconds(.5f);

        if(numOfGags > 0) // check if more than one gag was used 
        {
            for(int i = 0; i < numOfGags; i++)
            {
                StartCoroutine(UseSingleTrapGag(battleCalculation, i));
            }
        }

        yield return StartCoroutine(UseSingleTrapGag(battleCalculation, numOfGags)); // wait for the last gag to finish

        print("TRAP movie completed.");

        if(numOfGags > 1)
        {
            print("More than one TRAP gag used. DESTROYING THEM.");
        }

        battleMovie.MovieFinished();
    }

    IEnumerator StartTrapMovieEnd(GagData g)
    {
        yield return StartCoroutine(UseSingleTrapGagEnd(g)); // wait for the last gag to finish

        GameObject cog = battleMovie.GetCogFromIndex(g.whichTarget);

        bool isDead = cog.GetComponent<CogBattle>().hp - g.gag.power <= 0 ? true : false;

        if(isDead)
        {
            print("Cog should be dead, so animate Cog exploding.");
            explodeMovie.CallExplodeMovie(g.whichTarget);
        }
        else
        {
            battleMovie.MovieFinished();
        }
    }

    IEnumerator UseSingleTrapGag(BattleCalculation battleCalculation, int gagIndex)
    {
        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoBananaPeelStart(battleCalculation, gagIndex));
        }
    }

    IEnumerator UseSingleTrapGagEnd(GagData g)
    {
        if(g.gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoBananaPeelEnd(g));
        }
    }

    IEnumerator DoBananaPeelStart(BattleCalculation battleCalculation, int gagIndex)
    {
        ChangeCamera(1, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        GameObject toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[gagIndex].whichToon, true);

        GameObject cog = battleMovie.GetCogFromIndex(battleCalculation.gagDataList[gagIndex].whichTarget);

        toon.transform.LookAt(cog.transform);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Toss");

        var banana = Instantiate(trapProps[0], Vector3.zero, Quaternion.identity);

        banana.transform.SetParent(toon.GetComponent<PlayerAnimate>().pieSpot.transform);

        banana.transform.localPosition = Vector3.zero;

        banana.transform.localRotation = Quaternion.Euler(0,0,0);

        if(battleCalculation.gagDataList[gagIndex].whichTarget == 0)
        {
            trapsOneCog.Add(banana);
        }
        else if(battleCalculation.gagDataList[gagIndex].whichTarget == 1)
        {
            trapsTwoCog.Add(banana);
        }
        else if(battleCalculation.gagDataList[gagIndex].whichTarget == 2)
        {
            trapsThreeCog.Add(banana);
        }
        else if(battleCalculation.gagDataList[gagIndex].whichTarget == 3)
        {
            trapsFourCog.Add(banana);
        }

        trapsSpawned[battleCalculation.gagDataList[gagIndex].whichTarget] = banana;
        trapsSpawnedInfo[battleCalculation.gagDataList[gagIndex].whichTarget] = battleCalculation.gagDataList[gagIndex];

        Vector3 pos = battleCell.cogs[battleCalculation.gagDataList[gagIndex].whichTarget].transform.position;

        yield return new WaitForSeconds(2.6f);

        banana.transform.SetParent(null);

        banana.LeanMove(new Vector3(pos.x - 2, 0, pos.z), 1f).setEase(LeanTweenType.easeOutCubic);

        ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        yield return new WaitForSeconds(1.5f);

        banana.transform.position = new Vector3(pos.x - 2, 0, pos.z);

        if(battleCalculation.gagDataList.Count > 1 && battleCalculation.gagDataList.Count -1 == gagIndex)
        {
            // we are the last gag and there is more than one trap gag
            StartCoroutine(DestroyTrapGags(battleCalculation.gagDataList[gagIndex].whichTarget));
        }
        else
        {
            
        }
    }

    [Server]
    public void CallMoveTraps()
    {
        MoveTraps();

        RpcMoveTraps();
    }

    [ClientRpc]
    void RpcMoveTraps()
    {
        if(!isClient) {return;}

        MoveTraps();
    }

    
    void MoveTraps()
    {
        for(int i = 0; i < trapsSpawnedInfo.Length; i++)
        {

            if(trapsSpawnedInfo[i] == null)
            {
                
            }
            else
            {
                if(trapsSpawned[i] == null)
                {

                }
                else
                {
                    Vector3 pos = battleCell.cogs[trapsSpawnedInfo[i].whichTarget].transform.position;
                    trapsSpawned[i].transform.position = new Vector3(pos.x - 2, 0, pos.z);
                }
                
            }
        }
    }

    IEnumerator DestroyTrapGags(int whichCog)
    {
        if(whichCog == 0)
        {
            foreach(GameObject g in trapsOneCog)
            {
                Destroy(g);
            }
        }
        else if(whichCog == 1)
        {
            foreach(GameObject g in trapsOneCog)
            {
                Destroy(g);
            }
        }
        else if(whichCog == 2)
        {
            foreach(GameObject g in trapsOneCog)
            {
                Destroy(g);
            }
        }
        else if(whichCog == 3)
        {
            foreach(GameObject g in trapsOneCog)
            {
                Destroy(g);
            }
        }

        trapsSpawned[whichCog] = null;
        trapsSpawnedInfo[whichCog] = null;

        print("Destroying multiple traps on one cog.");

        yield return new WaitForSeconds(2f);
    }

    IEnumerator DoBananaPeelEnd(GagData g)
    {
        print("Showing end of the banana peel movie.");
        battleCell.cogs[g.whichTarget].GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "SlipBackwards");
        yield return new WaitForSeconds(.25f);
        battleCell.cogs[g.whichTarget].GetComponent<CogAnimate>().ChangeHealthButton(g.gag.power);
        battleCell.cogs[g.whichTarget].GetComponent<CogAnimate>().CallAnimateDamageText($"-{g.gag.power}", "red");
        var pos = battleCell.cogPositions[g.whichTarget].position;
        var randomPos = new Vector3(pos.x - Random.Range(4,7), pos.y -.2f, pos.z + Random.Range(-3, 3));
        trapsSpawned[g.whichTarget].LeanMove(randomPos, .75f).setEase(LeanTweenType.easeOutCubic);
        trapsSpawned[g.whichTarget].LeanRotateY(0, 0);
        trapsSpawned[g.whichTarget].LeanRotateY(180, .4f);
        yield return new WaitForSeconds(.4f);
        trapsSpawned[g.whichTarget].LeanRotateY(0, .4f);
        yield return new WaitForSeconds(.4f);
        trapsSpawned[g.whichTarget].LeanScale(Vector3.zero, 1).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(4f);
        Destroy(trapsSpawned[g.whichTarget]);
        trapsSpawned[g.whichTarget] = null;
        trapsSpawnedInfo[g.whichTarget] = null;
    }

    void ChangeCamera(int phase, int toonIndex, int cogIndex) // 1 for toons cam, 1 for cogs cam
    {
        if(!battleCell.toons.Contains(NetworkClient.localPlayer.gameObject))
        {
            return; // client is not in battle so we do not change cameras
        }

        if(phase == 1)
        {
            if(showedToonsCamera)
            {
                return;
            }
        }
        else if(phase == 2)
        {
            if(showedCogsCamera)
            {
                return;
            }
        }

        int gagsUsed = 0;

        foreach(BattleCalculation b in battleCalculations)
        {
            gagsUsed += b.gagDataList.Count;
        }

        if(phase == 1)
        {
            if(gagsUsed > 1)
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(true, toonIndex, true));
            }
            else
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(false, toonIndex, true));
            }
            showedToonsCamera = true;
        }
        else if(phase == 2)
        {
            if(battleCalculations.Count > 1)
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomCogCamera(true, cogIndex));
            }
            else
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomCogCamera(false, cogIndex));
            }
            showedCogsCamera = true;
        }
        
        
    }

    bool IsCogDead(BattleCalculation battleCalculation)
    {
        if(battleMovie.GetCogFromIndex(battleCalculation.whichCog).GetComponent<CogBattle>().hp - battleCalculation.gagDataList[0].gag.power <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
