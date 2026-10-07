using UnityEngine;

public class BigNut_CameraScript : MonoBehaviour
{
    public float CameraMovementSpeed = 0.5f;

    private float acornXCoordinate;

    void Start()
    {
        acornXCoordinate = GameObject.FindWithTag("Acorn").transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        if (Mathf.Abs(acornXCoordinate - transform.position.x) > 10)
        {
            transform.position += Vector3.right * CameraMovementSpeed * Time.deltaTime;
        }
    }
}
