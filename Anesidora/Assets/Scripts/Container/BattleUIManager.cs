using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    public GameObject battleUI, gagPanel, cogSelectPanel;
    public GameObject[] toonHps, cogHps, cogSelectButtons;
    public List<Gag>throwGags = new List<Gag>();
    public List<Gag>lureGags = new List<Gag>();
    public List<Gag>trapGags, soundGags, toonupGags = new List<Gag>();
    private Gag gagSelected;
    private int cogSelected;

    [HideInInspector]
    public GameObject localPlayer;
    public GameObject CogAttackText;
    public GameObject lurePanel, trapPanel, toonupPanel;
    public bool[] cogsTrappedArray = new bool[4];
    public bool[] cogsLuredArray = new bool[4];
    public GameObject backButton;
    public Image gagSelectedImage;
    public TMP_Text cogSelectedText; // ex: X--- WHICH COG SELECTED


    private void Awake()
    {
        if (Instance != null && Instance != this) {Destroy(this);}
        else {Instance = this;}
    }

    public void ShowBattleUI()
    {
        var battleCell = localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>();

        battleUI.SetActive(true);

        gagPanel.SetActive(true);
        cogSelectPanel.SetActive(false);

        if(battleCell.toons.Count < 2) // if there is only one toon in battle
        {
            foreach(Button b in toonupPanel.GetComponentsInChildren<Button>())
            {
                b.interactable = false;
            }
        }
        else
        {
            foreach(Button b in toonupPanel.GetComponentsInChildren<Button>())
            {
                b.interactable = true;
            }
        }

        int index = 0;
        int cogsLured = 0;
        int cogsTrapped = 0;
        bool allLured = false;


        foreach(GameObject g in battleCell.cogs)
        {
            if(g.GetComponent<CogBattle>().isLured)
            {
                cogsLuredArray[index] = true;
                cogsTrappedArray[index] = false;
                cogsLured++;
            }
            else if(g.GetComponent<CogBattle>().isTrapped)
            {
                cogsTrappedArray[index] = true;
                cogsLuredArray[index] = false;
                cogsTrapped++;
            }
            else
            {
                cogsTrappedArray[index] = false;
                cogsLuredArray[index] = false;
            }

            index++;
        }

        if(cogsLured >= battleCell.cogs.Count) // if all cogs are lured
        {
            foreach(Button b in lurePanel.GetComponentsInChildren<Button>())
            {
                b.interactable = false;
            }

            allLured = true;
        }
        else
        {
            foreach(Button b in lurePanel.GetComponentsInChildren<Button>())
            {
                b.interactable = true;
            }
        }

        if(cogsTrapped >= battleCell.cogs.Count || allLured) // if all cogs are trapped or all are lured
        {
            foreach(Button b in trapPanel.GetComponentsInChildren<Button>())
            {
                b.interactable = false;
            }
        }
        else
        {
            foreach(Button b in trapPanel.GetComponentsInChildren<Button>())
            {
                b.interactable = true;
            }
        }
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

    public void SelectTrapGag(int val)
    {
        gagSelected = trapGags[val];

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

    public void SelectToonUpGag(int val)
    {
        gagSelected = toonupGags[val];
        
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

    public void SelectSoundGag(int val)
    {
        gagSelected = soundGags[val];

        cogSelected = -1;
        SendGagData();
    }

    public void ShowSelectCogPanel()
    {
        var battleCell = localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>();
        gagPanel.SetActive(false);
        backButton.SetActive(true);

        foreach(GameObject g in cogSelectButtons)
        {
            g.SetActive(false);
        }

        for(int i = 0; i < battleCell.cogs.Count; i++)
        {
            cogSelectButtons[i].SetActive(true);
        }

        if(gagSelected.gagTrack == GagTrack.LURE)
        {
            int index = 0;

            foreach(GameObject g in cogSelectButtons)
            {
                if(cogsLuredArray[index])
                {
                    g.GetComponent<Button>().interactable = false;
                }
                else
                {
                    if(index <= battleCell.cogs.Count - 1)
                    {
                        g.GetComponent<Button>().interactable = true;
                    }
                    
                }

                index++;
            }
        }

        if(gagSelected.gagTrack == GagTrack.TRAP)
        {
            int index = 0;

            foreach(GameObject g in cogSelectButtons)
            {
                if(cogsLuredArray[index] || cogsTrappedArray[index])
                {
                    g.GetComponent<Button>().interactable = false;
                }
                else
                {   
                    if(index <= battleCell.cogs.Count - 1)
                    {
                        g.GetComponent<Button>().interactable = true;
                    }
                }

                index++;
            }
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
        backButton.SetActive(false);
    }

    public void SendGagData()
    {
        HideBattleUI();

        GagData gagData = new GagData();

        gagData.gag = gagSelected;
        gagData.whichTarget = cogSelected;
        gagData.whichToon = -1; // Needs to be changed in Network Behaviour

        var battleCell = localPlayer.GetComponent<PlayerBattle>().battleCell.GetComponent<BattleCell>();

        localPlayer.GetComponent<PlayerBattle>().SendGagData(gagData);

        // gagSelectedImage.sprite = gagSelected.gagSprite;

        // cogSelectedText.text = string.Empty;
        
        // if(cogSelected == -1)
        // {
        //     foreach(GameObject g in battleCell.cogs)
        //     {
        //         cogSelectedText.text += "X";
        //     }
        // }
        // else
        // {
        //     int index = 0;

        //     foreach(GameObject g in battleCell.cogs)
        //     {
        //         if(index == cogSelected)
        //         {
        //             cogSelectedText.text += "X";
        //         }
        //         else
        //         {
        //             cogSelectedText.text += "-";
        //         }
                
        //     }
        // }
    }

    public void SpawnCog()
    {
        localPlayer.GetComponent<PlayerBattle>().SpawnCog();
    }

    public void GoBack()
    {
        backButton.SetActive(false);

        gagSelected = null;
        cogSelected = 0;

        cogSelectPanel.SetActive(false);
        gagPanel.SetActive(true);
    }

    public void SetCogAttackText(string attackName)
    {
        StartCoroutine(CogAttackTextCO(attackName));
    }

    public IEnumerator CogAttackTextCO(string attackName)
    {
        CogAttackText.LeanScale(Vector3.zero, 0);
        CogAttackText.SetActive(true);
        CogAttackText.GetComponentInChildren<TMP_Text>().text = $"{attackName}!";
        CogAttackText.LeanScale(Vector3.one, .45f).setEase(LeanTweenType.easeOutBounce);

        yield return new WaitForSeconds(5f);

        CogAttackText.LeanScale(Vector3.zero, .45f).setEase(LeanTweenType.easeOutCubic);
        
        yield return new WaitForSeconds(.5f);

        CogAttackText.SetActive(false);
    }
}
