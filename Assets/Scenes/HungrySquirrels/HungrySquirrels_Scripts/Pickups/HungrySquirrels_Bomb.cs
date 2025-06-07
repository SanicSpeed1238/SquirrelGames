using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bomb : Pickup
{

    [SerializeField] public float bTimer = 10;
    [SerializeField] private float _launchFCE = 10f;
    [SerializeField] public float _expPower = 10f;
    [SerializeField] public float _expRadius = 10f;
    [SerializeField] public float _expLiftPower = 10f;
    [SerializeField] private LayerMask _objLayer;
    [SerializeField] private LayerMask _playerLayer;

    private Rigidbody _rb;
    private bool bOn = false;
    private bool doOnce = false;
    private float eTime;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (doOnce == true)
        {
            _rb.AddForce(transform.up * _launchFCE, ForceMode.Impulse);
            doOnce = false;
        }
        
    }

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {
        if(arm.mainBody.collectActive == true)
        {
            arm.bActive = true;
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (bOn == true)
        {
            eTime += Time.deltaTime;
            //Debug.Log("Bomb Time: " + eTime);
            if(eTime >= bTimer)
            {
                //Send out stun radius or destroy nuts?
                bombInteraction();
                //Debug.Log(transform.position);
                Destroy(this.gameObject);
            }
        }
    }

    public void bInitialize(float time)
    {
        bOn = true;
        doOnce = true;
        eTime = time;
    }

    public void bombInteraction()
    {

        Vector3 ePos = this.transform.position;

        Collider[] pickups = Physics.OverlapSphere(ePos, 10);

        //Collider[] players = Physics.OverlapSphere(ePos, 10, _playerLayer);

        foreach (Collider hit in pickups)
        {

            if(hit.tag == "Pickup" || hit.tag == "Hazard")
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if(rb != null)
                {
                    rb.AddExplosionForce( _expPower, ePos, _expRadius, _expLiftPower, ForceMode.Impulse);
                }

            }
            else if(hit.tag == "Player")
            {
                HungrySquirrels_ArmCollision arm = hit.GetComponent<HungrySquirrels_ArmCollision>();

                if(arm != null)
                {
                    arm.mainBody._stunActive = true;
                }
            }

            
        }
    }

}
