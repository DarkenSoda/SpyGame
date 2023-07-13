using UnityEngine;

public class StabRangeDetection : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        BackStab playerBackStab = other.GetComponent<BackStab>();
        if (playerBackStab == null) return;
        if (playerBackStab.currentEnemy != null) return;

        playerBackStab.SetCurrentEnemy(transform.parent.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        BackStab playerBackStab = other.GetComponent<BackStab>();
        if (playerBackStab == null) return;

        if (playerBackStab.currentEnemy == transform.parent.gameObject) {
            playerBackStab.SetCurrentEnemy(null);
        }
    }
}
