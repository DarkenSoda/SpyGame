using System.Collections;
using System;
using UnityEngine;

public class BackStab : MonoBehaviour {
    public GameObject currentEnemy { get; private set; }

    [Header("Transfer enemy to Kill Position")]
    [SerializeField] private float duration = .1f;
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
        StartCoroutine(LerpPositionRotation(currentEnemy.transform));
        GetComponent<PlayerAnimations>().Attack();
        currentEnemy.GetComponent<Enemy>().Die();
        // Alert Nearby Enemies
    }

    private IEnumerator LerpPositionRotation(Transform currentEnemy) {
        float timeElapsed = 0;
        Vector3 startPosition = currentEnemy.position;
        Quaternion startRotation = currentEnemy.rotation;
        while (timeElapsed < duration) {
            timeElapsed += Time.deltaTime;
            currentEnemy.position = Vector3.Lerp(startPosition, killPosition.position, timeElapsed / duration);
            currentEnemy.rotation = Quaternion.Slerp(startRotation, transform.rotation, timeElapsed / duration);
            yield return null;
        }
        currentEnemy.position = killPosition.position;
        currentEnemy.rotation = transform.rotation;
    }

    public void SetCurrentEnemy(GameObject enemy) {
        currentEnemy = enemy;
    }
}
