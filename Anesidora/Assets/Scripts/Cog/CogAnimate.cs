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
    public GameObject damageText;

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

    public void CallAnimateDamageText(string message)
    {
        StopCoroutine("AnimateDamageText");
        StartCoroutine(AnimateDamageText(message));
    }

    IEnumerator AnimateDamageText(string message)
    {
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
}
