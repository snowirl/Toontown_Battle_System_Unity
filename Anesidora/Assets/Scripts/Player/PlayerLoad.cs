using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerLoad : NetworkBehaviour
{
    public Transform lookTarget; // where the camera should look
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if(!isLocalPlayer) {return;}

        LoadCamera();

        BattleUIManager.Instance.localPlayer = this.gameObject;
    }

    private void LoadCamera()
    {
        var cam = Camera.main.gameObject;

        var cinemachineVirtualCameras = cam.GetComponentsInChildren<CinemachineVirtualCamera>();
        foreach(CinemachineVirtualCamera c in cinemachineVirtualCameras)
        {
            c.LookAt = lookTarget;
            c.Follow = lookTarget;
        }
    }
}
