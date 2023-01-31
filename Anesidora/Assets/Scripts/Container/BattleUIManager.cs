using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    public GameObject battleUI;
    public GameObject[] toonHps, cogHps;

    private void Awake()
    {
        if (Instance != null && Instance != this) {Destroy(this);}
        else {Instance = this;}
    }

    public void ShowBattleUI()
    {
        battleUI.SetActive(true);
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
}
