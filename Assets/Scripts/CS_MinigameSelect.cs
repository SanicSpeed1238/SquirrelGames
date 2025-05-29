using UnityEngine;
using EasyTransition;

public class CS_MinigameSelect : MonoBehaviour
{
    public TransitionSettings minigameTransition;

    public void LoadMinigame(string sceneName)
    {
        TransitionManager.Instance().Transition(sceneName, minigameTransition, 0f);
    }

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    // TEMPORARY
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}