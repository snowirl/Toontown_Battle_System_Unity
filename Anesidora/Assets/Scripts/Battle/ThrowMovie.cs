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
    public List<GameObject> throwProps = new List<GameObject>();

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
                StartCoroutine(UseSingleThrowGag(battleCalculation, i));
                print("Running Loop");
                yield return new WaitForSeconds(delay);
            }
        }

        print(numOfGags);

        yield return StartCoroutine(UseSingleThrowGag(battleCalculation, numOfGags)); // wait for the last gag to finish


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

    IEnumerator UseSingleThrowGag(BattleCalculation battleCalculation, int gagIndex)
    {
        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoCupcake(battleCalculation, gagIndex));
        }
    }

    IEnumerator DoCupcake(BattleCalculation battleCalculation, int gagIndex)
    {
        print("Throwing Cupcake.");

        ChangeCamera(1, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        GameObject toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[gagIndex].whichToon, true);

        GameObject cog = battleMovie.GetCogFromIndex(battleCalculation.gagDataList[gagIndex].whichTarget);
        
        toon.transform.LookAt(cog.transform);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Throw");

        GameObject pie = Instantiate(throwProps[0], Vector3.zero, Quaternion.Euler(-90,0,0));

        pie.transform.SetParent(toon.GetComponent<PlayerAnimate>().pieSpot.transform);

        pie.transform.localPosition = Vector3.zero;
        pie.transform.localRotation = Quaternion.Euler(0,0,0);
        pie.transform.localScale = Vector3.zero;

        pie.LeanScale(Vector3.one, .35f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(2.6f);

        StartCoroutine(PropThrow(pie, cog));

        if(battleCalculation.didHit)
        {
            yield return new WaitForSeconds(.25f);

            ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

            yield return new WaitForSeconds(.25f);

            cog.GetComponent<CogAnimate>().Animate("Idle");  // Set animation back to Idle so cog can take multiple hits 

            yield return new WaitForEndOfFrame();

            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{battleCalculation.gagDataList[gagIndex].gag.power}", "red");

            ChangeCogHealthButton(battleCalculation, gagIndex, cog, false);

            StopCoroutine("Splat");

            StartCoroutine(Splat(cog));

            if(battleCalculation.gagDataList.Count - 1 == gagIndex) // we are the last gag
            {
                if(battleCalculation.gagDataList.Count > 1) // more than one gag was used.
                {
                    print("More than one gag was used in this movie.");
                    cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit");

                    yield return new WaitForSeconds(.24f);

                    if(cog.GetComponent<CogBattle>().isLured)
                    {
                        cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "Knockback");
                        cog.LeanMoveX(cog.transform.position.x + 2, .5f);
                        yield return new WaitForSeconds(.5f);
                        cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{GetLureDamage(battleCalculation)}", "orange");
                        ChangeCogHealthButton(battleCalculation, gagIndex, cog, true);
                    }

                    yield return new WaitForSeconds(.75f);

                    
                    cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{GetKnockbackDamage(battleCalculation)}", "yellow");
                    yield return new WaitForSeconds(.5f);
                }
                else
                {

                    if(cog.GetComponent<CogBattle>().isLured)
                    {
                        cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit");
                        yield return new WaitForSeconds(.24f);
                        cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "Knockback");
                        cog.LeanMoveX(cog.transform.position.x + 2, .5f);
                        yield return new WaitForSeconds(.5f);
                        ChangeCogHealthButton(battleCalculation, gagIndex, cog, true);
                        cog.GetComponent<CogAnimate>().CallAnimateDamageText($"-{GetLureDamage(battleCalculation)}", "orange");
                        yield return new WaitForSeconds(1.5f);
                    }
                    else
                    {
                        yield return cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit"); // Wait until Animation Finishes
                    }

                    
                }
            }
            else
            {
                yield return cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PieHit"); // Wait until Animation Finishes
            }
        }
        else
        {
            ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"Miss", "red");

            yield return StartCoroutine(ThrowMissMovie(cog, battleCalculation.gagDataList[gagIndex].whichTarget));
        }
    }

    int GetKnockbackDamage(BattleCalculation battleCalculation)
    {
        int dmg = 0;

        foreach(GagData g in battleCalculation.gagDataList)
        {
            dmg += g.gag.power;
        }

        return (int)(dmg * .2f);
    }

    int GetLureDamage(BattleCalculation battleCalculation)
    {
        float dmg = 0;

        foreach(GagData g in battleCalculation.gagDataList)
        {
            dmg += g.gag.power;
        }

        return (int)(dmg * .5f);
    }

    void ChangeCogHealthButton(BattleCalculation battleCalculation, int gagIndex, GameObject target, bool isLured)
    {
        int dmg = 0;
        int totalDmg = 0;
        float teamBonus = 0;
        float lureBonus = 0;
        int gagsUsed = 0;

        for (int i = 0; i < gagIndex + 1; i++)
        {
            dmg += battleCalculation.gagDataList[i].gag.power;
            gagsUsed++;
        }

        if(gagsUsed > 1)
        {
            teamBonus = .2f;
        }

        if(isLured)
        {
            lureBonus = .5f;
        }

        totalDmg = (int)(dmg + (dmg * teamBonus) + (dmg * lureBonus));

        target.GetComponent<CogAnimate>().ChangeHealthButton(totalDmg);
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

    IEnumerator PropThrow(GameObject prop, GameObject target)
    {
        prop.transform.SetParent(null);
        prop.transform.localRotation = Quaternion.Euler(90,90,0);

        var targetPos = new Vector3(target.transform.position.x, target.transform.position.y + 2, target.transform.position.z);
        float distance = Vector3.Distance(prop.transform.position, targetPos);
        var step = 25 * Time.deltaTime; // calculate distance to move

        while(distance > .1f)
        {
            distance = Vector3.Distance(prop.transform.position, targetPos);
            prop.transform.position = Vector3.MoveTowards(prop.transform.position, targetPos, step);
            yield return null;
        }

        Destroy(prop);
    }

    IEnumerator Splat(GameObject target) // in the future, you can get gag index for the splat
    {
        var splatContainer = target.GetComponent<CogAnimate>().splatContainer;

        splatContainer.LeanScale(Vector3.zero, 0);

        yield return new WaitForEndOfFrame();

        splatContainer.SetActive(true);

        splatContainer.LeanScale(Vector3.one, .3f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(.75f);

        splatContainer.LeanScale(Vector3.zero, .25f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(.25f);

        splatContainer.SetActive(false);
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
}
