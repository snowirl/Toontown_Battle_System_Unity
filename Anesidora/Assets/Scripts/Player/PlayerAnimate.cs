using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerAnimate : NetworkBehaviour
{
    public string currentState;
    public PlayerLoad playerLoad;
    public GameObject pieSpot, leftHandSpot;
    public GameObject damageText;

    public void ChangeAnimationState(string newState)
    {
        // if (toonDNA.legsInstance == null || toonDNA.torsoInstance == null) return;

        if (currentState == newState) return;

        currentState = newState;
        CmdSendAnimation(newState);
        Animate(newState);

    }

    [Command]
    private void CmdSendAnimation(string anim)
    {
        RpcReceiveAnimation(anim);
    }

    [ClientRpc]
    private void RpcReceiveAnimation(string anim)
    {
        if(isLocalPlayer) { return; }

        Animate(anim);
    }

    public void Animate(string newState)
    {
        playerLoad.torsoInstance.GetComponent<Animator>().Play(newState);
        playerLoad.legsInstance.GetComponent<Animator>().Play(newState);
    }

    public IEnumerator BattleAnimate(string newState)
    {
        playerLoad.torsoInstance.GetComponent<Animator>().Play(newState);
        playerLoad.legsInstance.GetComponent<Animator>().Play(newState);

        yield return new WaitForEndOfFrame();

        while(playerLoad.legsInstance.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        print("Animation finished.");

        Animate("Idle");
    }

    public void CallAnimateDamageText(string message, string color) // 
    {
        LeanTween.cancelAll();
        StopCoroutine("AnimateDamageText");
        StartCoroutine(AnimateDamageText(message, color));
    }

    public Transform GetLeftHand()
    {
        return leftHandSpot.transform;
    }

    IEnumerator AnimateDamageText(string message, string color)
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
