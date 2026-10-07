using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bolt : Pickup
{

    //[SerializeField] private int _pointVal;
    [SerializeField] private float _launchFCE = 10f;
    [SerializeField] private float _rollSpeedMax;
    [SerializeField] private float _sChangeRate;

    private Rigidbody _rb;
    private bool doOnce = false;
    private float _moveChange;
    private Coroutine _randMovement;

    private IEnumerator RandMovementRoutine()
    {
        while(gameObject != null)
        {
            _moveChange = Random.Range(0, _sChangeRate);
            yield return new WaitForSeconds(_moveChange);
            _rb.AddForce(new Vector3(Random.Range(-_rollSpeedMax, _rollSpeedMax), 0, Random.Range(-_rollSpeedMax, _rollSpeedMax)), ForceMode.Impulse);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _randMovement = StartCoroutine(RandMovementRoutine());
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
            //arm.aOn += _pointVal;
            arm.boltOn = true;
            arm.mainBody.rebound = true;
            arm.screwModel.SetActive(true);
            Destroy(this.gameObject);
        }

    }

    public void aInitialize()
    {
        doOnce = true;
    }

}
