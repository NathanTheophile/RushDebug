using DG.Tweening;
using UnityEngine;

public class UI_PopupOnEnable : MonoBehaviour
{
    [SerializeField] private float _StartScale = 0.9f;
    [SerializeField] private float _PeakScale = 1.05f;
    [SerializeField] private float _Duration = 0.35f;

    private Tween _PopupTween;

    private void OnEnable()
    {
        _PopupTween.Kill();

        transform.localScale = Vector3.one * _StartScale;

        _PopupTween = DOTween.Sequence()
            .Append(transform.DOScale(_PeakScale, _Duration * 0.6f).SetEase(Ease.OutBack))
            .Append(transform.DOScale(1f, _Duration * 0.4f).SetEase(Ease.OutSine));
    }

    private void OnDisable()
    {
        _PopupTween.Kill();
        transform.localScale = Vector3.one;
    }
}