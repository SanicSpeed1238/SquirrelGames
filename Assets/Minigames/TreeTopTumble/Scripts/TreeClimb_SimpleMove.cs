using UnityEngine;

public class TreeClimb_SimpleMove : MonoBehaviour
{
    [SerializeField] private float destroyHeight = -100f;
    private float moveSpeed;

    private void Start()
    {
        moveSpeed = TreeClimb_GameManager.Instance.gameMoveSpeed;
    }

    private void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying || !TreeClimb_GameManager.Instance.treesMoving) 
            return;

        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);

        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}