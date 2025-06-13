using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class HungrySquirrels_GameController : MonoBehaviour
{

    [SerializeField] private float _gameDur = 60;

    public float _eTime { get; private set; }

    public UnityEvent TimeUp;

    // Start is called before the first frame update
    void Start()
    {
        _eTime = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (!MG_GameManager.Instance.gamePlaying)
            return;

        _eTime += Time.deltaTime;
        if(_eTime >= _gameDur)
        {
            TimeUp?.Invoke();//When time runs out
        }
    }
}
