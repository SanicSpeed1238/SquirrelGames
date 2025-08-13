using UnityEngine;

public class HungrySquirrels_Acorn : Pickup
{
    [SerializeField] private int _pointVal;
    [SerializeField] private float _launchFCE = 10f;
    [SerializeField] private float _rollSpeed;
    [SerializeField] private float _sChangeRate;

    private Rigidbody _rb;
    private bool doOnce = false;
    private Coroutine _randMovement;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(new Vector3(Random.Range(-5,5), 0, Random.Range(-5,5)), ForceMode.Impulse);
        if (doOnce == true)
        {
            _rb.AddForce(transform.up * _launchFCE, ForceMode.Impulse);
            //Debug.Log("ACORN RELEASE");
            doOnce = false;
        }
    }

    private void Update()
    {

        _rb.AddForce(new Vector3(Random.Range(0, _rollSpeed), Random.Range(0, _rollSpeed), Random.Range(0, _rollSpeed)), ForceMode.Force);
    }

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {

        if (arm.mainBody.collectActive == true)
        {
            //Debug.Log(_pointVal);//Acorn is now within player's possession.
            arm.aOn += _pointVal;
            arm.mainBody.rebound = true;
            arm.acornModel.SetActive(true);
            Destroy(this.gameObject);
        }
        
    }

    public void aInitialize()
    {
        doOnce = true;
    }

}
