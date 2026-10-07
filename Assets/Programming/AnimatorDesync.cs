using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorDesync : MonoBehaviour
{
    IEnumerator Start()
    {
        Animator animator = GetComponent<Animator>();

        animator.speed = 0f;

        yield return new WaitForSeconds(Random.Range(0.1f, 1.0f));

        animator.speed = 1f;
    }
}