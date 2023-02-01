using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThrowMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    private ExplodeMovie explodeMovie;

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
        explodeMovie = GetComponent<ExplodeMovie>();
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
        int numOfGags = battleCalculation.gagDataList.Count - 1;
        float delay = .5f; // delay between gags

        yield return new WaitForSeconds(delay);

        if(numOfGags > 0) // check if more than one gag was used 
        {
            for(int i = 0; i < numOfGags; i++)
            {
                StartCoroutine(UseSingleThrowGag(battleCalculation.gagDataList[i], battleCalculation.didHit));
                print("Running Loop");
                yield return new WaitForSeconds(delay);
            }
        }

        print(numOfGags);

        yield return StartCoroutine(UseSingleThrowGag(battleCalculation.gagDataList[numOfGags], battleCalculation.didHit)); // wait for the last gag to finish

        print("Throw movie completed.");

        bool isDead = false;

        if(battleCalculation.didHit)
        {
            isDead = IsCogDead(battleCalculation);
        }

        if(isDead)
        {
            print("Cog should be dead, so animate Cog exploding.");
            explodeMovie.CallExplodeMovie(battleCalculation.whichCog);
        }
        else
        {
            battleMovie.MovieFinished();
        }
    }

    bool IsCogDead(BattleCalculation battleCalculation)
    {
        bool isDead = false;

        GameObject cog = battleCell.cogs[battleCalculation.whichCog];

        float teamBonus = 0;
        float lureBonus = 0;

        int dmg = 0;

        if(battleCalculation.gagDataList.Count > 1)
        {
            teamBonus = .2f;
        }

        if(cog.GetComponent<CogBattle>().isLured)
        {
            lureBonus = .5f;
        }

        foreach(GagData g in battleCalculation.gagDataList)
        {
            dmg += g.gag.power;
        }

        int totalDmg = (int)(dmg + (dmg * teamBonus) + (dmg * lureBonus));

        if(totalDmg >= cog.GetComponent<CogBattle>().hp)
        {
            isDead = true;
        }

        return isDead;
    }

    IEnumerator UseSingleThrowGag(GagData g, bool didHit)
    {
        if(g.gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoCupcake(g, didHit));
        }
    }

    IEnumerator DoCupcake(GagData gagData, bool didHit)
    {
        print("Throwing Cupcake.");

        GameObject toon = battleMovie.GetToonFromIndex(gagData.whichToon);

        GameObject cog = battleMovie.GetCogFromIndex(gagData.whichTarget);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Throw");

        if(didHit)
        {
            yield return new WaitForSeconds(3.25f);

            cog.GetComponent<CogAnimate>().Animate("Idle");  // Set animation back to Idle so cog can take multiple hits 

            yield return new WaitForEndOfFrame();

            yield return cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit"); // Wait until Animation Finishes
        }
        else
        {
            yield return new WaitForSeconds(2.25f);

            yield return StartCoroutine(ThrowMissMovie(cog, gagData.whichTarget));
        }
    }

    IEnumerator ThrowMissMovie(GameObject cog, int cogIndex)
    {
        string dodgeAnim = "SidestepRight";

        int numOfCogs = battleCell.cogs.Count;

        List<GameObject> additionalCogsToAnimate = new List<GameObject>();

        if(numOfCogs == 1)
        {
            // do nothing
        }
        if(numOfCogs == 2)
        {
            if(cogIndex == 0)
            {
                dodgeAnim = "SidestepRight";
            }
            else if(cogIndex == 1)
            {
                dodgeAnim = "SidestepLeft";
            }
        }
        if(numOfCogs == 3)
        {
            if(cogIndex == 0)
            {
                dodgeAnim = "SidestepRight";
            }
            else if(cogIndex == 1)
            {
                dodgeAnim = "SidestepLeft";
                additionalCogsToAnimate.Add(battleMovie.GetCogFromIndex(2));
            }
            else if(cogIndex == 2)
            {
                dodgeAnim = "SidestepLeft";
            }
        }
        if(numOfCogs == 4)
        {
            if(cogIndex == 0)
            {
                dodgeAnim = "SidestepRight";
            }
            else if(cogIndex == 1)
            {
                dodgeAnim = "SidestepRight";
                additionalCogsToAnimate.Add(battleMovie.GetCogFromIndex(0));
            }
            else if(cogIndex == 2)
            {
                dodgeAnim = "SidestepLeft";
                additionalCogsToAnimate.Add(battleMovie.GetCogFromIndex(3));
            }
            else if(cogIndex == 3)
            {
                dodgeAnim = "SidestepLeft";
            }
        }

        foreach(GameObject g in additionalCogsToAnimate)
        {
            g.GetComponent<CogAnimate>().StopCoroutine("BattleAnimate"); // Stop CO if we are running just in case 
            g.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", dodgeAnim); // Wait until Animation Finishes
        }
        
        cog.GetComponent<CogAnimate>().StopCoroutine("BattleAnimate"); // Stop CO if we are running just in case 
        yield return cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", dodgeAnim); // Wait until Animation Finishes

        yield return new WaitForSeconds(2f);
    }
}
