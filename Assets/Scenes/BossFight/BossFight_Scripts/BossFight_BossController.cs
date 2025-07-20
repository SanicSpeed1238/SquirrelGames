using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFight_BossController : MonoBehaviour
{

    public int bHealth { get; private set; }
    public float bSpeed { get; private set; }

    public GameObject[] _bulletSpawner;
    private Coroutine __currRoutine;

    private IEnumerator BulletAttack()
    {
        //Check to see if enraged
        //Will Keep moving
        //Spawn Bullets at spawner locations
        //Rotate
        yield break;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
