using System;
using UnityEngine;

public enum PlayerState {
    Walking,
    Running,
    Crouching,
    CarryingEnemy,
}

public class PlayerStateManager : MonoBehaviour {
    public PlayerState playerState { get; private set; }

    [Header("GameInputs")]
    [SerializeField] private GameInputs gameInputs;

    private void Start() {
        playerState = PlayerState.Walking;

        gameInputs.OnCrouchPerformed += OnToggleCrouch;
        gameInputs.OnRunPerformed += OnToggleRunning;
    }

    private void OnToggleRunning(object sender, EventArgs e) {
        playerState = playerState == PlayerState.Running ? PlayerState.Walking : PlayerState.Running;
    }

    private void OnToggleCrouch(object sender, EventArgs e) {
        if (playerState == PlayerState.CarryingEnemy) return;

        playerState = playerState == PlayerState.Crouching ? PlayerState.Walking : PlayerState.Crouching;
    }

    public void SetPlayerState(PlayerState state) {
        playerState = state;
    }
}