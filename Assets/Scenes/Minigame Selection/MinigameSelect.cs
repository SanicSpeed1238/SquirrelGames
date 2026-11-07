using UnityEngine;
using PL.Transitions;

public class MinigameSelect : MonoBehaviour
{
    public GameObject minigameSelectScreen;
    public TransitionEffect minigameTransition;
    public TransitionEffect backTransition;
    private readonly string playerSetupSceneName = "Player Setup";

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    public void LoadMinigame(string sceneName)
    {
        SceneTransitioner.Instance.LoadScene(sceneName, minigameTransition);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SceneTransitioner.Instance.LoadScene(playerSetupSceneName, backTransition);
        }
    }
}