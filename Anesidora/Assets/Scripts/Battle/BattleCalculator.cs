using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BattleCalculator : NetworkBehaviour
{
    public BattleCell battleCell;
    private int[] tgtDef = {-2, -5, -10, -12, -15, -25, -30, -35, -40, -45, -50, -55};
    public List<GagData> gagDatas = new List<GagData>();
    public BattleMovie battleMovie;
    public GagTrack track; // Which gag track are we calculating for?

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

        battleCell.AddCogs();
        battleCell.AddToons();

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

        var newSender = gagData.whichToon;

        foreach(GagData g in gagDatas)
        {
            if(g.whichToon == newSender) 
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

        track = gagTrack;

        battleCell.battleState = BattleState.PLAYER_ATTACK;

        if(gagTrack == GagTrack.THROW)
        {
            CalcThrow();
        }
        else if(gagTrack == GagTrack.SQUIRT)
        {
            CalcSquirt();
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

        if(throwList.Count == 0)
        {
            print("No Throw gags found. Moving on...");
            NextTrack();
            return;
        }

        foreach(GagData g in throwList)
        {
            if(g.whichTarget == 0)
            {
                cogOneList.Add(g);
            }
            else if(g.whichTarget == 1)
            {
                cogTwoList.Add(g);
            }
            else if(g.whichTarget == 2)
            {
                cogThreeList.Add(g);
            }
            else if(g.whichTarget == 3)
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
                if(battleCell.cogs[i].GetComponent<CogBattle>().isDead)
                {
                    print("Cog is already dead, we can't attack him.");
                    break; // Cog is dead, so we do not need to calculate this Battle Calculation // Might be in the wrong spot though.
                }

                var battleCalc = new BattleCalculation();

                battleCalc.whichCog = i;

                foreach(GagData g in cogList)
                {
                    battleCalc.gagDataList.Add(g);
                }

                var cogs = new List<GameObject>();

                cogs.Add(battleCell.cogs[i]);

                battleCalc.didHit = CalculateAttackHit(battleCalc.gagDataList, cogs);

                battleCalculationList.Add(battleCalc);
            }
        } 

        // Need to check if BattleCalc list is null, and if it is, we move on to the next track.

        battleMovie.SendThrowMovies(battleCalculationList);
    }

    [Server]
    public void ExecuteCalcThrow(List<BattleCalculation> battleCalculationList)
    {
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
        }

        CheckIfCogsAreDead();
    }

    [Server]
    void CalcSquirt()
    {
        print("Calculating squirt now.");

        var battleCalculationList = new List<BattleCalculation>();

        var squirtList = new List<GagData>();
        
        var cogOneList = new List<GagData>();
        var cogTwoList = new List<GagData>();
        var cogThreeList = new List<GagData>();
        var cogFourList = new List<GagData>();
        
        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagTrack == GagTrack.SQUIRT)
            {
                squirtList.Add(g);
                print("Found SQUIRT gag.");
            }
        }

        if(squirtList.Count == 0)
        {
            print("No SQUIRT gags found. Moving on...");
            NextTrack();
            return;
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
    void CheckIfCogsAreDead()
    {
        int cogsDead = 0;

        foreach(GameObject g in battleCell.cogs)
        {
            if(g.GetComponent<CogBattle>().hp <= 0)
            {
                g.GetComponent<CogBattle>().isDead = true;
                print("Cog is dead.");
                cogsDead++;
            }
        }

        if(cogsDead >= battleCell.cogs.Count)
        {
            if(battleCell.cogsPending.Count == 0)
            {
                print("All cogs are dead. And no cogs are pending. Battle should be over.");
                Win();
            }
            else
            {
                print("All cogs are dead. Cogs are pending; Battle should continue.");
                EnemyAttack();
            }   
        }
        else
        {
            NextTrack();
        }
    }

    [Server]
    void RemoveDeadCogs() // Called after both toons and cogs attacked
    {
        var cogIdList = new List<uint>();

        foreach(GameObject g in battleCell.cogs)
        {
            if(g.GetComponent<CogBattle>().isDead)
            {
                cogIdList.Add(g.GetComponent<NetworkIdentity>().netId);
            }
        }

        if(cogIdList.Count > 0)
        {
            battleCell.RemoveCogs(cogIdList);
        }

        if(battleCell.battleState != BattleState.WIN)
        {
            PlayerChoose(); // Back to player choose after we check if cogs died, and battle continues
        }
    }

    [Server]
    void NextTrack()
    {
        if(track == GagTrack.THROW)
        {
            PlayerAttack(GagTrack.SQUIRT);
        }
        else if(track == GagTrack.SQUIRT)
        {
            EnemyAttack();
        }
    }

    [Server]
    public void EnemyAttack()
    {
        battleCell.battleState = BattleState.ENEMY_ATTACK;

        print("Enemies attacking.");

        RemoveDeadCogs(); // back to player choose after attack // need to check if cogs died
    }

    [Server]
    public void Win()
    {
        battleCell.battleState = BattleState.WIN;

        RemoveDeadCogs(); // Might need to call here too

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




