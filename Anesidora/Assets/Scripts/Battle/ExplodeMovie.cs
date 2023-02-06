using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplodeMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;
    private BattleCell battleCell;

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
        battleCell = GetComponent<BattleCell>();
    }

    public void CallExplodeMovie(int whichCog)
    {
        StartCoroutine(StartExplodeMovie(whichCog));
    }

    public IEnumerator MultipleExplodeMovie(List<GameObject> cogs)
    {
        foreach(GameObject g in battleCell.toons)
        {
            g.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Duck"); // This is going to make some Toons run Idle early on multiple explosions...
        }

        foreach(GameObject g in cogs)
        {
            g.GetComponent<CogAnimate>().StartCoroutine("ExplodeCog");
        }

        yield return new WaitForSeconds(6.5f);

        battleMovie.MovieFinished();
    }

    IEnumerator StartExplodeMovie(int whichCog)
    {
        print("Cog is exploding...");

        foreach(GameObject g in battleCell.toons)
        {
            g.GetComponent<PlayerAnimate>().StartCoroutine("BattleAnimate", "Duck"); // This is going to make some Toons run Idle early on multiple explosions...
        }

        GameObject cog = battleMovie.GetCogFromIndex(whichCog);

        yield return cog.GetComponent<CogAnimate>().StartCoroutine("ExplodeCog");

        battleMovie.MovieFinished();
    }
}
