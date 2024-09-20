using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Important Components")]
    public GameObject treeObject;

    public void SpawnNewChunk()
    {
        Instantiate(treeObject, transform.position, Quaternion.identity);
    }
}