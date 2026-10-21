using UnityEngine;
using PL.Transitions;

public class TransitionManager : MonoBehaviour
{
    public GameObject transitionManager;

    [Header("Transition Effects")]
    public TransitionEffect normalTransition;
    public TransitionEffect exitTransition;

    // Other Variables
    [HideInInspector] public float midpoint;

    private void Start()
    {
        Instantiate(transitionManager);
        midpoint = (normalTransition.Duration/2f) + (normalTransition.MiddleDelay/2f);
    }

    public void PlayTransition()
    {
        SceneTransitioner.Instance.PlayTransition(()=> DebugTransition(), normalTransition);
    }
    private void DebugTransition()
    {
        Debug.Log("Transition at Midpoint.");
    }

    public void ExitMinigame()
    {
        SceneTransitioner.Instance.LoadScene("Minigame Selection", exitTransition);
    }
}