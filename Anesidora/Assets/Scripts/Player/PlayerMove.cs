using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerMove : NetworkBehaviour
{
    private CharacterController characterController;
    #region Movement Values
    public int moveSpeed, turnSpeed, jumpForce;
    public bool canMove, isGrounded;
    private Vector3 velocity;
    private float gravity = -70f;
    private float horizontal, vertical;
    public AudioSource runAudioSource;
    public AudioClip runClip, walkClip;
    private string audioState;

    #endregion

    [HideInInspector]
    public PhysicsScene physicsScene;  
    public Transform GroundCheck;  
    public float groundDistance = .4f;
    public LayerMask groundLayer;
    public PlayerAnimate playerAnimate;


    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) { return; }

        base.OnStartLocalPlayer();
        characterController = GetComponent<CharacterController>();

        physicsScene = gameObject.scene.GetPhysicsScene();
        canMove = true;
    }

    private void Update()
    {
        if (!isLocalPlayer) { return; }

        if (characterController == null) { return; }

        if(canMove) { Move(); }

        Gravity();

        if(ControlsManager.Instance.pressedJump && isGrounded && canMove) {StartCoroutine(Jump());}
    }

    private void Move()
    {
        horizontal = ControlsManager.Instance.playerControls.Player.MoveH.ReadValue<float>(); // read horizontal input
        vertical = ControlsManager.Instance.playerControls.Player.MoveV.ReadValue<float>(); // read vertical input // 
        Vector2 moveDirection = new Vector2(horizontal, vertical); // the direction the player is going to move from the toonControls

        if(canMove)
        {
            transform.Rotate(moveDirection.x * Time.deltaTime * turnSpeed * Vector3.up); // rotate player

            if(vertical > 0)
                characterController.Move(moveSpeed * Time.deltaTime * transform.TransformDirection(Vector3.forward * moveDirection.y));  // move player
            else
                characterController.Move(moveSpeed * .5f * Time.deltaTime * transform.TransformDirection(Vector3.forward * moveDirection.y));  // move player
        }
        else
        {
            moveDirection = new Vector2(0, 0); // for animation to be idle
        }

        Animate(moveDirection);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }

        if(physicsScene != null)
        {
            Physics.autoSimulation = false;
            physicsScene.Simulate(Time.fixedDeltaTime * 1);
        }
        
        RaycastHit hit;

        isGrounded = (physicsScene.SphereCast(GroundCheck.position, groundDistance, -transform.up, out hit, groundDistance, groundLayer));
    }

    public IEnumerator Jump()
    {
        yield return new WaitForSeconds(0.005f);
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void Gravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        if(characterController.enabled) {characterController.Move(velocity * Time.deltaTime);}
        
    }

    public void DisableMovement()
    {
        canMove = false;
        characterController.enabled = false;
    }

    public void EnableMovement()
    {
        canMove = true;
        characterController.enabled = true;
    }

    private void Animate(Vector2 direction)
    {
        if(!canMove) {return;}
        
        if(!isGrounded)
        {
            PlayAudio("None");

            if(playerAnimate.currentState != "Leap" && playerAnimate.currentState != "Jump")
            {
                RaycastHit hit;
                bool didHit = false;

                if(physicsScene.Raycast(GroundCheck.position, Vector3.down, out hit, 1f, groundLayer))
                {
                    print("Did hit!");
                    didHit = true;
                }

                if(!didHit)
                {
                    if(direction.y != 0)
                        playerAnimate.ChangeAnimationState("Leap");
                    else
                        playerAnimate.ChangeAnimationState("Jump");
                }
            }
                
        }
        else if(direction.y == 1)
        {
            PlayAudio("Running");
            playerAnimate.ChangeAnimationState("Run");
        }
        else if(direction.y == -1)
        {
            PlayAudio("Walking");
            playerAnimate.ChangeAnimationState("Walk");
        }
        else if(direction.x != 0)
        {
            PlayAudio("Walking");
            playerAnimate.ChangeAnimationState("Turn");
        }
        else
        {
            PlayAudio("None");
            playerAnimate.ChangeAnimationState("Idle");
        }
    }

    private void PlayAudio(string audioState)
    {
        if(audioState == "Running")
        {
            if(!runAudioSource.isPlaying || runAudioSource.clip != runClip)
            {
                runAudioSource.clip = runClip;
                runAudioSource.Play();
            }
        }
        else if(audioState == "Walking")
        {
            if(!runAudioSource.isPlaying || runAudioSource.clip != walkClip)
            {
                runAudioSource.clip = walkClip;
                runAudioSource.Play();
            }
        }
        else if(audioState == "None")
        {
            if(runAudioSource.isPlaying)
            {
                runAudioSource.clip = null;
                runAudioSource.Stop();
            }
        }
    }
}
