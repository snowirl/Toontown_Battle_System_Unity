using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThrowMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    private ExplodeMovie explodeMovie;
    bool showedToonsCamera, showedCogsCamera;
    List<BattleCalculation> battleCalculations = new List<BattleCalculation>();

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
        explodeMovie = GetComponent<ExplodeMovie>();
    }

    public void StartThrowMovies(List<BattleCalculation> battleCalculations)
    {
        this.battleCalculations = battleCalculations;

        showedToonsCamera = false;
        showedCogsCamera = false;

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

        // if(battleCalculation.gagDataList.Count > 1)
        // {
        //     int teamBonus = 0;

        //     foreach(GagData g in battleCalculation.gagDataList)
        //     {
        //         teamBonus += g.gag.power;
        //     }

        //     teamBonus = (int)(teamBonus * 1.2f);

        //     GameObject cog = battleMovie.GetCogFromIndex(battleCalculation.gagDataList[0].whichTarget); // should be okay to ask index 0 since all will hit same
        //     // more than one gag used, so show Team Bonus Damage
        //     cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{teamBonus}");
        // }


        // tried to put team bonus here, was very late 

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

        ChangeCamera(1, gagData.whichToon, gagData.whichTarget);

        GameObject toon = battleMovie.GetToonFromIndex(gagData.whichToon);

        GameObject cog = battleMovie.GetCogFromIndex(gagData.whichTarget);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Throw");

        if(didHit)
        {
            yield return new WaitForSeconds(2.5f);

            ChangeCamera(2, gagData.whichToon, gagData.whichTarget);

            yield return new WaitForSeconds(.25f);

            cog.GetComponent<CogAnimate>().Animate("Idle");  // Set animation back to Idle so cog can take multiple hits 

            yield return new WaitForEndOfFrame();

            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{gagData.gag.power}");

            yield return cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit"); // Wait until Animation Finishes
        }
        else
        {
            yield return new WaitForSeconds(2.15f);

            ChangeCamera(2, gagData.whichToon, gagData.whichTarget);

            yield return new WaitForSeconds(.25f);

            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"Miss");

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

    void ChangeCamera(int phase, int toonIndex, int cogIndex) // 1 for toons cam, 1 for cogs cam
    {
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
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(true, toonIndex));
            }
            else
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(false, toonIndex));
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
}
