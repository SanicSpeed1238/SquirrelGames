using UnityEngine;

public class UIAnimator : MonoBehaviour
{
    public enum AppearAnimation
    {
        None,
        Grow,
        MoveIn,
    }

    [Header("Appear Animation")]
    public AppearAnimation appearAnimation = AppearAnimation.None;

    [Header("Grow Animation")]
    public float grow_TargetScale;
    public float grow_Duration;
    public float grow_Delay;
    public LeanTweenType grow_EaseType;

    [Header("MoveIn Animation")]
    public float moveIn_XOffset;
    public float moveIn_YOffset;
    public float moveIn_Duration;
    public float moveIn_Delay;
    public LeanTweenType moveIn_EaseType;

    public enum IdleAnimation
    {
        None,
        Floating,
        Pulsing,
    }

    [Header("Idle Animation")]
    public IdleAnimation idleAnimation = IdleAnimation.None;

    [Header("Floating Animation")]
    public float floating_PositionOffset;
    public float floating_Duration;
    public LeanTweenType floating_EaseType;

    [Header("Pulsing Animation")]
    public float pulsing_TargetGrowScale;
    public float pulsing_Duration;
    public LeanTweenType pulsing_EaseType;

    // Other Variables Needed
    private float idleDelay = 0.0f;

    #region Appear Animations

    private void Start()
    {
        switch(appearAnimation)
        {
            case AppearAnimation.None:
                StartIdleAnimation(); break;
            case AppearAnimation.Grow:
                GrowAnimation(); break;
            case AppearAnimation.MoveIn:
                MoveInAnimation(); break;
        }
    }

    private void GrowAnimation()
    {
        GetComponent<RectTransform>().localScale = Vector3.zero;
        Vector3 targetScale = new(grow_TargetScale, grow_TargetScale, grow_TargetScale);
        LeanTween.scale(GetComponent<RectTransform>(), targetScale, grow_Duration).setDelay(grow_Delay).setEase(grow_EaseType);
        idleDelay = grow_Delay + grow_Duration;
        StartIdleAnimation();
    }

    private void MoveInAnimation()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2
            (GetComponent<RectTransform>().anchoredPosition.x - moveIn_XOffset, GetComponent<RectTransform>().anchoredPosition.y - moveIn_YOffset);

        LeanTween.moveX(GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.x + moveIn_XOffset,
            moveIn_Duration).setDelay(moveIn_Delay).setEase(moveIn_EaseType);
        LeanTween.moveY(GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.y + moveIn_YOffset,
            moveIn_Duration).setDelay(moveIn_Delay).setEase(moveIn_EaseType);

        idleDelay = moveIn_Delay + moveIn_Duration;
        StartIdleAnimation();
    }

    #endregion

    #region Idle Animations

    private void StartIdleAnimation()
    {
        switch(idleAnimation)
        {
            case IdleAnimation.None:
                break;
            case IdleAnimation.Floating:
                FloatingAnimation(); break;
            case IdleAnimation.Pulsing:
                PulsingAnimation(); break;
        }
    }

    private void FloatingAnimation()
    {
        LeanTween.moveY(GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.y + floating_PositionOffset,
            floating_Duration/2).setDelay(idleDelay).setEase(floating_EaseType);

        LeanTween.moveY(GetComponent<RectTransform>(), GetComponent<RectTransform>().anchoredPosition.y - floating_PositionOffset,
            floating_Duration).setDelay(idleDelay + floating_Duration/2).setLoopPingPong().setEase(floating_EaseType);
    }

    private void PulsingAnimation()
    {
        Vector3 targetGrowScale = new(pulsing_TargetGrowScale, pulsing_TargetGrowScale, pulsing_TargetGrowScale);
        LeanTween.scale(GetComponent<RectTransform>(), targetGrowScale, pulsing_Duration)
            .setDelay(idleDelay).setLoopPingPong().setEase(pulsing_EaseType);
    }

    #endregion
}