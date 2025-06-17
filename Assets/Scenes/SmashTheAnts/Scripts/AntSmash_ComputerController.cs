using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AntSmash_HammerController))]
public class AntSmash_ComputerController : MonoBehaviour
{
    // Variables Needed
    private AntSmash_HammerController controller;

    // Variables for Left Stick input
    private Vector3 targetPosition;
    private readonly float distanceBuffer = 0.25f;

    // Variables for 'A' Button input
    private Coroutine actionCoroutine;
    private readonly float delayBetweenActionInput = 0.25f;

    private void Awake()
    {
        controller = GetComponent<AntSmash_HammerController>();
    }

    private void Start()
    {
        SetMovementInput();
    }

    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying) 
            return;

        SetMovementInput();
        SetActionInput();
    }

    private void SetMovementInput()
    {
        Vector3 toTarget = targetPosition - transform.position;

        if (toTarget.sqrMagnitude > distanceBuffer)
        {
            Vector3 direction = toTarget.normalized;
            Vector2 leftStickInput = new(direction.x, direction.z);
            controller.SetLeftStickInput(leftStickInput);
        }
        else
        {
            float newX = Random.Range(-controller.rangeX, controller.rangeX);
            float newY = Random.Range(-controller.rangeYTop, controller.rangeYTop);
            targetPosition = new Vector3(newX, transform.position.y, newY);
            controller.SetLeftStickInput(Vector2.zero);
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
        controller.SetActionInput(true);

        yield return new WaitUntil(() => controller.GetActionInputState() == false);
        actionCoroutine = null;
    }
}