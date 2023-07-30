using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
    private NavMeshAgent agent;
    private EnemyAnimations animator;
    public EnemyState enemyState { get; private set; }

    [Header("Speed")]
    public float normalSpeed = 2f;
    public float alertedSpeed = 4f;
    public float aggressiveSpeed = 6f;

    [SerializeField] private float stopDistance = 0.8f;

    [Header("Cooldowns")]
    [SerializeField] private float waitCooldownMax;
    [SerializeField] private float alertCooldownMax;
    private float waitCooldown = 0f;
    private float alertCooldown = 0f;

    [Header("WayPoints")]
    public Transform pathHolder;
    private Transform[] pointList;
    private int currentPointIndex = 0;
    private bool isGoingBackwards = false;

    private Vector3 alertPosition;
    private Vector3 lastAlertPosition;
    public bool IsWalking { get; private set; }


    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<EnemyAnimations>();
        pointList = new Transform[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++) {
            pointList[i] = pathHolder.GetChild(i).transform;
        }
        enemyState = EnemyState.Patrolling;
        waitCooldown = waitCooldownMax;
    }

    private void Update() {
        waitCooldown += Time.deltaTime;
        alertCooldown += Time.deltaTime;

        switch (enemyState) {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Alerted:
                break;
            case EnemyState.CheckAlert:
                MoveToAlertPosition();
                CheckPoint(lastAlertPosition);
                break;
            case EnemyState.Aggressive:
                break;
            case EnemyState.Dead:
                agent.isStopped = true;
                break;
        }
        IsWalking = agent.velocity != Vector3.zero;
    }

    private void Patrol() {
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

        if (enemyState != EnemyState.Alerted && enemyState != EnemyState.CheckAlert) {
            agent.ResetPath();
            SetEnemyState(EnemyState.Alerted);
            // play Alert Animation

            animator.AlertAnimation();
        }
        alertPosition = position;
    }

    public void MoveToAlertPosition() {
        MoveToPoint(alertPosition);
        lastAlertPosition = alertPosition;
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

        if (enemyState == EnemyState.Alerted || enemyState == EnemyState.CheckAlert) {
            SetEnemyState(EnemyState.Patrolling);
            alertCooldown = 0f;
        }
    }

    public void Die() {
        SetEnemyState(EnemyState.Dead);
        animator.DeathAnimation();
        GetComponentInChildren<StabRangeDetection>().gameObject.SetActive(false);
        GetComponent<EnemyDetectionSystem>().meshFilter.gameObject.SetActive(false);
    }

    public void SetEnemyState(EnemyState state) {
        enemyState = state;
    }

    public void SetSpeed(float speed) {
        agent.speed = speed;
    }

    private void OnDrawGizmos() {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
    }
}
