using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThrowMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
    }

    public void StartThrowMovies(List<BattleCalculation> battleCalculations)
    {
        foreach(BattleCalculation b in battleCalculations)
        {
            StartCoroutine(StartThrowMovie(b));
        }

        print("Battle calculations received: " + battleCalculations.Count);
    }

    IEnumerator StartThrowMovie(BattleCalculation battleCalculation)
    {
        yield return new WaitForSeconds(2f);

        int numOfGags = battleCalculation.gagDataList.Count - 1;
        float delay = .2f; // delay between gags

        for(int i = 0; i < numOfGags - 1; i++)
        {
            if(i > 0)
            {
                yield return new WaitForSeconds(delay * numOfGags);
            }

            StartCoroutine(UseSingleThrowGag(battleCalculation.gagDataList[i]));
        }

        yield return StartCoroutine(UseSingleThrowGag(battleCalculation.gagDataList[numOfGags])); // wait for the last gag to finish

        print("Finished waiting on CO.");

        yield return new WaitForSeconds(delay * numOfGags);
        

        print("Throw movie completed.");

        battleMovie.MovieFinished();
    }

    IEnumerator UseSingleThrowGag(GagData g)
    {
        if(g.gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoCupcake(new GagData()));
        }

        print("Finished running Single Throw Gag.");
        
    }

    IEnumerator DoCupcake(GagData gagData)
    {
        print("Throwing Cupcake.");
        yield return new WaitForSeconds(1f);
    }
}
