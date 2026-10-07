using UnityEngine;
using EasyTransition;

public class CS_MinigameSelect : MonoBehaviour
{
    public TransitionSettings minigameTransition;
    public TransitionSettings backTransition;
    private readonly string playerSetupSceneName = "Player Setup";

    public void LoadMinigame(string sceneName)
    {
        TransitionManager.Instance().Transition(sceneName, minigameTransition, 0f);
    }

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            TransitionManager.Instance().Transition(playerSetupSceneName, backTransition, 0f);
        }
    }
}