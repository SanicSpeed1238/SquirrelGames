using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HammerModelScript : MonoBehaviour
{
    public Animator anim;
    public ParticleSystem smash;
    private int timer = 0;         
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        smash.Pause();
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && timer == 0)
        {
            Squish();
            timer = 60;         // since game runs at 60 fps this should hopefully make you wait a second
        }
        else if (timer > 0)
        {
            timer--;
        }
    }
    void Squish()
    {
        anim.Play("HammerSmash");
        smash.Play();
        
    }
}
