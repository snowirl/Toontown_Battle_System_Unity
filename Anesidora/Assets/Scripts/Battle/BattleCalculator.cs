using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleCalculator : NetworkBehaviour
{
    public BattleCell battleCell;
    private int[] tgtDef = {-2, -5, -10, -12, -15, -25, -30, -35, -40, -45, -50, -55};
    public List<GagData> gagDatas = new List<GagData>();

    [Server]
    public void StartBattle()
    {
        battleCell.battleState = BattleState.START;

        print("Battle started.");
    }

    [Server]
    public void PlayerChoose()
    {
        battleCell.battleState = BattleState.PLAYER_CHOOSE;

        foreach(GameObject g in battleCell.toons)
        {
            TargetPlayerChoose(g.GetComponent<NetworkIdentity>().connectionToClient);
        }

        print("Player(s) choosing.");
    }

    [Server]
    public void PlayerAttack()
    {
        battleCell.battleState = BattleState.PLAYER_ATTACK;

        print("Player(s) attacking.");
    }

    [Server]
    public void EnemyAttack()
    {
        battleCell.battleState = BattleState.ENEMY_ATTACK;

        print("Enemies attacking.");
    }

    [Server]
    public void Win()
    {
        battleCell.battleState = BattleState.WIN;

        print("Toons win.");
    }

    [Server]
    public void Lose()
    {
        battleCell.battleState = BattleState.LOSE;

        print("Cogs win.");
    }

    [Server]
    public bool CalculateAttackHit(Gag gag)
    {
        int rand = Random.Range(0,100);

        // atkAcc = propAcc + trackExp + tgtDef + bonus

        int atkAcc = gag.acc + ((gag.gagLevel - 1) * 10) + tgtDef[0];

        if(atkAcc > rand)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [TargetRpc]
    public void TargetPlayerChoose(NetworkConnection conn)
    {
        BattleUIManager.Instance.ShowBattleUI();
        BattleUIManager.Instance.UpdateHP_UI(battleCell);
    }
}

public enum BattleState {IDLE, START, PLAYER_CHOOSE, PLAYER_ATTACK, ENEMY_ATTACK, WIN, LOSE}




