using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyDetectionSystem))]
public class DetectionEditor : Editor {
    private void OnSceneGUI() {
        EnemyDetectionSystem fov = (EnemyDetectionSystem)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.detectionRadius);

        Vector3 viewAngleLeft = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.detectionAngle / 2);
        Vector3 viewAngleRight = DirectionFromAngle(fov.transform.eulerAngles.y, fov.detectionAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleLeft * fov.detectionRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleRight * fov.detectionRadius);
    }

    private Vector3 DirectionFromAngle(float eulerY, float angle) {
        angle += eulerY;

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
