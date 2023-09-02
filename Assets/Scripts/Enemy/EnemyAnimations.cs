using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour {
    private Animator anim;
    private Enemy enemy;

    private bool isWalking;
    public string[] deathAnimations;

    private void Start() {
        anim = GetComponent<Animator>();
        enemy = GetComponentInParent<Enemy>();
    }

    private void Update() {
        isWalking = enemy.IsWalking;

        anim.SetBool("IsWalking", isWalking);
    }

    public void DeathAnimation() {
        int i = UnityEngine.Random.Range(0, deathAnimations.Length);
        anim.SetTrigger(deathAnimations[i]);
    }

    public void CheckPointAnimation() {
        
    }

    public void AlertAnimation() {
        anim.SetBool("Alert", true);
        enemy.CanRotate = false;
    }

    public void EndAlert() {
        anim.SetBool("Alert", false);
        enemy.CanRotate = true;
    }
}
