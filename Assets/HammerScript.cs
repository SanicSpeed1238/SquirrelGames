using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    [SerializeField] private Camera mainCamera;
    public Vector3 crosshair;
    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            crosshair = raycastHit.point;   // where the camera actually points to
            crosshair.x -= 2.5F;            //to realign the hammer with the actual crosshair
            crosshair.z += 6.5F;            //^^^^^
            crosshair.y = .5F;              //makes it so that the hammer doesnt change height if it hits a squirrel or ant
            transform.position = crosshair; //move the hammer to where the crosshair is
        }
    }
}
