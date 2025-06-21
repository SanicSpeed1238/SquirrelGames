using UnityEngine;

public class BigNut_CameraScript : MonoBehaviour
{
    public float CameraMovementSpeed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
        {
            return;
        }

        transform.position += Vector3.right * CameraMovementSpeed * Time.deltaTime;
    }
}
