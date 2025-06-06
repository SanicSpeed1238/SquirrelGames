using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bomb : Pickup
{

    [SerializeField] public float bTimer = 10;
    [SerializeField] private float _launchFCE = 10f;
    [SerializeField] private LayerMask _objLayer;

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
                Vector3 ePos = this.transform.position;
                Debug.Log(transform.position);
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

}
