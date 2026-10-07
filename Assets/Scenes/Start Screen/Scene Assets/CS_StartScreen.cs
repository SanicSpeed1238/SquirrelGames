using UnityEngine;
using EasyTransition;

public class CS_StartScreen : MonoBehaviour
{
    [Header("'Press Start' Components")]
    public TransitionSettings pressStartTransition;
    private readonly string mainMenuScene = "Main Menu";

    private void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            TransitionManager.Instance().Transition(mainMenuScene, pressStartTransition, 0f);
        }
    }
}