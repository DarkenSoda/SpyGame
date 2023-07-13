using UnityEngine;

public class StabRangeDetection : MonoBehaviour {
    private BackStab playerBackStab;
    private void OnTriggerEnter(Collider other) {
        playerBackStab = other.GetComponent<BackStab>();
        if (playerBackStab == null) return;
        if (playerBackStab.currentEnemy != null) return;

        playerBackStab.SetCurrentEnemy(transform.parent.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        playerBackStab = other.GetComponent<BackStab>();
        if (playerBackStab == null) return;

        if (playerBackStab.currentEnemy == transform.parent.gameObject) {
            playerBackStab.SetCurrentEnemy(null);
        }
    }

    private void OnDisable() {
        if (playerBackStab == null) return;

        if (playerBackStab.currentEnemy == transform.parent.gameObject) {
            playerBackStab.SetCurrentEnemy(null);
        }
    }
}
