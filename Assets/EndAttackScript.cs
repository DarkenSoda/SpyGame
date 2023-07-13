using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAttackScript : MonoBehaviour {
    public void EndAttack() {
        GetComponentInParent<PlayerAnimations>().EndAttack();
    }
}
