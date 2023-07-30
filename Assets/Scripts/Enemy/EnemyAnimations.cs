using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour {
    private Animator anim;

    private bool isWalking;
    public string[] deathAnimations;

    private void Start() {
        anim = GetComponent<Animator>();
    }

    private void Update() {
        isWalking = GetComponentInParent<Enemy>().IsWalking;

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
    }

    public void EndAlert() {
        anim.SetBool("Alert", false);
        GetComponentInParent<Enemy>().MoveToAlertPosition();
    }
}
