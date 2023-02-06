using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager Instance { get; private set; }
    public PlayerControls playerControls;
    public bool pressedJump;

    private void Awake()
    {
        if (Instance != null && Instance != this) {Destroy(this);}
        else {Instance = this;}

        Application.runInBackground = true;
    }

    private void Start()
    {
        playerControls = new PlayerControls();

        playerControls.Player.MoveH.performed += ctx => MoveH();
        playerControls.Player.MoveV.performed += ctx => MoveV();
        playerControls.Player.Jump.started += ctx => Jump(true);
        playerControls.Player.Jump.canceled += ctx => Jump(false);
        
        OnEnable();
    }

    private void OnEnable()
    {
        if (playerControls != null) {playerControls.Enable();}
    }

    private void OnDisable()
    {
        if (playerControls != null) {playerControls.Disable();}
    }

    private void MoveV()
    {
        // print("pressed v key");
    }

    private void MoveH()
    {
        // print("pressed h key");
    }

    private void Enter()
    {
        // ChatManager.Instance.CallSendChat();
    }
    private void Jump(bool isJumping)
    {
        pressedJump = isJumping;
    }
}
