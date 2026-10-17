using UnityEngine;

public class ANT_GameManager : MonoBehaviour
{
    public static ANT_GameManager Instance;

    [Header("Gameplay Attributes")]
    [Range(10f, 60f)]
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
        if(MG_GameManager.Instance.gamePlaying) UpdateGameTime();
    }
    private void UpdateGameTime()
    {
        gameTime -= Time.deltaTime;

        if (Mathf.Round(gameTime) == 0)
        {
            gameTime = 0.0f;
            MG_GameManager.Instance.EndGame();
        }

        MG_GameManager.Instance.UpdateUITimer(gameTime);
    }
}