using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour {
    private PlayerState playerState;

    // private int currentState;
    // private int previousState;
    // private readonly int Crouching = Animator.StringToHash("Crouching");
    // private readonly int Sneaking = Animator.StringToHash("Sneaking");
    // private readonly int RunningIdle = Animator.StringToHash("RunningIdle");
    // private readonly int Running = Animator.StringToHash("Running");
    // private readonly int WalkingIdle = Animator.StringToHash("WalkingIdle");
    // private readonly int Walking = Animator.StringToHash("Walking");
    // private readonly int Stabbing = Animator.StringToHash("Stabbing");

    private bool isMoving = false;
    private bool isRunning = false;
    private bool isCrouching = false;
    public bool IsAttacking { get; private set; } = false;

    private Animator anim;

    private void Start() {
        anim = GetComponent<Animator>();
    }

    private void Update() {
        playerState = Player.Instance.playerState;

        isMoving = Player.Instance.IsWalking;
        isCrouching = playerState == PlayerState.Crouching;
        isRunning = playerState == PlayerState.Running;

        anim.SetBool("IsMoving", isMoving);
        anim.SetBool("IsRunning", isRunning);
        anim.SetBool("IsCrouching", isCrouching);
    }

    public void Attack() {
        if (IsAttacking) return;

        IsAttacking = true;
        anim.SetTrigger("IsAttacking");
    }

    public void EndAttack() {
        IsAttacking = false;
    }

    // private void Update() {
    //     isMoving = GetComponent<PlayerMovement>().IsWalking;
    //     currentState = GetState();

    //     if (currentState == previousState) return;

    //     previousState = currentState;
    //     anim.CrossFade(currentState, 0.1f, 0);
    // }

    // private int GetState() {
    //     playerState = GetComponent<PlayerStateManager>().playerState;

    //     if (playerState == PlayerState.Running) {
    //         return isMoving ? Running : WalkingIdle;
    //     }
    //     if (playerState == PlayerState.Crouching) {
    //         return isMoving ? Sneaking : Crouching;
    //     }

    //     return isMoving ? Walking : WalkingIdle;
    // }
}
