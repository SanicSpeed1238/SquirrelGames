using UnityEngine;

public class CS_LimitFPS : MonoBehaviour
{
    [Range(60, 120)]
    public int framerateCap = 60;

    private void Start()
    {
        Application.targetFrameRate = framerateCap;
    }
}