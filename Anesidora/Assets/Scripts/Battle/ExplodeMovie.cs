using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ExplodeMovie : NetworkBehaviour
{
    private BattleMovie battleMovie;

    void Start()
    {
        battleMovie = GetComponent<BattleMovie>();
    }

    public void StartExplodeMovies(List<int> whichCogs)
    {
        // Explode should be called during the track Movie, not after the battle checks if they are dead.
    }
}
