using UnityEngine;
using System.Collections;

public class AntSmash_ComputerPlayer : AntSmash_HammerController
{
    // Variables Needed
    Vector3 targetPosition;
    Coroutine actionCoroutine;

    private void Start()
    {
        SetMovementInput();
        SetActionInput();
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
        if(actionCoroutine == null)
        {
            actionCoroutine = StartCoroutine(ActionInputCoroutine());
        }
    }
    IEnumerator ActionInputCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));
        actionPressed = true;

        yield return new WaitUntil(() => actionPressed == false);
        actionCoroutine = null;
    }
}