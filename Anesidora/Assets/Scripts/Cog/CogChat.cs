using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class CogChat : NetworkBehaviour
{
    public GameObject speechBubble, nametag;
    public TMP_Text chatText;

    public void CogTalk(string chatMessage)
    {
        StopCoroutine("AnimateChatBubble");
        StartCoroutine(AnimateChatBubble(chatMessage));
    }

    IEnumerator AnimateChatBubble(string chatMessage)
    {
        LeanTween.scale(speechBubble, Vector3.zero, 0);

        speechBubble.SetActive(true);

        nametag.SetActive(false);

        chatText.text = chatMessage;

        LeanTween.scale(speechBubble, Vector3.one, .4f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(5f);

        LeanTween.scale(speechBubble, Vector3.zero, .4f).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(.5f);

        speechBubble.SetActive(false);
        
        nametag.SetActive(true);
    }
}
