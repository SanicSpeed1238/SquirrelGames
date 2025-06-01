using UnityEngine;

public class HungrySquirrels_Acorn : Pickup
{
    [SerializeField] private int _pointVal = 1;
    [SerializeField] private float _launchFCE = 10f;

    private Rigidbody _rb;
    private bool doOnce = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {

        if (arm.mainBody.collectActive == true)
        {
            //Debug.Log(_pointVal);//Acorn is now within player's possession.
            arm.mainBody.rebound = true;
            arm.aOn = _pointVal;
            Destroy(this.gameObject);
        }
        
    }

    private void FixedUpdate()
    {

        if (doOnce == true)
        {
            _rb.AddForce(transform.up * _launchFCE, ForceMode.Impulse);
            Debug.Log("ACORN RELEASE");
            doOnce = false;
        }
    }

    //private void 

    public void aInitialize(bool oN)
    {
        doOnce = oN;
    }

}
