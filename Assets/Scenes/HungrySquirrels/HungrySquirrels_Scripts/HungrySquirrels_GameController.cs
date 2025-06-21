using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class HungrySquirrels_GameController : MonoBehaviour
{

    [SerializeField] private float _gameDur = 60;

    private bool _doOnce = false;

    public float _eTime { get; private set; }
    private float _rTime;

    public UnityEvent GameStart;
    public UnityEvent TimeUp;
    public UnityEvent LastRush;
    public UnityEvent IntensityUp;

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
        if(_doOnce == false)
        {
            GameStart?.Invoke();
            _doOnce = true;
        }
        _eTime += Time.deltaTime;
        _rTime += Time.deltaTime;
        if(_rTime >= 10)
        {
            IntensityUp?.Invoke();
            _rTime = 0;
        }
        if(_eTime >= 40)
        {
            LastRush?.Invoke();
        }
        if(_eTime >= _gameDur)
        {
            MG_GameManager.Instance.UpdateUITimer(0);
            TimeUp?.Invoke();//When time runs out
            MG_GameManager.Instance.EndGame();
        }
        else
        {
            MG_GameManager.Instance.UpdateUITimer(_gameDur - _eTime);
        }
    }
}
