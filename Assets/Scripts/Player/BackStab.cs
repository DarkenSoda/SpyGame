using System;
using UnityEngine;

public class BackStab : MonoBehaviour {
    public GameObject currentEnemy { get; private set; }
    [SerializeField] private Transform killPosition;


    [Header("Game Inputs")]
    [SerializeField] private GameInputs gameInputs;

    private void Start() {
        gameInputs.OnBackStabPerformed += OnBackStabPerformed;
    }

    private void OnDestroy() {
        gameInputs.OnBackStabPerformed -= OnBackStabPerformed;
    }

    private void OnBackStabPerformed(object sender, EventArgs e) {
        if (currentEnemy == null) return;
        if (GetComponent<PlayerAnimations>().isAttacking) return;

        // Perform Kill

        currentEnemy.transform.rotation = transform.rotation;
        currentEnemy.transform.position = killPosition.position;
        GetComponent<PlayerAnimations>().Attack();
        currentEnemy.GetComponent<Enemy>().Die();
        // Alert Nearby Enemies
    }

    public void SetCurrentEnemy(GameObject enemy) {
        currentEnemy = enemy;
    }
}
