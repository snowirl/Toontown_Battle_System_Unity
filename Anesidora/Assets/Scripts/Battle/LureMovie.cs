using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LureMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    bool showedToonsCamera, showedCogsCamera;
    List<BattleCalculation> battleCalculations = new List<BattleCalculation>();
    public List<GameObject> lureProps = new List<GameObject>();

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
    }

    public void StartLureMovies(List<BattleCalculation> battleCalculations)
    {
        this.battleCalculations = battleCalculations;

        showedToonsCamera = false;
        showedCogsCamera = false;

        foreach(BattleCalculation b in battleCalculations)
        {
            StartCoroutine(StartLureMovie(b));
        }

        print("Battle calculations received: " + battleCalculations.Count);
    }

    IEnumerator StartLureMovie(BattleCalculation battleCalculation)
    {
        int numOfGags = battleCalculation.gagDataList.Count - 1;
        float delay = .5f; // delay between gags
        yield return new WaitForSeconds(delay);

        if(numOfGags > 0) // check if more than one gag was used 
        {
            for(int i = 0; i < numOfGags; i++)
            {
                StartCoroutine(UseSingleLureGag(battleCalculation, i));
                print("Running Loop");
                yield return new WaitForSeconds(delay);
            }
        }

        yield return StartCoroutine(UseSingleLureGag(battleCalculation, numOfGags)); // wait for the last gag to finish

        battleMovie.MovieFinished();
    }

    IEnumerator UseSingleLureGag(BattleCalculation battleCalculation, int gagIndex)
    {
        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoDollarBill(battleCalculation, gagIndex));
        }

        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 1)
        {
            yield return StartCoroutine(DoSmallMagnet(battleCalculation, gagIndex));
        }
    }

    IEnumerator DoDollarBill(BattleCalculation battleCalculation, int gagIndex)
    {
        ChangeCamera(1, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        GameObject toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[gagIndex].whichToon, true);

        GameObject cog = battleMovie.GetCogFromIndex(battleCalculation.gagDataList[gagIndex].whichTarget);

        toon.transform.LookAt(cog.transform);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Fish");


        if(battleCalculation.didHit) // only run if we are the last gag on this cog.
        {

            if(battleCalculation.gagDataList.Count - 1 != gagIndex)
            {
                // we are not the last gag
                yield break;
            }

            yield return new WaitForSeconds(3.25f);

            ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"Lured", "green");

            yield return StartCoroutine(cog.GetComponent<CogAnimate>().BattleAnimate("WalkNReach"));
            cog.LeanMoveX(cog.transform.position.x - 2, 0f); // we dont need to do it over time for this animation


            if(!cog.GetComponent<CogBattle>().isTrapped) // if we are not trapped then we show idle lure animation
            {
                cog.GetComponent<CogAnimate>().Animate("Lured");
            }
        }
        else
        {
            cog.GetComponent<CogAnimate>().CallAnimateDamageText($"Missed", "red");

            yield return new WaitForSeconds(.25f);
        }
    }

    IEnumerator DoSmallMagnet(BattleCalculation battleCalculation, int gagIndex)
    {
        ChangeCamera(1, battleCalculation.gagDataList[gagIndex].whichToon, 0);

        GameObject toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[gagIndex].whichToon, true);

        List<GameObject> cogList = new List<GameObject>();

        foreach(GameObject g in battleCell.cogs)
        {
            if(!g.GetComponent<CogBattle>().isLured)
            {
                cogList.Add(g);
            }
        }

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Magnet");

        var handsSpot = toon.GetComponent<PlayerAnimate>().GetLeftHand();

        var magnet = Instantiate(lureProps[1], Vector3.zero, Quaternion.identity);

        magnet.transform.SetParent(handsSpot);

        var scale = magnet.transform.localScale.x;

        magnet.LeanScale(Vector3.zero, 0f).setEase(LeanTweenType.easeOutCubic);

        magnet.LeanScale(new Vector3(scale, scale, scale), 1f).setEase(LeanTweenType.easeOutCubic);

        magnet.transform.localPosition = new Vector3(.002f, .002f, .002f);

        magnet.transform.localRotation = Quaternion.Euler(180,0,90);

        if(battleCalculation.didHit)
        {
            if(battleCalculation.gagDataList.Count - 1 != gagIndex)
            {
                // we are not the last gag
                yield break;
            }

            yield return new WaitForSeconds(2.60f);

            ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, 0);

            foreach(GameObject g in cogList)
            {
                g.GetComponent<CogAnimate>().CallAnimateDamageText($"Lured", "green");
                StartCoroutine(CogMagnetAnimation(g));
            }

            yield return new WaitForSeconds(3.25f); 
  

            foreach(GameObject g in cogList)
            {
                if(!g.GetComponent<CogBattle>().isTrapped)
                {
                    g.GetComponent<CogAnimate>().Animate("Lured");
                }
            }
        }
        else
        {
            foreach(GameObject g in cogList)
            {
                g.GetComponent<CogAnimate>().CallAnimateDamageText($"Missed", "red");
            }

            yield return new WaitForSeconds(1f);
        }

        magnet.LeanScale(Vector3.zero, 1f).setEase(LeanTweenType.easeOutCubic);
        Destroy(magnet, 1);
    }

    IEnumerator CogMagnetAnimation(GameObject cog)
    {
        cog.GetComponent<CogAnimate>().Animate("LandingBackward");

        yield return new WaitForSeconds(.2f);

        cog.GetComponent<CogAnimate>().Animate("Idle");

        yield return new WaitForEndOfFrame();

        cog.GetComponent<CogAnimate>().Animate("LandingBackward");

        yield return new WaitForSeconds(.2f);

        cog.GetComponent<CogAnimate>().Animate("Idle");

        yield return new WaitForEndOfFrame();

        cog.GetComponent<CogAnimate>().Animate("LandingBackward");

        yield return new WaitForSeconds(1f);

        cog.LeanMoveX(cog.transform.position.x - 2, .45f);

        cog.GetComponent<CogAnimate>().Animate("Landing");

        yield return StartCoroutine(cog.GetComponent<CogAnimate>().BattleAnimate("Landing"));
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
