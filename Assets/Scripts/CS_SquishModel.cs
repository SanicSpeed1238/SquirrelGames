using System.Collections;
using UnityEngine;

// Attach this script to the GameObject that will have its scale modified.

public class CS_SquishModel : MonoBehaviour
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

        LeanTween.scale(this.gameObject, originalScale, .5f).setEase(LeanTweenType.easeOutElastic);
    }

    public void SquishPermanently(Vector3 squishScale)
    {
        transform.localScale = squishScale;
    }
}