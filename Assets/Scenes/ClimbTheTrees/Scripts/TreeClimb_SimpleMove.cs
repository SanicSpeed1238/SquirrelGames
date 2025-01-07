using UnityEngine;

public class TreeClimb_SimpleMove : MonoBehaviour
{
    private float moveSpeed;
    private readonly float destroyHeight = -100f;

    private void Start()
    {
        moveSpeed = GameObject.FindAnyObjectByType<TreeClimb_TreesScript>().moveSpeed;
    }

    private void Update()
    {
        if (!TreeClimb_GameManager.Instance.gamePlaying || !TreeClimb_GameManager.Instance.treesMoving) 
            return;

        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);

        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}