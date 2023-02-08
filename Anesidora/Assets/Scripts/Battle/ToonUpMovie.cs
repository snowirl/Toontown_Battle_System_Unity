using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ToonUpMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    bool showedToonsCamera, showedCogsCamera;
    List<BattleCalculation> battleCalculations = new List<BattleCalculation>();
    public List<GameObject> toonupProps = new List<GameObject>();
    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
    }

    public IEnumerator StartToonUpMovies(List<BattleCalculation> battleCalculations)
    {
        this.battleCalculations = battleCalculations;

        showedToonsCamera = false;
        showedCogsCamera = false;

        foreach(BattleCalculation b in battleCalculations)
        {
            yield return StartCoroutine(StartToonUpMovie(b));
        }
    }

    IEnumerator StartToonUpMovie(BattleCalculation battleCalculation)
    {
        int numOfGags = battleCalculation.gagDataList.Count - 1;
        float delay = .5f; // delay between gags

        yield return new WaitForSeconds(delay);

        if(numOfGags > 0) // check if more than one gag was used 
        {
            for(int i = 0; i < numOfGags; i++)
            {
                StartCoroutine(UseSingleToonUpGag(battleCalculation, i));
                print("Running Loop");
                yield return new WaitForSeconds(delay);
            }
        }

        yield return StartCoroutine(UseSingleToonUpGag(battleCalculation, numOfGags)); // wait for the last gag to finish

        battleMovie.MovieFinished();
    }

    IEnumerator UseSingleToonUpGag(BattleCalculation battleCalculation, int gagIndex)
    {
        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoFeather(battleCalculation, gagIndex));
        }
    }

    IEnumerator DoFeather(BattleCalculation battleCalculation, int gagIndex)
    {
        var toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[0].whichToon, true);

        var receivingToon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[0].whichTarget, false);

        var pos = battleCell.toonupSpot.position;

        var oldPos = toon.transform.position;

        var newPos = new Vector3(pos.x, toon.transform.position.y, pos.z);

        toon.LeanMove(newPos, 1f);

        toon.GetComponent<PlayerAnimate>().Animate("Run");

        toon.transform.LookAt(newPos);

        yield return new WaitForSeconds(1f);

        toon.GetComponent<PlayerAnimate>().Animate("Idle");

        yield return new WaitForSeconds(.25f);

        toon.transform.LookAt(receivingToon.transform);

        yield return toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Tickle");

        toon.transform.LookAt(oldPos);

        toon.LeanMove(oldPos, 1f);

        toon.GetComponent<PlayerAnimate>().Animate("Run");

        yield return new WaitForSeconds(1f);

        toon.GetComponent<PlayerAnimate>().Animate("Idle"); 

        toon.transform.LookAt(battleCell.cogPositions[0]);
    }
}
