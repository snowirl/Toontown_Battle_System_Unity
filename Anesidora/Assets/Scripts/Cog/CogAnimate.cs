using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class CogAnimate : NetworkBehaviour
{
    public string currentState;
    public Animator cogAnim, loseCogAnim;
    public GameObject suit, suitLose;
    public GameObject damageText, splatContainer;
    public Color redColor, orangeColor, yellowColor, greenColor;
    public GameObject cogHealthButton;
    public Material redMat, orangeMat, yellowMat, greenMat, blackMat;

    [Server]
    public void ChangeAnimationState(string newState)
    {
        // if (toonDNA.legsInstance == null || toonDNA.torsoInstance == null) return;

        if (currentState == newState) return;

        currentState = newState;
        Animate(newState);
    }

    [ClientRpc]
    private void RpcReceiveAnimation(string anim)
    {
        if(isServerOnly) { return; }

        Animate(anim);
    }

    public void Animate(string newState)
    {
        cogAnim.Play(newState);
    }

    public IEnumerator BattleAnimate(string newState)
    {
        cogAnim.Play(newState);

        yield return new WaitForEndOfFrame();

        while(cogAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        Animate("Idle");
    }

    public IEnumerator ExplodeCog()
    {
        suit.SetActive(false);
        suitLose.SetActive(true);

        loseCogAnim.Play("Lose");

        yield return new WaitForEndOfFrame();

        while(loseCogAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        suitLose.SetActive(false);
    }

    public void CallAnimateDamageText(string message, string color) // 
    {
        LeanTween.cancelAll();

        StopCoroutine("AnimateDamageText");
        StartCoroutine(AnimateDamageText(message, color));
    }

    IEnumerator AnimateDamageText(string message, string color)
    {
        if(color == "red")
        {
            damageText.GetComponentInChildren<TMP_Text>().color = redColor;
        }
        else if (color == "orange")
        {
            damageText.GetComponentInChildren<TMP_Text>().color = orangeColor;
        }
        else if (color == "yellow")
        {
            damageText.GetComponentInChildren<TMP_Text>().color = yellowColor;
        }

        CanvasGroup canvasGroup = damageText.GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(canvasGroup, 0, 0);

        damageText.GetComponentInChildren<TMP_Text>().text = message;

        damageText.LeanMoveLocalY(-600, 0);

        damageText.SetActive(true);

        yield return new WaitForEndOfFrame();

        damageText.LeanMoveLocalY(350, 1.75f).setEase(LeanTweenType.easeOutCubic);

        LeanTween.alphaCanvas(canvasGroup, 1, .25f);

        yield return new WaitForSeconds(2f);

        LeanTween.alphaCanvas(canvasGroup, 0, .25f);

        yield return new WaitForSeconds(.25f);

        damageText.SetActive(false);

    }

    public void ChangeHealthButton (int dmg)
    {

        int hp = GetComponent<CogBattle>().hp;
        int maxHp = GetComponent<CogBattle>().maxHp;
        Material mat = null;

        hp -= dmg;

        float healthPercent = ((float)hp / (float)maxHp) * 100f;

        if(healthPercent > 95)
        {
            // green
            mat = greenMat;
        }
        else if(healthPercent > 70)
        {
            // yellow
            mat = yellowMat;
            
        }
        else if(healthPercent > 25)
        {
            // orange
            mat = orangeMat;
        }
        else if(healthPercent > 0)
        {
            // red 
            mat = redMat;
        }
        else
        {
            // dead
            mat = redMat;
            StartCoroutine(RedBlinking());
        }

        cogHealthButton.GetComponent<Renderer>().material = mat;

        print("Ran");
    }

    IEnumerator RedBlinking()
    {
        yield return new WaitForSeconds(.2f);

        cogHealthButton.GetComponent<Renderer>().material = redMat;

        StartCoroutine(BlackBlinking());
    }

    IEnumerator BlackBlinking()
    {
        yield return new WaitForSeconds(.2f);

        cogHealthButton.GetComponent<Renderer>().material = blackMat;

        StartCoroutine(RedBlinking());
    }
}
