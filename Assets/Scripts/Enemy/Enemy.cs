using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
    public static event EventHandler OnEnemyDeath;
    private NavMeshAgent agent;
    private EnemyAnimations animator;
    private EnemyDetectionSystem detection;
    public EnemyState enemyState { get; private set; }
    public EnemyState previousState { get; private set; }

    [Header("Speed")]
    [SerializeField] private float normalSpeed = 2f;
    [SerializeField] private float alertedSpeed = 4f;
    [SerializeField] private float aggressiveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private float stopRotation = 10f;
    [SerializeField] private float aggressiveSearchDistance = 5f;

    [Header("Cooldowns")]
    [SerializeField] private float waitCooldownMax;
    [SerializeField] private float alertCooldownMax;
    [SerializeField] private float aggressiveTimeMax;
    private float waitCooldown = 0f;
    private float alertCooldown = 0f;
    private float aggressiveTime;

    [Header("WayPoints")]
    public Transform pathHolder;
    private Transform[] pointList;
    private int currentPointIndex = 0;
    private bool isGoingBackwards = false;

    private Vector3 alertPosition;
    private Vector3 lastAlertPosition;
    private bool isRotating;
    public bool CanRotate { get; set; }
    public bool IsWalking { get; private set; }
    public bool IsAggressive { get; private set; }


    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<EnemyAnimations>();
        detection = GetComponent<EnemyDetectionSystem>();

        if (pathHolder != null) {
            pointList = new Transform[pathHolder.childCount];
            for (int i = 0; i < pathHolder.childCount; i++) {
                pointList[i] = pathHolder.GetChild(i).transform;
            }
        }

        aggressiveTime = aggressiveTimeMax;
        IsAggressive = false;
        SetEnemyState(EnemyState.Patrolling);
        waitCooldown = waitCooldownMax;
    }

    private void Update() {
        waitCooldown += Time.deltaTime;
        alertCooldown += Time.deltaTime;

        if (detection.CanSeePlayer) {
            Alert(Player.Instance.transform.position);
        }

        StateController();

        IsWalking = agent.velocity != Vector3.zero;
    }

    private void StateController() {
        switch (enemyState) {
            case EnemyState.Patrolling:
                agent.speed = normalSpeed;
                IsAggressive = false;
                aggressiveTime = aggressiveTimeMax;
                Patrol();
                break;
            case EnemyState.Alerted:
                if (previousState == EnemyState.Patrolling) {
                    agent.speed = alertedSpeed;
                }

                // if (previousState == EnemyState.Aggressive) {
                //     agent.speed = aggressiveSpeed;
                // }

                MoveToPoint(transform.position);

                if (!CanRotate) return;

                // Rotate to face player
                if (detection.CanSeePlayer) {
                    RotateAgent(Player.Instance.transform.position);
                } else {
                    RotateAgent(alertPosition);
                }

                // if Rotation finished, Go check alert
                if (!isRotating && !detection.CanSeePlayer && !detection.IsAlerted) {
                    alertCooldown = 0f;
                    SetEnemyState(EnemyState.CheckAlert);
                }
                break;
            case EnemyState.CheckAlert:
                MoveToAlertPosition();
                CheckPoint(lastAlertPosition);
                break;
            case EnemyState.Aggressive:
                agent.speed = aggressiveSpeed;
                aggressiveTime -= Time.deltaTime;
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    waitCooldown = 0f;
                    WaitAtPoint();
                    MoveToPoint(MoveToRandomPosition(transform.position, aggressiveSearchDistance));
                }
                if (aggressiveTime <= 0f) {
                    IsAggressive = false;
                    SetEnemyState(EnemyState.Patrolling);
                }
                break;
            case EnemyState.Dead:
                agent.isStopped = true;
                agent.ResetPath();
                break;
        }
    }

    private void Patrol() {
        if (pointList == null) return;
        if (waitCooldown < waitCooldownMax) return;

        MoveToPoint(pointList[currentPointIndex].position);

        if (!PointReached(pointList[currentPointIndex].position)) return;

        waitCooldown = 0f;

        if (currentPointIndex == 0) {
            isGoingBackwards = false;
        }
        if (currentPointIndex == pointList.Length - 1) {
            isGoingBackwards = true;
        }

        if (isGoingBackwards) {
            currentPointIndex--;
        } else {
            currentPointIndex++;
        }
    }

    private bool PointReached(Vector3 position) {
        Vector3 enemyPosition = new Vector3(transform.position.x, 0, transform.position.z);
        position.y = 0;
        float distance = Vector3.Distance(enemyPosition, position);
        if (distance <= stopDistance)
            return true;

        return false;
    }

    private void MoveToPoint(Vector3 position) {
        agent.SetDestination(position);
    }

    public void Alert(Vector3 position) {
        if (alertCooldown < alertCooldownMax) return;

        if (enemyState != EnemyState.Alerted) {
            SetEnemyState(EnemyState.Alerted);
            // play Alert Animation

            animator.AlertAnimation();
        }

        alertPosition = position;
    }

    private void RotateAgent(Vector3 position) {
        Vector3 dir = (position - transform.position);
        dir.y = 0f;
        dir = dir.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        if (Vector3.Angle(transform.forward, dir) < stopRotation) {
            agent.isStopped = false;
            isRotating = false;
            return;
        }

        agent.isStopped = true;
        isRotating = true;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    public void MoveToAlertPosition() {
        lastAlertPosition = alertPosition;
        MoveToPoint(alertPosition);
    }

    private void CheckPoint(Vector3 position) {
        if (PointReached(position)) {
            WaitAtPoint();
        } else {
            waitCooldown = 0f;
        }
    }

    private void WaitAtPoint() {
        if (waitCooldown < waitCooldownMax) {
            animator.CheckPointAnimation();
            return;
        }

        if (enemyState == EnemyState.CheckAlert) {
            if (IsAggressive) {
                SetEnemyState(EnemyState.Aggressive);
            } else {
                SetEnemyState(EnemyState.Patrolling);
            }
            alertCooldown = 0f;
        }
    }

    private Vector3 MoveToRandomPosition(Vector3 origin, float distance) {
        Vector3 randomDirection = origin + UnityEngine.Random.insideUnitSphere * distance;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

        return navHit.position;
    }

    public void Die() {
        SetEnemyState(EnemyState.Dead);
        animator.DeathAnimation();
        GetComponentInChildren<StabRangeDetection>().gameObject.SetActive(false);
        GetComponent<EnemyDetectionSystem>().meshFilter.gameObject.SetActive(false);
        GetComponent<CapsuleCollider>().isTrigger = true;


        OnEnemyDeath?.Invoke(this, EventArgs.Empty);
    }

    public void SetEnemyState(EnemyState state) {
        previousState = enemyState;
        enemyState = state;
    }

    public void SetSpeed(float speed) {
        agent.speed = speed;
    }

    private void OnDrawGizmos() {
        if (pathHolder == null) return;

        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
    }
}
