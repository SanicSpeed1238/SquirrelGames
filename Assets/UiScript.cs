using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiScript : MonoBehaviour
{
    public int Score;
    [Range(1, 10)]
    public int AntScore; //how many points you get for squishing an Ant
    [Range(-10,10)]
    public int SquirrelScore; //how many points you lose for squishing a squirrel, it is positive and negative if you wanna squish squirrels sicko
    // Start is called before the first frame update
    [Range(0, 120)]
    public float Timer; // how long the game goes on before a winner is decided
    private bool GameOver = false;
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0 && !GameOver)
        {
            Debug.Log("Times up! Your score is: ");
            Debug.Log(Score);
            Time.timeScale = 0;
            GameOver = true;
        }
    }
    public void onSquirrelSquish()
    {
        Score += SquirrelScore;
        Debug.Log(Score);
    }
    public void onAntSquish()
    {
        Score += AntScore;
        Debug.Log(Score);
    }
}
