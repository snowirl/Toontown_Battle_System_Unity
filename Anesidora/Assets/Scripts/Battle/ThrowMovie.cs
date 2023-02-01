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

        print("Throw movie completed.");

        battleMovie.MovieFinished();
    }
}
