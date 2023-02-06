using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SoundMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    private ExplodeMovie explodeMovie;
    bool showedToonsCamera, showedCogsCamera;
    List<BattleCalculation> battleCalculations = new List<BattleCalculation>();
    public List<GameObject> soundProps = new List<GameObject>();
    public GameObject megaphoneProp;
    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
        explodeMovie = GetComponent<ExplodeMovie>();
    }

    public void StartSoundMovies(List<BattleCalculation> battleCalculations)
    {
        this.battleCalculations = battleCalculations;

        showedToonsCamera = false;
        showedCogsCamera = false;

        foreach(BattleCalculation b in battleCalculations)
        {
            StartCoroutine(StartSoundMovie(b));
        }

        print("Battle calculations received: " + battleCalculations.Count);
    }

    IEnumerator StartSoundMovie(BattleCalculation battleCalculation)
    {
        int numOfGags = battleCalculation.gagDataList.Count - 1;
        float delay = .5f; // delay between gags

        yield return new WaitForSeconds(delay);

        if(numOfGags > 0) // check if more than one gag was used 
        {
            for(int i = 0; i < numOfGags; i++)
            {
                StartCoroutine(UseSingleSoundGag(battleCalculation, i));
                print("Running Loop");
                yield return new WaitForSeconds(delay);
            }
        }

        print(numOfGags);

        yield return StartCoroutine(UseSingleSoundGag(battleCalculation, numOfGags)); // wait for the last gag to finish

        int index = 0;
        List<GameObject> deadCogs = new List<GameObject>();

        foreach(GameObject g in battleCell.cogs)
        {
            if(!g.GetComponent<CogBattle>().isDead && IsCogDead(battleCalculation, index))
            {
                deadCogs.Add(g);
            }

            index++;
        }

        if(deadCogs.Count > 0 && battleCalculation.didHit)
        {
            explodeMovie.StartCoroutine("MultipleExplodeMovie", deadCogs);
        }
        else
        {
            battleMovie.MovieFinished();
        }
    }

    bool IsCogDead(BattleCalculation battleCalculation, int whichCog)
    {
        bool isDead = false;

        GameObject cog = battleCell.cogs[whichCog];

        float teamBonus = 0;

        int dmg = 0;

        if(battleCalculation.gagDataList.Count > 1)
        {
            teamBonus = .2f;
        }

        foreach(GagData g in battleCalculation.gagDataList)
        {
            dmg += g.gag.power;
        }

        int totalDmg = (int)(dmg + (dmg * teamBonus));

        if(totalDmg >= cog.GetComponent<CogBattle>().hp)
        {
            isDead = true;
        }

        return isDead;
    }

    IEnumerator UseSingleSoundGag(BattleCalculation battleCalculation, int gagIndex)
    {
        if(battleCalculation.gagDataList[gagIndex].gag.gagLevel == 0)
        {
            yield return StartCoroutine(DoBikeHorn(battleCalculation, gagIndex));
        }
    }

    IEnumerator DoBikeHorn(BattleCalculation battleCalculation, int gagIndex)
    {
        GameObject toon = battleMovie.GetToonFromIndex(battleCalculation.gagDataList[gagIndex].whichToon, true);

        toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Shout");

        var megaphone = Instantiate(megaphoneProp, Vector3.zero, Quaternion.identity);

        megaphone.transform.SetParent(toon.GetComponent<PlayerAnimate>().pieSpot.transform);

        float scale = megaphone.transform.localScale.x;

        megaphone.LeanScale(Vector3.zero, 0);

        megaphone.LeanScale(new Vector3(scale, scale, scale), 1).setEase(LeanTweenType.easeOutCubic);

        megaphone.transform.localPosition = Vector3.zero;
        megaphone.transform.localRotation = Quaternion.Euler(0,0,0);

        StartCoroutine(SoundPropAnimation(megaphone, 0));

        ChangeCamera(1, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        yield return new WaitForSeconds(2.25f);

        ChangeCamera(2, battleCalculation.gagDataList[gagIndex].whichToon, battleCalculation.gagDataList[gagIndex].whichTarget);

        yield return new WaitForSeconds(.25f);


        foreach(GameObject g in battleCell.cogs)
        {
            if(battleCalculation.didHit && !g.GetComponent<CogBattle>().isDead)
            {
                g.GetComponent<CogAnimate>().Animate("Idle"); // so when we use multiple sounds it plays anim again
                yield return new WaitForEndOfFrame();
                g.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "Squirt");
                g.GetComponent<CogAnimate>().CallAnimateDamageText($"-{battleCalculation.gagDataList[gagIndex].gag.power}", "red");
                ChangeCogHealthButton(battleCalculation, gagIndex, g);
            }
            else
            {
                g.GetComponent<CogAnimate>().CallAnimateDamageText($"Miss", "red");
            }
            
        }

        yield return new WaitForSeconds(3.75f);

        if(battleCalculation.gagDataList.Count - 1 == gagIndex)
        {
            if(battleCalculation.gagDataList.Count > 1)
            {
                yield return new WaitForSeconds(.5f);

                foreach(GameObject g in battleCell.cogs)
                {
                    if(!g.GetComponent<CogBattle>().isDead && battleCalculation.didHit)
                    {
                         g.GetComponent<CogAnimate>().CallAnimateDamageText($"-{GetTeamDamage(battleCalculation)}", "yellow");
                    }
                }
            } // more than one gag was used.

            megaphone.LeanScale(Vector3.zero, 1).setEase(LeanTweenType.easeOutCubic);

            yield return new WaitForSeconds(.5f);

            Destroy(megaphone);
        }

        
    }

    IEnumerator SoundPropAnimation(GameObject megaphone, int soundPropIndex)
    {
        var soundProp = Instantiate(soundProps[soundPropIndex], Vector3.zero, Quaternion.identity);

        Transform soundSpot = megaphone.transform.Find("SoundSpot");

        soundProp.transform.LeanScale(Vector3.zero, 0);

        soundProp.transform.SetParent(soundSpot);

        soundProp.transform.localPosition = Vector3.zero;
        soundProp.transform.localRotation = Quaternion.Euler(0,35,0);

        yield return new WaitForSeconds(1f);

        soundProp.transform.LeanScale(Vector3.one, 1f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(1.25f);

        soundProp.transform.LeanScaleZ(1.6f, .1f);

        yield return new WaitForSeconds(.1f);

        soundProp.transform.LeanScaleZ(.97f, .1f);

        yield return new WaitForSeconds(.1f);

        soundProp.transform.LeanScaleZ(1.6f, .1f);

        yield return new WaitForSeconds(.1f);

        soundProp.transform.LeanScaleZ(.97f, .1f);

        yield return new WaitForSeconds(.1f);

        soundProp.transform.LeanScaleZ(1f, .1f);
    }

    int GetTeamDamage(BattleCalculation battleCalculation)
    {
        int dmg = 0;

        foreach(GagData g in battleCalculation.gagDataList)
        {
            dmg += g.gag.power;
        }

        return (int)(dmg * .2f);
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
            if(battleCell.cogs.Count > 1)
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomCogCamera(true, 0));
            }
            else
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomCogCamera(false, 0));
            }
            showedCogsCamera = true;
        }
    }

    void ChangeCogHealthButton(BattleCalculation battleCalculation, int gagIndex, GameObject target)
    {
        int dmg = 0;
        int totalDmg = 0;
        float teamBonus = 0;
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

        totalDmg = (int)(dmg + (dmg * teamBonus));

        target.GetComponent<CogAnimate>().ChangeHealthButton(totalDmg);
    }
}
