using UnityEngine;

public class BigNut_SpawnBirb : MonoBehaviour
{
    public GameObject birb;

    private float distanceFromCamera;

    // Start is called before the first frame update
    void Start()
    {
        distanceFromCamera = transform.position.x - Camera.main.transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        transform.position = new Vector3(Camera.main.transform.position.x + distanceFromCamera, transform.position.y, transform.position.z);
    }

    public void SpawnBirb(GameObject player, Transform shadow)
    {
        GameObject birbObject = Instantiate(birb, transform.position, Quaternion.identity);
        birbObject.GetComponent<BigNut_BirbScript>().Initialize(player, transform, shadow);
    }
}
