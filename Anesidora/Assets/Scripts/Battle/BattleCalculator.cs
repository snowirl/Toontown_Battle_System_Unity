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
    public List<GameObject> cogsAttackingList = new List<GameObject>(); // see how many cogs are unlured or not dead to attack
    public List<GagData>trapsInPlay = new List<GagData>();

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
            PlayerAttack(GagTrack.TRAP);
        }

    }

    [Server]
    void PlayerAttack(GagTrack gagTrack)
    {
        print("Player(s) attacking.");

        track = gagTrack;

        battleCell.battleState = BattleState.PLAYER_ATTACK;

        if(gagTrack == GagTrack.TRAP)
        {
            CalcTrapStart();
        }
        else if(gagTrack == GagTrack.LURE)
        {
            CalcLure();
        }
        else if(gagTrack == GagTrack.SOUND)
        {
            CalcSound();
        }
        else if(gagTrack == GagTrack.THROW)
        {
            CalcThrow();
        }
        else if(gagTrack == GagTrack.SQUIRT)
        {
            CalcSquirt();
        }
    }

    [Server]
    void CalcTrapStart()
    {
        var battleCalculationList = new List<BattleCalculation>();

        var trapList = new List<GagData>();

        var cogOneList = new List<GagData>();
        var cogTwoList = new List<GagData>();
        var cogThreeList = new List<GagData>();
        var cogFourList = new List<GagData>();

        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagTrack == GagTrack.TRAP)
            {
                trapList.Add(g);
                print("Found TRAP gag.");
            }
        }
        
        if(trapList.Count == 0)
        {
            print("No TRAP gags found. Moving on...");
            NextTrack();
            return;
        }

        foreach(GagData g in trapList)
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
                // More than one trap was used on a Cog! 
                print("More than one trap was used on a single Cog!");
            }
            
            if(cogList.Count > 0)
            {
                if(battleCell.cogs[i].GetComponent<CogBattle>().isDead)
                {
                    print("Cog is already dead, we can't attack him.");
                    break; // Cog is dead, so we do not need to calculate this Battle Calculation // Might be in the wrong spot though.
                }
                else if(battleCell.cogs[i].GetComponent<CogBattle>().isTrapped)
                {
                    print("Cog is already Trapped, we can't attack him.");
                    //break; // Cog is already Trapped, so we do not need to calculate this Battle Calculation // Might be in the wrong spot though.
                }
                else if(battleCell.cogs[i].GetComponent<CogBattle>().isLured)
                {
                    print("Cog is already LURED, we can't TRAP him.");
                    //break; // Cog is already Trapped, so we do not need to calculate this Battle Calculation // Might be in the wrong spot though.
                }

                var battleCalc = new BattleCalculation();

                battleCalc.whichCog = i;

                foreach(GagData g in cogList)
                {
                    battleCalc.gagDataList.Add(g);
                }

                battleCalc.didHit = true;

                battleCalculationList.Add(battleCalc);
            }
        }

        battleMovie.SendTrapMovies(battleCalculationList);
    }

    [Server]
    public void ExecuteCalcTrapStart(List<BattleCalculation> battleCalculationList)
    {
        print("Executing trap logic.");

        foreach(BattleCalculation b in battleCalculationList)
        {
            if(b.gagDataList.Count > 1)
            {
                // more than one trap used.
                print("More than one trap used. Not calculating this...");
            }
            else
            {
                trapsInPlay.Add(b.gagDataList[0]);
                battleCell.cogs[b.whichCog].GetComponent<CogBattle>().isTrapped = true;
            }
        }

        CheckIfCogsAreDead();
    }

    [Server]
    void CalcTrapEnd()
    {
        battleMovie.lureCompleted = true;

        print("did I make it here?");

        if(trapsInPlay.Count > 0)
        {
            battleMovie.SendTrapMoviesEnd(trapsInPlay);
        }
        else
        {
            CheckIfCogsAreDead();
        }

        
    }

    [Server]
    public void ExecuteCalcTrapEnd()
    {

        foreach(GagData g in trapsInPlay)
        {
            GameObject whichCog = battleCell.cogs[g.whichTarget];
            var cogBattle = whichCog.GetComponent<CogBattle>();
            int dmg = g.gag.power;

            cogBattle.hp -= dmg;
            cogBattle.isLured = false;
            cogBattle.isTrapped = false;

            print($"Cog took TRAP damage {dmg}");
        }

        trapsInPlay.Clear();

        CheckIfCogsAreDead();
    }

    [Server]
    void CalcLure()
    {
        battleMovie.lureCompleted = false;

        var battleCalculationList = new List<BattleCalculation>();

        var lureList = new List<GagData>();

        var cogOneList = new List<GagData>();
        var cogTwoList = new List<GagData>();
        var cogThreeList = new List<GagData>();
        var cogFourList = new List<GagData>();
        var cogAllList = new List<GagData>();

        bool allLuredUsed = false;

        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagTrack == GagTrack.LURE)
            {
                lureList.Add(g);
                print("Found lure gag.");
            }
        }

        if(lureList.Count == 0)
        {
            print("No LURE gags found. Moving on...");
            NextTrack();
            return;
        }

        foreach(GagData g in lureList)
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
            else if(g.whichTarget == -1)
            {
                cogAllList.Add(g);
            }
        }

        for(int i = 0; i < 5; i++)
        {
            var cogList = new List<GagData>();

            if(i == 0)
            {
                cogList = cogAllList;
            }
            else if(i == 1)
            {
                cogList = cogOneList;
            }
            else if(i == 2)
            {
                cogList = cogTwoList;
            }
            else if(i == 3)
            {
                cogList = cogThreeList;
            }
            else if(i == 4)
            {
                cogList = cogFourList;
            }

            if(cogList.Count > 1)
            {
                cogList = OrderGagList(cogList);
            }
            
            if(cogList.Count > 0)
            {
                if(cogList[0].whichTarget != -1)
                {
                    if(battleCell.cogs[cogList[0].whichTarget].GetComponent<CogBattle>().isDead)
                    {
                        print("Cog is already dead, we can't attack him.");
                        break; // Cog is dead, so we do not need to calculate this Battle Calculation // Might be in the wrong spot though.
                    }
                }
                

                var battleCalc = new BattleCalculation();

                battleCalc.whichCog = cogList[0].whichTarget;

                foreach(GagData g in cogList)
                {
                    battleCalc.gagDataList.Add(g);
                }

                var cogs = new List<GameObject>();

                if(i == 0)
                {
                    foreach(GameObject g in battleCell.cogs)
                    {
                        if(!g.GetComponent<CogBattle>().isLured) // if we are not lured, then we are added to the list we hit
                        {
                            cogs.Add(g);
                        }
                    }
                }
                else
                {
                    cogs.Add(battleCell.cogs[battleCalc.whichCog]);
                }

                if(i == 0)
                {
                    battleCalc.didHit = CalculateLureHit(battleCalc.gagDataList, cogs);
                    print("An AoE lure was used");
                    allLuredUsed = true;
                }
                else
                {
                    if(allLuredUsed)
                    {
                        print("An AoE lure was used but we are not in that group, so we get the AoE lure accuracy.");
                        battleCalc.didHit = battleCalculationList[0].didHit;
                    }
                    else
                    {
                        battleCalc.didHit = CalculateLureHit(battleCalc.gagDataList, cogs);
                    }
                    
                }
                

                battleCalculationList.Add(battleCalc);
            }
        }

        battleMovie.SendLureMovies(battleCalculationList);
    }

    [Server]
    public void ExecuteCalcLure(List<BattleCalculation> battleCalculationList)
    {
        foreach(BattleCalculation b in battleCalculationList)
        {
            List<GameObject> whichCogs = new List<GameObject>();

            int luredRounds = b.gagDataList[b.gagDataList.Count - 1].gag.power;

            if(b.whichCog == -1)
            {
                foreach(GameObject g in battleCell.cogs)
                {
                    if(!g.GetComponent<CogBattle>().isLured)
                    {
                        whichCogs.Add(g);
                    }
                }
            }
            else
            {
                whichCogs.Add(battleCell.cogs[b.whichCog]);
            }

            if(b.didHit)
            {
                foreach(GameObject g in whichCogs)
                {
                    if(!g.GetComponent<CogBattle>().isTrapped)
                    {
                        g.GetComponent<CogBattle>().isLured = true;
                        g.GetComponent<CogBattle>().luredRounds = luredRounds;
                    }
                }
            }
        }

        CalcTrapEnd();
    }

    [Server]
    void CalcSound()
    {
        var battleCalculationList = new List<BattleCalculation>();

        var soundList = new List<GagData>();

        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagTrack == GagTrack.SOUND)
            {
                soundList.Add(g);
                print("Found SOUND gag.");
            }
        }

        if(soundList.Count == 0)
        {
            print("No SOUND gags found. Moving on...");
            NextTrack();
            return;
        }

        if(soundList.Count > 1)
        {
            soundList = OrderGagList(soundList);
        }

        if(soundList.Count > 0)
        {
            var battleCalc = new BattleCalculation();

            battleCalc.whichCog = -1;

            foreach(GagData g in soundList)
            {
                battleCalc.gagDataList.Add(g);
            }

            battleCalc.didHit = CalculateAttackHit(battleCalc.gagDataList, battleCell.cogs);

            battleCalculationList.Add(battleCalc);
        }

        battleMovie.SendSoundMovies(battleCalculationList);
    }

    [Server]
    public void ExecuteCalcSound(List<BattleCalculation> battleCalculationList)
    {
        foreach(BattleCalculation b in battleCalculationList)
        {
            int index = 0;
            float teamBonus = 0;
            int dmg = 0;

            foreach(GagData g in b.gagDataList)
            {
                dmg += g.gag.power;
            }

            if(b.gagDataList.Count > 1)
            {
                teamBonus = .2f;
            }

            int totalDmgTaken = (int)(dmg + (float)(dmg * teamBonus));

            foreach(GameObject g in battleCell.cogs)
            {
                GameObject whichCog = battleCell.cogs[index];
                var cogBattle = whichCog.GetComponent<CogBattle>();

                if(b.didHit)
                {
                    cogBattle.hp -= totalDmgTaken;
                    cogBattle.isLured = false;
                }

                index++;
            }
        }

        CheckIfCogsAreDead();
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
        if(battleCalculationList.Count == 0)
        {
            print("No Throw gags found. Moving on...");
            NextTrack();
            return;
        }

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
                // EnemyAttack();
                RemoveDeadCogs();
            }   
        }
        else
        {
            if(battleCell.battleState == BattleState.PLAYER_ATTACK)
            {
                NextTrack();
            }
            else if(battleCell.battleState == BattleState.ENEMY_ATTACK)
            {
                // PlayerChoose();
                RemoveDeadCogs();
                print("IDK what to put here.");
            }
        }
    }

    [Server]
    public void CheckIfToonsAreDead()
    {
        int toonsDead = 0;

        foreach(GameObject g in battleCell.toons)
        {
            if(g.GetComponent<PlayerBattle>().hp <= 0)
            {
                print("Toon is dead.");
                toonsDead++;
            }
        }

        if(toonsDead >= battleCell.toons.Count)
        {
            if(battleCell.toonsPending.Count == 0)
            {
                print("All Toons are dead. And no Toons are pending. Battle should be over.");
                Lose();
            }
            else
            {
                print("All Toons are dead. Toons are pending; Battle should continue.");
                PlayerChoose();
            }   
        }
    }

    [Server]
    public void RemoveDeadCogs() // Called after both toons and cogs attacked
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
            // Currently if a cog joins from far away before it can get there, 
        }
    }

    [Server]
    void NextTrack()
    {
        if(track == GagTrack.TRAP)
        {
            PlayerAttack(GagTrack.LURE);
        }
        else if(track == GagTrack.LURE)
        {
            PlayerAttack(GagTrack.SOUND);
        }
        else if(track == GagTrack.SOUND)
        {
            PlayerAttack(GagTrack.THROW);
        }
        else if(track == GagTrack.THROW)
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
        
        CheckIfCogsAreUnlured();

        CalcCogAttacks();

        // RemoveDeadCogs(); // back to player choose after attack // need to check if cogs died
    }

    [Server]
    void CheckIfCogsAreUnlured()
    {
        foreach(GameObject g in battleCell.cogs)
        {
            if(g.GetComponent<CogBattle>().isLured)
            {

                if(g.GetComponent<CogBattle>().luredRounds <= 0)
                {
                    g.GetComponent<CogBattle>().isLured = false;
                    g.GetComponent<CogBattle>().luredRounds = 0;
                    print("Cog is unlured.");
                }
                else
                {
                    g.GetComponent<CogBattle>().luredRounds--;
                }
            }
        }
    }

    [Server]
    void CalcCogAttacks() // maybe one at a time in case the Toon dies so we can pick a new target...
    {
        cogsAttackingList.Clear();

        foreach(GameObject g in battleCell.cogs)
        {
            if(!g.GetComponent<CogBattle>().isLured && !g.GetComponent<CogBattle>().isDead) // is not lured and is not dead
            {
                cogsAttackingList.Insert(0, g);
            }
        }

        if(cogsAttackingList.Count > 0)
        {
            CalcSingleCogAttack();
        }
        else
        {
            CheckIfCogsAreDead(); // prolly not
        }
    }

    [Server]
    public void CalcSingleCogAttack() // index = which Cog
    {
        var cog = cogsAttackingList[0];

        cogsAttackingList.RemoveAt(0);

        var battleCalculationCog = new BattleCalculationCog();
        
        var cogBattle = cog.GetComponent<CogBattle>();
        var cogSO = cog.GetComponent<CogLoad>().cog;
        int cogTierIndex = cogBattle.level - cogSO.minCogLevel;

        battleCalculationCog.whichCog = battleCell.cogs.IndexOf(cog);
        battleCalculationCog.cogAttack = cogSO.cogAttacks[Random.Range(0, cogSO.cogAttacks.Count)];

        battleCalculationCog.dmg = battleCalculationCog.cogAttack.damages[cogTierIndex];

        for(int i = 0; i < 4; i++) // get four probabilities if aoe attack
        {
            battleCalculationCog.didHitList.Add(CalcCogAttackHit(battleCalculationCog.cogAttack, cogBattle.level));
        }

        battleMovie.SendCogMovie(battleCalculationCog);
    }

    [Server]
    public void ExecuteCogAttack(BattleCalculationCog battleCalculationCog) // one at a time for real time health updates
    {
        var b = battleCalculationCog;

        if(b.cogAttack.areaOfEffect)
        {
            int index = 0;

            print("Hitting all toons.");

            foreach(GameObject g in battleCell.toons)
            {
                if(b.didHitList[index])
                {
                    g.GetComponent<PlayerBattle>().hp -= b.dmg;
                }

                index++;
            }
        }
        else
        {
            if(b.didHitList[0])
            {
                battleCell.toons[b.whichTarget].GetComponent<PlayerBattle>().hp -= b.dmg;
            }
        }

        print($"Cog attack: {b.cogAttack.attackName}, Target: {b.whichTarget}, Did Hit: {b.didHitList[0]}, Damage: {b.dmg}");
    }

    [Server]
    bool CalcCogAttackHit(CogAttack cogAttack, int cogLevel) // Remember to calculate in the cogs min level and max level for accuracy array 
    {
        int acc = cogAttack.accuracy[cogLevel];

        int rand = Random.Range(0,100);

        if(rand < acc)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    [Server]
    public void Win()
    {
        battleCell.battleState = BattleState.WIN;

        RemoveDeadCogs(); // Might need to call here too

        print("Toons win.");

        RpcVictoryMovie();
    }

    [ClientRpc]
    void RpcVictoryMovie()
    {
        StartCoroutine(Victory());
    }

    IEnumerator Victory()
    {
        foreach(GameObject g in battleCell.toons)
        {
            g.GetComponent<PlayerAnimate>().ChangeAnimationState("Dance");
        }

        yield return new WaitForSeconds(5f);

        if(isServer)
        {
            EndBattle();
        }
    }

    [Server]
    void EndBattle()
    {
        print("Battle ended.");

        battleCell.battleState = BattleState.IDLE;
        battleCell.colliderEnabled = false;
        battleCell.toons.Clear();
        battleCell.toonIDs.Clear();
        battleCell.cogs.Clear();

        foreach(GameObject g in battleCell.toons)
        {
            TargetEnableMovement(g.GetComponent<NetworkIdentity>().connectionToClient);
        }

        if(isServer && isClient)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<PlayerMove>().EnableMovement();

            var cam = Camera.main.gameObject;
            cam.SetActive(false);
            cam.SetActive(true);
            NetworkClient.localPlayer.gameObject.GetComponent<PlayerBattle>().battleCell = null;
            battleMovie.battleCamera.SetActive(false);
            battleMovie.SwitchCamera(null);
        }
    }

    [TargetRpc]
    void TargetEnableMovement(NetworkConnection conn)
    {
        print("Called target");
        NetworkClient.localPlayer.gameObject.GetComponent<PlayerMove>().EnableMovement();
        var cam = Camera.main.gameObject;
        cam.SetActive(false);
        cam.SetActive(true);
        NetworkClient.localPlayer.gameObject.GetComponent<PlayerBattle>().battleCell = null;
        battleMovie.battleCamera.SetActive(false);
        battleMovie.SwitchCamera(null); // turn off all the cameras after battle is over.
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
                if(!g.GetComponent<CogBattle>().isDead) // check if cog is dead 
                {
                    highestCogLevel = g.GetComponent<CogBattle>().level;
                }
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

    private bool CalculateLureHit(List<GagData> gagDatas, List<GameObject> targets)
    {
        int rand = Random.Range(0,100);

        int highestGagLevel = 0;
        int highestCogLevel = 0;
        int propAcc = 0;
        bool trapBonus = false;

        foreach(GagData g in gagDatas)
        {
            if(g.gag.gagLevel >= highestGagLevel)
            {
                highestGagLevel = g.gag.gagLevel;

                propAcc = g.gag.acc;
            }
        }

        foreach(GameObject g in targets)
        {
            if(g.GetComponent<CogBattle>().level > highestCogLevel)
            {
                highestCogLevel = g.GetComponent<CogBattle>().level;
            }
        }

        int atkAcc = propAcc + ((highestGagLevel) * 10) + tgtDef[highestCogLevel] + (trapBonus ? 20 : 0);

        if(atkAcc > 95) {atkAcc = 95;}

        print($"LURE ACCURACY: {atkAcc} --- TRAP BONUS: {trapBonus}");

        // if(atkAcc > rand)
        // {
        //     return true;
        // }
        // else
        // {
        //     return false;
        // }

        return true; // RETURNING TRUE FOR TEST PURPOSES RIGHT NOW
    }

    [TargetRpc]
    public void TargetPlayerChoose(NetworkConnection conn)
    {
        BattleUIManager.Instance.ShowBattleUI();
        BattleUIManager.Instance.UpdateHP_UI(battleCell);

        battleMovie.SwitchCamera(battleMovie.battleCamera); // switch camera to top view
    }
}

public enum BattleState {IDLE, START, PLAYER_CHOOSE, PLAYER_ATTACK, ENEMY_ATTACK, WIN, LOSE}




