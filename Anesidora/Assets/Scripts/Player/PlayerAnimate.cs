using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerAnimate : NetworkBehaviour
{
    public string currentState;
    public PlayerLoad playerLoad;
    public GameObject pieSpot;

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
}
