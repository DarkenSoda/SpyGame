using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
    public EnemyState enemyState { get; private set; }

    private NavMeshAgent agent;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        enemyState = EnemyState.Alive;
    }

    public void SetEnemyState(EnemyState state) {
        enemyState = state;
    }
}
