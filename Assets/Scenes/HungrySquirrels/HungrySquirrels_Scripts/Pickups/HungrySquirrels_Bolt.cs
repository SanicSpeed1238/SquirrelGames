using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bolt : Pickup
{

    [SerializeField] private int _pointVal = 1;
    [SerializeField] private float _launchFCE = 10f;

    private Rigidbody _rb;
    private bool doOnce = false;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
        if (doOnce == true)
        {
            _rb.AddForce(transform.up * _launchFCE, ForceMode.Impulse);
            //Debug.Log("ACORN RELEASE");
            doOnce = false;
        }
    }

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {

        if (arm.mainBody.collectActive == true)
        {
            //Debug.Log(_pointVal);//Acorn is now within player's possession.
            arm.aOn += _pointVal;
            arm.mainBody.rebound = true;
            Destroy(this.gameObject);
        }

    }

    public void aInitialize()
    {
        doOnce = true;
    }

}
