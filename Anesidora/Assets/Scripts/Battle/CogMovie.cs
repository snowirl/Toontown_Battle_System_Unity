using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CogMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;
    private BattleCalculator battleCalculator;
    bool showedToonsCamera, showedCogsCamera;
    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
        battleCalculator = GetComponent<BattleCalculator>();
    }

    public void StartCogMovie(BattleCalculationCog battleCalculationCog)
    {

        showedToonsCamera = false;
        showedCogsCamera = false;

        StartCoroutine(StartCogMovieCO(battleCalculationCog));

        print("Showing Cog Movie.");
    }

    IEnumerator StartCogMovieCO(BattleCalculationCog battleCalculationCog)
    {
        
        if(isServer)
        {
            battleCalculator.ExecuteCogAttack(battleCalculationCog); // Execute cog attack during the movie for a realtime output // in case I add unites in 5 years
        }

        if(battleCalculationCog.cogAttack.attackName == AttackName.PoundKey)
        {
            yield return StartCoroutine(PoundKey(battleCalculationCog));
        }

        battleMovie.MovieFinished();
    }

    IEnumerator PoundKey(BattleCalculationCog battleCalculationCog)
    {

        var cog = battleMovie.GetCogFromIndex(battleCalculationCog.whichCog);
        var toon = battleCell.toons[battleCalculationCog.whichTarget];

        cog.transform.LookAt(toon.transform);
        toon.transform.LookAt(cog.transform);

        var particles = Resources.Load<GameObject>("Particles/PoundKey");

        ChangeCamera(2, battleCalculationCog.whichTarget, battleCalculationCog.whichCog, battleCalculationCog);

        cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PoundKey");

        yield return new WaitForSeconds(2.25f);

        var tempParticles = Instantiate(particles, Vector3.zero, Quaternion.identity, cog.GetComponent<CogAnimate>().particleSpawnSpot.transform);

        tempParticles.transform.localPosition = Vector3.zero;
        tempParticles.transform.localRotation = Quaternion.Euler(0,0,0);

        ChangeCamera(1, battleCalculationCog.whichTarget, battleCalculationCog.whichCog, battleCalculationCog);

        if(battleCalculationCog.didHitList[0])
        {
            yield return new WaitForSeconds(.5f);
            toon.GetComponent<PlayerAnimate>().CallAnimateDamageText($"-{battleCalculationCog.dmg}", "red");
            yield return toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Cringe");
        }
        else
        {
            yield return new WaitForSeconds(.15f);
            toon.GetComponent<PlayerAnimate>().CallAnimateDamageText($"Miss", "red");
            yield return toon.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "SidestepLeft");
        }

        yield return new WaitForSeconds(.5f);

        Destroy(tempParticles);
    }

    void ChangeCamera(int phase, int toonIndex, int cogIndex, BattleCalculationCog battleCalculationCog) // 1 for toons cam, 1 for cogs cam
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

        if(phase == 1)
        {
            if(battleCalculationCog.cogAttack.areaOfEffect)
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(true, toonIndex, false));
            }
            else
            {
                battleMovie.SwitchCamera(battleMovie.GetRandomToonCamera(false, toonIndex, false));
            }
            showedToonsCamera = true;
        }
        else if(phase == 2)
        {
            int rand = Random.Range(0, 2);

            if(rand == 0)
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
