using UnityEngine;
using EasyTransition;

public class CS_MainMenu : MonoBehaviour
{
    [Header("Play Button")]
    public TransitionSettings playTransition;
    private readonly string playerSetupScene = "Player Setup";

    public void LoadPlayerSetup()
    {
        TransitionManager.Instance().Transition(playerSetupScene, playTransition, 0f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}