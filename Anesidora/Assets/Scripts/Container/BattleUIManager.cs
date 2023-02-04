using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    public GameObject battleUI, gagPanel, cogSelectPanel;
    public GameObject[] toonHps, cogHps, cogSelectButtons;
    public List<Gag>throwGags = new List<Gag>();
    public List<Gag>lureGags = new List<Gag>();
    private Gag gagSelected;
    private int cogSelected;

    [HideInInspector]
    public GameObject localPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) {Destroy(this);}
        else {Instance = this;}
    }

    public void ShowBattleUI()
    {
        battleUI.SetActive(true);

        gagPanel.SetActive(true);
        cogSelectPanel.SetActive(false);
    }

    public void UpdateHP_UI(BattleCell battleCell)
    {
        for(int i = 0; i < toonHps.Length; i++)
        {
            toonHps[i].SetActive(false);
        }

        for(int i = 0; i < cogHps.Length; i++)
        {
            cogHps[i].SetActive(false);
        }

        int index = 0;

        foreach(GameObject g in battleCell.toons)
        {
            toonHps[index].SetActive(true);
            toonHps[index].GetComponentInChildren<TMP_Text>().text = $"{g.GetComponent<PlayerBattle>().hp}/{g.GetComponent<PlayerBattle>().maxHp}";
            index++;
        }

        index = 0;

        foreach(GameObject g in battleCell.cogs)
        {
            cogHps[index].SetActive(true);
            cogHps[index].GetComponentInChildren<TMP_Text>().text = $"{g.GetComponent<CogBattle>().hp}/{g.GetComponent<CogBattle>().maxHp}";
            index++;
        }
    }

    public void SelectThrowGag(int val)
    {
        gagSelected = throwGags[val];

        if(gagSelected.attackAll)
        {
            cogSelected = -1;
            SendGagData();
        }
        else if(localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>().cogs.Count < 2)
        {
            cogSelected = 0;
            SendGagData();
        }
        else
        {
            ShowSelectCogPanel();
        }
    }

    public void SelectLureGag(int val)
    {
        gagSelected = lureGags[val];

        if(gagSelected.attackAll)
        {
            cogSelected = -1;
            SendGagData();
        }
        else if(localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>().cogs.Count < 2)
        {
            cogSelected = 0;
            SendGagData();
        }
        else
        {
            ShowSelectCogPanel();
        }
    }

    public void ShowSelectCogPanel()
    {
        gagPanel.SetActive(false);

        foreach(GameObject g in cogSelectButtons)
        {
            g.SetActive(false);
        }

        for(int i = 0; i < localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>().cogs.Count; i++)
        {
            cogSelectButtons[i].SetActive(true);
        }

        cogSelectPanel.SetActive(true);
    }

    public void SelectCog(int val)
    {
        cogSelected = val;

        SendGagData();
    }

    public void HideBattleUI()
    {
        battleUI.SetActive(false);
        gagPanel.SetActive(false);
        cogSelectPanel.SetActive(false);
    }

    public void SendGagData()
    {
        HideBattleUI();

        GagData gagData = new GagData();

        gagData.gag = gagSelected;
        gagData.whichTarget = cogSelected;
        gagData.whichToon = -1; // Needs to be changed in Network Behaviour

        localPlayer.GetComponent<PlayerBattle>().SendGagData(gagData);
    }

    public void SpawnCog()
    {
        localPlayer.GetComponent<PlayerBattle>().SpawnCog();
    }
}
