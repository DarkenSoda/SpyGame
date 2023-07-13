using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
    private NavMeshAgent agent;
    public EnemyState enemyState { get; private set; }

    private float waitCooldown = 0f;
    [SerializeField] private float waitCooldownMax;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("WayPoints")]
    public Transform[] pointList;
    private int currentPointIndex = 0;
    private bool isGoingBackwards = false;
    public bool IsWalking { get; private set; }


    private void Start() {
        agent = GetComponent<NavMeshAgent>();
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

        MoveToPoint(pointList[currentPointIndex]);

        if (!PointReached(pointList[currentPointIndex])) return;

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

    private bool PointReached(Transform point) {
        float distance = Vector3.Distance(transform.position, point.position);
        if (distance <= stopDistance)
            return true;

        return false;
    }

    private void MoveToPoint(Transform point) {
        IsWalking = true;
        agent.SetDestination(point.position);
    }

    public void Die() {
        SetEnemyState(EnemyState.Dead);
        GetComponent<EnemyAnimations>().DeathAnimation();
        GetComponentInChildren<StabRangeDetection>().gameObject.SetActive(false);
    }

    public void SetEnemyState(EnemyState state) {
        enemyState = state;
    }
}
