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
        gagDatas.Clear(); // Clear the last turn's gags

        battleCell.battleState = BattleState.PLAYER_CHOOSE;

        foreach(GameObject g in battleCell.toons)
        {
            TargetPlayerChoose(g.GetComponent<NetworkIdentity>().connectionToClient);
        }

        print("Player(s) choosing.");
    }

    [Command (requiresAuthority = false)]
    public void CmdSelectGag(GagData gagData)
    {
        if(battleCell.battleState != BattleState.PLAYER_CHOOSE)
        {
            print("Battle state is not set to PlayerChoose, cannot pick gags at this time.");
            return;
        }

        var newSender = gagData.sender;

        foreach(GagData g in gagDatas)
        {
            if(g.sender == newSender) 
            {
                print("Sender has already selected a gag.");
                return;
            }
        }

        gagDatas.Add(gagData);

        if(gagDatas.Count >= battleCell.toons.Count)
        {
            PlayerAttack(GagTrack.THROW);
        }

    }

    [Server]
    void PlayerAttack(GagTrack gagTrack)
    {
        print("Player(s) attacking.");

        battleCell.battleState = BattleState.PLAYER_ATTACK;

        if(gagTrack == GagTrack.THROW)
        {
            CalcThrow();
        }

        
    }

    [Server]
    void CalcThrow()
    {
        var battleCalculationList = new List<BattleCalculation>();

        var throwList = new List<GagData>();
        
        var cogOneList = new List<GagData>();
        var cogTwoList = new List<GagData>();
        var cogThreeList = new List<GagData>();
        var cogFourList = new List<GagData>();
        
        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagTrack == GagTrack.THROW)
            {
                throwList.Add(g);
                print("Found throw gag.");
            }
        }

        foreach(GagData g in throwList)
        {
            if(g.whichCog == 0)
            {
                cogOneList.Add(g);
            }
            else if(g.whichCog == 1)
            {
                cogTwoList.Add(g);
            }
            else if(g.whichCog == 2)
            {
                cogThreeList.Add(g);
            }
            else if(g.whichCog == 3)
            {
                cogFourList.Add(g);
            }
        }

        for(int i = 0; i < 4; i++)
        {
            var cogList = new List<GagData>();

            if(i == 0)
            {
                cogList = cogOneList;
            }
            else if(i == 1)
            {
                cogList = cogTwoList;
            }
            else if(i == 2)
            {
                cogList = cogThreeList;
            }
            else if(i == 3)
            {
                cogList = cogFourList;
            }

            if(cogList.Count > 1)
            {
                cogList = OrderGagList(cogList);
            }
            
            if(cogList.Count > 0)
            {
                var battleCalc = new BattleCalculation();

                battleCalc.whichCog = 0;

                foreach(GagData g in cogList)
                {
                    battleCalc.gagDataList.Add(g);
                }

                var cogs = new List<GameObject>();

                cogs.Add(battleCell.cogs[0]);

                battleCalc.didHit = CalculateAttackHit(battleCalc.gagDataList, cogs);

                battleCalculationList.Add(battleCalc);
            }
        }

        // Calculation Time

        foreach(BattleCalculation b in battleCalculationList)
        {
            GameObject whichCog = battleCell.cogs[b.whichCog];
            var cogBattle = whichCog.GetComponent<CogBattle>();

            float teamBonus = 0;
            float lureBonus = 0;

            int dmg = 0;

            foreach(GagData g in b.gagDataList)
            {
                dmg += g.gag.power;
            }

            if(b.gagDataList.Count > 1)
            {
                teamBonus = .2f;
            }

            if(b.didHit)
            {
                if(cogBattle.isLured)
                {
                    lureBonus = .5f;
                }
                else
                {
                    lureBonus = 0;
                }
            }
            else
            {
                dmg = 0;
            }

            int totalDmgTaken = (int)(dmg + (float)(dmg * teamBonus) + (float)(dmg * lureBonus));

            cogBattle.hp -= totalDmgTaken;
            cogBattle.isLured = false;

            if(b.didHit)
            {
                print($"Cog took damage {totalDmgTaken} ---- Team Bonus: {teamBonus} ---- Lure Bonus: {lureBonus}");
            }
            else
            {
                print($"Cog dodged the attack.");
            }

            RpcReceiveThrowCalculations(battleCalculationList);
        }
    }

    [ClientRpc]
    void RpcReceiveThrowCalculations(List<BattleCalculation> battleCalculations)
    {
        foreach(BattleCalculation b in battleCalculations)
        {
            print($"Battle Calculations received on Client: Did Hit: {b.didHit}, Which Cog: {b.whichCog}");
        }
    }

    private List<GagData> OrderGagList(List<GagData> gagList)
    {
        var newList = new List<GagData>();

        for(int i = 0; i < 6; i++) // orders Gag List by Gag Level
        {
            foreach(GagData g in gagList)
            {
                if(g.gag.gagLevel == i)
                {
                    newList.Add(g);
                }
            }
        }

        return newList;
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
    private bool CalculateAttackHit(List<GagData> gagDatas, List<GameObject> targets)
    {
        int rand = Random.Range(0,100);

        int highestGagLevel = 0;
        int highestCogLevel = 0;

        bool allLured = false;

        int propAcc = 0;

        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagLevel >= highestGagLevel)
            {
                highestGagLevel = g.gag.gagLevel;

                propAcc = g.gag.acc;
            }
        }

        int cogsLured = 0;

        foreach(GameObject g in targets)
        {
            if(g.GetComponent<CogBattle>().level > highestCogLevel)
            {
                highestCogLevel = g.GetComponent<CogBattle>().level;
            }

            if(g.GetComponent<CogBattle>().isLured)
            {
                cogsLured++;
            }
        }

        if(cogsLured >= targets.Count)
        {
            allLured = true;
            print("All cogs are lured. Accuracy is 100%");
        }
        else
        {
            allLured = false;
            print("Not all cogs are lured. Accuracy is not 100%");
        }

        // atkAcc = propAcc + trackExp + tgtDef + bonus

        int bonus = 0;

        int atkAcc = 0;

        if(allLured)
        {
            atkAcc = 100;
        }
        else
        {
            atkAcc = propAcc + ((highestGagLevel) * 10) + tgtDef[highestCogLevel] + bonus;

            if(atkAcc > 95) {atkAcc = 95;}
        }

        print($"ATTACK ACCURACY: {atkAcc} --- PROPACC: {propAcc}");

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




