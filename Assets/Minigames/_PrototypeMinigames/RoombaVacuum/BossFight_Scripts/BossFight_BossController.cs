using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight_BossController : MonoBehaviour
{

    public int bHealth { get; private set; }
    public float bSpeed { get; private set; }
    public float _fireRate = 2f;

    public GameObject[] _bulletSpawner;

    private Rigidbody _rb;
    private Collider _hb;
    private Coroutine __currRoutine;
    private bool _bAlive = true;
    //private bool _bStun = false;
    private bool _bAngy = false;

    private IEnumerator BulletAttack()
    {
        Vector3 _randDirection;

        while (_bAlive == true)
        {
            //Check to see if enraged
            if(_bAngy == true)
            {
                //Increase Speed & Fire Rate
            }
            else
            {
                //Will Keep moving
                yield return new WaitForSeconds(_fireRate);
                _randDirection = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                if (_randDirection != new Vector3(0, 0, 0))
                {
                    MoveBoss(_randDirection);
                }
                else
                {
                    yield break;
                }

                //Spawn Bullets at spawner locations
                //Rotate
            }
        }
        
    }

    private IEnumerator LaserAttack()
    {
        //Check to see if enraged
        //Freeze Position
        //Activate Laser Hitbox at spawner locations
        //Rotate
        yield break;
    }

    private IEnumerator StunRoutine()
    {
        //When all buttons are hit, stun for a duration
        yield break;
    }

    private IEnumerator RushAttack()
    {
        //
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _hb = GetComponent<Collider>();
        __currRoutine = StartCoroutine(BulletAttack());
    }

    // Update is called once per frame
    void Update()
    {
        //Pause when game is paused and then start the coroutine
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Wall":
                Debug.Log("Bounce on it");
                break;
            case "Player":
                Debug.Log("KILL");
                break;
        }

    }

    private void MoveBoss(Vector3 randDir)
    {
        Vector3 Movement = new Vector3(randDir.x, 0, randDir.z).normalized * bSpeed * Time.deltaTime * 60f;
        _rb.MovePosition(_rb.position + Movement);
    }
}
