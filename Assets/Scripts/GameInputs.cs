using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputs : MonoBehaviour {
    public event EventHandler OnBackStabPerformed;
    public event EventHandler OnCrouchPerformed;
    public event EventHandler OnRunPerformed;
    public event EventHandler OnCarryEnemyPerformed;
    public event EventHandler OnDashPerformed;
    public event EventHandler OnInvisibilityPerformed;
    public event EventHandler OnViewMap;

    public static GameInputs Instance { get; private set; }
    private PlayerInputs playerInputs;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
        playerInputs = new PlayerInputs();
    }

    private void Start() {
        playerInputs.Player.Enable();

        playerInputs.Player.BackStab.performed += OnPlayerBackStab;
        playerInputs.Player.Dash.performed += OnPlayerDash;
        playerInputs.Player.Invisibility.performed += OnPlayerInvisibility;
        playerInputs.Player.ViewMap.performed += OnPlayerViewMap;
        playerInputs.Player.Crouch.performed += OnPlayerCrouch;
        playerInputs.Player.Run.performed += OnPlayerRun;
        playerInputs.Player.CarryEnemy.performed += OnPlayerCarryEnemy;
    }

    private void OnDestroy() {
        playerInputs.Player.BackStab.performed -= OnPlayerBackStab;
        playerInputs.Player.Dash.performed -= OnPlayerDash;
        playerInputs.Player.Invisibility.performed -= OnPlayerInvisibility;
        playerInputs.Player.ViewMap.performed -= OnPlayerViewMap;

        playerInputs.Dispose();
    }

    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerInputs.Player.Move.ReadValue<Vector2>();

        return inputVector;
    }

    private void OnPlayerBackStab(InputAction.CallbackContext obj) {
        OnBackStabPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerDash(InputAction.CallbackContext obj) {
        OnDashPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerInvisibility(InputAction.CallbackContext obj) {
        OnInvisibilityPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerViewMap(InputAction.CallbackContext obj) {
        OnViewMap?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerCrouch(InputAction.CallbackContext obj) {
        OnCrouchPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerRun(InputAction.CallbackContext obj) {
        OnRunPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerCarryEnemy(InputAction.CallbackContext obj) {
        OnCarryEnemyPerformed?.Invoke(this, EventArgs.Empty);
    }
}
