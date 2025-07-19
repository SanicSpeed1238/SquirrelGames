using System.Collections;
using UnityEngine;

// This script modifies an object's scale to simulate a squish effect.

public class SquishModel : MonoBehaviour
{
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void SquishTemporarily(Vector3 squishScale, float squishDuration)
    {
        StartCoroutine(TemporarySquish(squishScale, squishDuration));
    }
    IEnumerator TemporarySquish(Vector3 squishScale, float squishDuration)
    {
        transform.localScale = squishScale;

        yield return new WaitForSeconds(squishDuration);

        LeanTween.scale(this.gameObject, originalScale, 1f).setEase(LeanTweenType.easeOutElastic);
    }

    public void SquishPermanently(Vector3 squishScale)
    {
        transform.localScale = squishScale;
    }
}