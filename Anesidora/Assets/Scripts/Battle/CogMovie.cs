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
    public List<GameObject> particleSystemPrefabs = new List<GameObject>();
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

        cog.GetComponent<CogAnimate>().StartCoroutine("BattleAnimate", "PoundKey");

        yield return new WaitForSeconds(2.25f);

        var tempParticles = Instantiate(particles, Vector3.zero, Quaternion.identity, cog.GetComponent<CogAnimate>().particleSpawnSpot.transform);

        tempParticles.transform.localPosition = Vector3.zero;
        tempParticles.transform.localRotation = Quaternion.Euler(0,0,0);

        yield return new WaitForSeconds(2f);

        Destroy(tempParticles);
    }
}
