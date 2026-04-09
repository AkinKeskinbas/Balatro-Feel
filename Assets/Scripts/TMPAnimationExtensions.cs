using DG.Tweening;
using TMPro;
using UnityEngine;

public static class TMPAnimationExtensions
{
    public static Tween DOCountInt(this TextMeshProUGUI text, int from, int to, float duration, string prefix = "", string suffix = "")
    {
        if (text == null)
            return null;

        int value = from;

        return DOTween.To(() => value, x =>
        {
            value = x;
            if (text != null)
                text.text = $"{prefix}{value}{suffix}";
        }, to, duration)
            .SetEase(Ease.OutQuad)
            .SetLink(text.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public static Tween DOPunchScaleText(this TextMeshProUGUI text, float scale = 1.15f, float duration = 0.18f)
    {
        if (text == null)
            return null;

        text.transform.DOKill(true);
        text.transform.localScale = Vector3.one;

        return text.transform
            .DOScale(scale, duration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutBack)
            .SetLink(text.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public static Tween DOFadeInText(this TextMeshProUGUI text, float duration = 0.15f)
    {
        if (text == null)
            return null;

        text.DOKill(true);
        text.alpha = 0f;
        return text.DOFade(1f, duration)
            .SetLink(text.gameObject, LinkBehaviour.KillOnDestroy);
    }

    public static Tween DOFadeOutText(this TextMeshProUGUI text, float duration = 0.15f)
    {
        if (text == null)
            return null;

        text.DOKill(true);
        return text.DOFade(0f, duration)
            .SetLink(text.gameObject, LinkBehaviour.KillOnDestroy);
    }
}
