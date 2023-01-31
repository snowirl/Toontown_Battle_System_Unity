using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerStreet : NetworkBehaviour
{
    public PlayerMove playerMove;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(!isLocalPlayer) {return;}

        if(hit.gameObject.tag == "Cog")
        {

            if(!hit.gameObject.GetComponent<CogMove>().isBusy)
            {
                CmdRequestBattleCell(this.gameObject, hit.gameObject);
                playerMove.DisableMovement();
            }
            else
            {
                print("Cog is busy right now. Cannot create a battle.");
            }
        }

        if(hit.gameObject.tag == "BattleCell")
        {
            print("Hit battle cell.");
            playerMove.DisableMovement(); // If the battle is full we are still disabling movement, so player will be stuck
            hit.gameObject.GetComponentInParent<BattleCell>().CmdAddToonPending(this.gameObject);
        }
    }

    [Command]
    void CmdRequestBattleCell(GameObject player, GameObject cog)
    {
        print("Finding closest battle cell.");

        GameObject closestCell = null;

        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = player.transform.position;
        var cells = GameObject.FindGameObjectsWithTag("BattleCell");

        foreach(GameObject c in cells)
        {
            Vector3 directionToTarget = c.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestCell = c;
            }
        }

        TargetReceiveBattleCell(closestCell, cog);
    }

    [TargetRpc]
    void TargetReceiveBattleCell(GameObject cell, GameObject cog)
    {
        if(cell == null)
        { 
            print("Could not find a cell.");
            playerMove.EnableMovement();
            return;
        }
        else
        {
            print("Received cell.");
        }

        var battleCell = cell.GetComponent<BattleCell>();

        battleCell.CmdAddToonPending(this.gameObject);
        battleCell.CmdAddCogPending(cog);
    }
}
