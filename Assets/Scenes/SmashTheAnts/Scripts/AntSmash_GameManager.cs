using UnityEngine;

public class AntSmash_GameManager : MonoBehaviour
{
    public static AntSmash_GameManager Instance;

    [Header("Gameplay Attributes")]
    [Range(30f, 60f)]
    public float gameLength;

    // Variables Needed
    private float gameTime;

    private void Awake()
    {
        Instance = this;       
    }
    private void Start()
    {
        gameTime = gameLength;
    }

    private void Update()
    {
        if(CS_GameManager.Instance.gamePlaying) UpdateGameTime();
    }
    private void UpdateGameTime()
    {
        gameTime -= Time.deltaTime;

        if (Mathf.Round(gameTime) == 0)
        {
            gameTime = 0.0f;
            CS_GameManager.Instance.EndGame();
        }

        CS_GameManager.Instance.timeUI.text = gameTime.ToString("F0");
    }
}