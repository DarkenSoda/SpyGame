using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
    private NavMeshAgent agent;
    public EnemyState enemyState { get; private set; }

    public float normalSpeed = 2f;
    public float alertedSpeed = 4f;
    public float aggressiveSpeed = 6f;

    private float waitCooldown = 0f;
    [SerializeField] private float waitCooldownMax;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("WayPoints")]
    public Transform pathHolder;
    private Transform[] pointList;
    private int currentPointIndex = 0;
    private bool isGoingBackwards = false;

    private Vector3 alertPosition;
    public bool IsWalking { get; private set; }


    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        pointList = new Transform[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++) {
            pointList[i] = pathHolder.GetChild(i).transform;
        }
        enemyState = EnemyState.Patrolling;
        waitCooldown = waitCooldownMax;
    }

    private void Update() {
        waitCooldown += Time.deltaTime;

        switch (enemyState) {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Alerted:
                CheckPoint(alertPosition);
                break;
            case EnemyState.Aggressive:
                break;
            case EnemyState.Dead:
                agent.isStopped = true;
                IsWalking = false;
                break;
        }
    }

    private void FixedUpdate() {
        agent.isStopped = !IsWalking;
    }

    private void Patrol() {
        if (waitCooldown < waitCooldownMax) return;

        MoveToPoint(pointList[currentPointIndex].position);

        if (!PointReached(pointList[currentPointIndex].position)) return;

        IsWalking = false;
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
        IsWalking = true;
        agent.SetDestination(position);
    }

    public void Alert(Vector3 position) {
        SetEnemyState(EnemyState.Alerted);
        alertPosition = position;
        MoveToPoint(position);
    }

    private void CheckPoint(Vector3 position) {
        if (PointReached(position)) {
            IsWalking = false;
            WaitAtPoint();
        } else {
            waitCooldown = 0f;
        }
    }

    private void WaitAtPoint() {
        if (waitCooldown < waitCooldownMax) {
            GetComponent<EnemyAnimations>().CheckPointAnimation();
            return;
        }

        if (enemyState == EnemyState.Alerted) {
            SetEnemyState(EnemyState.Patrolling);
        }
    }

    public void Die() {
        SetEnemyState(EnemyState.Dead);
        GetComponent<EnemyAnimations>().DeathAnimation();
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
