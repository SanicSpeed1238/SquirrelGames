using UnityEngine;
using System.Collections;

public class AntSmash_ComputerPlayer : AntSmash_HammerController
{
    // Variables Needed

    private Vector3 targetPosition;

    private Coroutine actionCoroutine;
    private readonly float delayBetweenActionInput = 0.25f;

    private void Start()
    {
        SetMovementInput();
    }

    private void LateUpdate()
    {
        if (!AntSmash_GameManager.Instance.gamePlaying) 
            return;

        SetMovementInput();
        SetActionInput();
    }

    private void SetMovementInput()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // Set movementInput variable to match the direction of the targetPosition
            Vector3 direction = (targetPosition - transform.position).normalized;
            movementInput = new Vector2(direction.x, direction.z);
        }
        else
        {
            // Set a new targetPosition based on rangeX, rangeYTop, and rangeYBottom
            float newX = Random.Range(-rangeX, rangeX);
            float newY = Random.Range(rangeYBottom, rangeYTop);
            targetPosition = new Vector3(newX, transform.position.y, newY);
            movementInput = Vector2.zero;
        }
    }

    private void SetActionInput()
    {
        // Press the actionInput at random time intervals
        actionCoroutine ??= StartCoroutine(ActionInputCoroutine());
    }
    IEnumerator ActionInputCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, delayBetweenActionInput));
        actionPressed = true;

        yield return new WaitUntil(() => actionPressed == false);
        actionCoroutine = null;
    }
}