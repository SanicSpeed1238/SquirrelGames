using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungrySquirrels_Bomb : Pickup
{

    [SerializeField] public float bTimer = 10;
    [SerializeField] private float _launchFCE = 10f;
    [SerializeField] public float _expPower = 20f;
    [SerializeField] public float _expRadius = 10f;
    [SerializeField] public float _expLiftPower = 10f;
    [SerializeField] private ParticleSystem _explostionFX;
    [SerializeField] private ParticleSystem _fuseFX;

    private Rigidbody _rb;
    private bool bOn = false;
    private bool doOnce = false;
    private float eTime;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), ForceMode.Impulse);
        if (doOnce == true)
        {
            _rb.AddForce(transform.up * _launchFCE, ForceMode.Impulse);
            doOnce = false;
        }
        if (bOn == true)
        {
            _fuseFX.Play();
        }
        
    }

    protected override void Collect(HungrySquirrels_ArmCollision arm)
    {
        if(arm.mainBody.collectActive == true)
        {
            arm.bActive = true;
            arm.mainBody.rebound = true;
            arm.bombModel.SetActive(true);
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (bOn == true)
        {
            eTime += Time.deltaTime;
            if(eTime >= bTimer)
            {
                _fuseFX.Stop();
                bombInteraction();
                Instantiate(_explostionFX, this.transform.position, Quaternion.identity);
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
