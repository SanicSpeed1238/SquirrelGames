using System.Collections;
using UnityEngine;

public class BushShake : MonoBehaviour
{
    private Material bushLeaves;
    private Coroutine currentShake;

    private bool isShaking;
    private float idleTimer;

    [HideInInspector] public float idleStrength;
    [HideInInspector] public float idleSpeed;

    private void Start()
    {
        bushLeaves = GetComponentInChildren<Renderer>().material;
        idleStrength = bushLeaves.GetFloat("_Shake_Strength");
        idleSpeed = bushLeaves.GetFloat("_Shake_Speed");
        idleTimer = 0f;
    }
    private void Update()
    {
        if (!isShaking)
        {
            idleTimer += Time.deltaTime;
            bushLeaves.SetFloat("_Idle_Time", idleTimer);
        }
    }

    public void ShakeBush(float strength, float speed, float duration)
    {
        currentShake ??= StartCoroutine(ShakeAnimation(strength, speed, duration));
    }
    IEnumerator ShakeAnimation(float shakeStrength, float shakeSpeed, float duration)
    {
        isShaking = true;
        bushLeaves.SetFloat("_Auto_Time", 0f);

        float elapsedTime = 0f;
        float shakeTimer = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            shakeTimer += Time.deltaTime * Mathf.Lerp(shakeSpeed, idleSpeed, t);
            bushLeaves.SetFloat("_Shake_Time", shakeTimer);

            bushLeaves.SetFloat("_Shake_Strength", Mathf.Lerp(shakeStrength, idleStrength, t));
            bushLeaves.SetFloat("_Shake_Speed", Mathf.Lerp(shakeSpeed, idleSpeed, t));

            yield return null;
        }

        bushLeaves.SetFloat("_Auto_Time", 1f);
        idleTimer = shakeTimer;

        isShaking = false;
        currentShake = null;
    }
}