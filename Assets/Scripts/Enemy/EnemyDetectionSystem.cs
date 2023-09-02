using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionSystem : MonoBehaviour {
    [Header("Cone Vision")]
    public float detectionRadius;
    [Range(0, 360)]
    public float detectionAngle;
    [Range(0, 1)]
    [SerializeField] private float meshResolution;
    [Range(0, 20)]
    [SerializeField] private int edgeResolveIterations;
    [SerializeField] private float edgeDistanceThreshold;

    [Header("Walking Alert Range")]
    [SerializeField] private float walkingAlertRange;

    [Header("Running Alert Range")]
    [SerializeField] private float runningAlertRange;

    [Header("Death Alert Range")]
    [SerializeField] private float deathAlertRange;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask notObstructionLayers;

    [Header("Mesh")]
    public MeshFilter meshFilter;
    private Mesh mesh;

    private Enemy enemy;
    private AlertMeter alert;
    private List<Enemy> deadEnemyList = new List<Enemy>();
    public bool CanSeePlayer { get; private set; }
    public bool CanSeeDeadEnemy { get; private set; }
    public bool IsAlerted { get; private set; }

    private void Start() {
        enemy = GetComponent<Enemy>();
        alert = GetComponent<AlertMeter>();
        mesh = new Mesh();
        mesh.name = "Cone Mesh";
        meshFilter.mesh = mesh;

        Enemy.OnEnemyDeath += Enemy_OnDeath;
    }

    private void OnDisable() {
        Enemy.OnEnemyDeath -= Enemy_OnDeath;
    }

    private void Update() {
        if (enemy.enemyState == EnemyState.Dead) {
            this.enabled = false;
            return;
        }

        CheckAlert(walkingAlertRange, PlayerState.Walking);
        CheckAlert(runningAlertRange, PlayerState.Running);
        CanSeePlayer = ConeVision(Player.Instance.transform, enemyLayer);

        foreach (Enemy deadEnemy in deadEnemyList) {
            CanSeeDeadEnemy = ConeVision(deadEnemy.transform, playerLayer);
            if(CanSeeDeadEnemy) {
                Debug.Log(deadEnemy.transform.position);
                break;
            }
        }
    }

    private void LateUpdate() {
        DrawConeVision();
    }

    private void CheckAlert(float range, PlayerState state) {
        if (!Player.Instance.IsWalking || Player.Instance.playerState != state) {
            IsAlerted = false;
            return;
        }

        Vector3 playerPosition = Player.Instance.transform.position;
        float distanceFromPlayer = Vector3.Distance(transform.position, playerPosition);

        if (distanceFromPlayer <= range) {
            IsAlerted = true;
            enemy.Alert(playerPosition);
            alert.IncreaseMeter(distanceFromPlayer);
        } else { IsAlerted = false; }
    }

    private bool ConeVision(Transform target, LayerMask layer) {
        Vector3 targetPosition = target.position;
        float distanceFromPlayer = Vector3.Distance(transform.position, targetPosition);

        if (distanceFromPlayer <= detectionRadius) {
            Vector3 direction = (targetPosition - transform.position).normalized;

            // if Outside ConeVision Angle
            if (Vector3.Angle(transform.forward, direction) > detectionAngle / 2) {
                return false;
            }

            // Check if player is not Hidden
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, detectionRadius, ~layer)) {
                if (hit.transform.gameObject != target.gameObject) {
                    return false;
                }

                // Kill Player
                return true;
            }
        }

        return false;
    }

    private void Enemy_OnDeath(object sender, EventArgs e) {
        deadEnemyList.Add(sender as Enemy);
    }

    private void DrawConeVision() {
        mesh.Clear();

        int stepCount = (int)(detectionAngle * meshResolution);
        float stepSize = detectionAngle / stepCount;

        List<Vector3> points = new List<Vector3>();
        CastInfo oldCast = new CastInfo();
        for (int i = 0; i <= stepCount; i++) {
            float angle = transform.eulerAngles.y - detectionAngle / 2 + stepSize * i;
            CastInfo castInfo = ViewCast(angle);

            if (i > 0) {
                bool isEdgeThresholdExceeded = Mathf.Abs(oldCast.distance - castInfo.distance) > edgeDistanceThreshold;
                if (oldCast.hit != castInfo.hit || (oldCast.hit && castInfo.hit && isEdgeThresholdExceeded)) {
                    EdgeInfo edge = FindEdge(oldCast, castInfo);

                    if (edge.pointA != Vector3.zero) {
                        points.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        points.Add(edge.pointB);
                    }
                }
            }

            points.Add(castInfo.point);
            oldCast = castInfo;
        }

        int vertexCount = points.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(points[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private CastInfo ViewCast(float angle) {
        Vector3 dir = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, detectionRadius, ~notObstructionLayers)) {
            return new CastInfo(true, hit.point, hit.distance, angle);
        }

        return new CastInfo(false, transform.position + dir * detectionRadius, detectionRadius, angle);
    }

    private EdgeInfo FindEdge(CastInfo minCast, CastInfo maxCast) {
        float minAngle = minCast.angle, maxAngle = maxCast.angle;
        Vector3 minPoint = Vector3.zero, maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++) {
            float middleAngle = (minAngle + maxAngle) / 2;
            CastInfo newCast = ViewCast(middleAngle);

            bool isEdgeThresholdExceeded = Mathf.Abs(minCast.distance - newCast.distance) > edgeDistanceThreshold;
            if (newCast.hit == minCast.hit && !isEdgeThresholdExceeded) {
                minAngle = middleAngle;
                minPoint = newCast.point;
            } else {
                maxAngle = middleAngle;
                maxPoint = newCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private struct CastInfo {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public CastInfo(bool _hit, Vector3 _point, float _distance, float _angle) {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    private struct EdgeInfo {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
