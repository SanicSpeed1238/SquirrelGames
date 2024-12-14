using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    public int framerateCap = 60;

    private void Start()
    {
        Application.targetFrameRate = framerateCap;
    }
}