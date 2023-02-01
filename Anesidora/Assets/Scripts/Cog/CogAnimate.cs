using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CogAnimate : NetworkBehaviour
{
    public string currentState;
    public Animator cogAnim;

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
}
