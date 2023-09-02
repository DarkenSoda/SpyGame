using UnityEngine;

public class AlertMeter : MonoBehaviour {
    [SerializeField] private float alertMeterMax;
    private float alertMeter = 0f;

    [SerializeField] private float walkIncreaseValue;
    [SerializeField] private float runIncreaseValue;

    private void Start() {

    }

    public void IncreaseMeter(float distance) {
        if (alertMeter >= alertMeterMax) return;

        if (Player.Instance.playerState == PlayerState.Walking) {
            alertMeter += walkIncreaseValue * Time.deltaTime / distance;
        }

        if (Player.Instance.playerState == PlayerState.Running) {
            alertMeter += runIncreaseValue * Time.deltaTime / distance;
        }
    }
}
