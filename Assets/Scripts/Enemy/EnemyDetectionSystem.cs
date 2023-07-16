using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionSystem : MonoBehaviour {
    [Header("Cone Vision")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float detectionRadius;

    [Header("Walking Alert Range")]
    [SerializeField] private float walkingAlertRange;

    [Header("Running Alert Range")]
    [SerializeField] private float runningAlertRange;

    [Header("Death Alert Range")]
    [SerializeField] private float deathAlertRange;

    private Enemy enemy;

    private void Start() {
        enemy = GetComponent<Enemy>();
    }

    private void FixedUpdate() {
        CheckAlert(walkingAlertRange);
        CheckAlert(runningAlertRange);
    }

    private void CheckAlert(float range) {
        if (Physics.SphereCast(transform.position, range, Vector3.zero, out RaycastHit hit)) {
            PlayerMovement player = hit.transform.gameObject.GetComponent<PlayerMovement>();
            if (player == null) return;

            enemy.Alert(player.transform.position);
        }
    }
}
