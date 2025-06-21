using UnityEngine;

public class BigNut_SpawnBirb : MonoBehaviour
{
    public GameObject birb;

    private float movementSpeed;

    // Start is called before the first frame update
    void Start()
    {
        movementSpeed = Camera.main.GetComponent<BigNut_CameraScript>().CameraMovementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        transform.position += Vector3.right * movementSpeed * Time.deltaTime;
    }

    public void SpawnBirb(GameObject player)
    {
        GameObject birbObject = Instantiate(birb, transform.position, Quaternion.identity);
        birbObject.GetComponent<BigNut_BirbScript>().Initialize(player, transform);
    }
}
