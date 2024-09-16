using UnityEngine;

public class MoveDown : MonoBehaviour
{
    [Header("Movement Values")]
    public float moveSpeed;
    public float deadHeight;

    [Header("Variables only for Trees")]
    public bool isTree;
    
    // Other Variables Needed
    private bool newTreeSpawned;

    private void Update()
    {
        transform.Translate((moveSpeed * Time.deltaTime * -transform.up), Space.World);

        if(isTree)
        {
            if(transform.position.y <= 0.01f && !newTreeSpawned)
            {
                GameObject.FindAnyObjectByType<TreeSpawner>().SpawnNewChunk();
                newTreeSpawned = true;
            }
        }

        if(transform.position.y < deadHeight)
        {
            Destroy(gameObject);
        }
    }
}