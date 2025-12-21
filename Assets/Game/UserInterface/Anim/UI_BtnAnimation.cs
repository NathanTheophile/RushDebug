using DG.Tweening;
using Rush.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BtnAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover visuals")]
    [SerializeField] private Image _LeftHover;
    [SerializeField] private Image _RightHover;

    [Header("Scale tween")]
    [SerializeField] private float _HoverScale = 1.15f;
    [SerializeField] private float _OriginalScale = 1f;
    [SerializeField] private float _ScaleDuration = 0.15f;
    [SerializeField] private Ease _ScaleEase = Ease.OutQuad;

    [Header("Logo rotation")]
    [SerializeField] private float _LogoRotation = 30f;
    [SerializeField] private float _LogoRotationDuration = 1.2f;
    [SerializeField] private float _LogoRotationDelay = 1.0f;
    [SerializeField] private float _LogoScale = 1.3f;
    [Header("Click tween")]
    [SerializeField] private float _ClickScale = 0.92f;
    [SerializeField] private float _ClickDuration = 0.08f;
    [SerializeField] private Ease _ClickEase = Ease.OutQuad;
        [Header("Audio")]
    [SerializeField] private AudioClip _ClickSound;
    [SerializeField, Range(0f, 1f)] private float _ClickVolume = 1f;
    [SerializeField] private string _MixerGroup = "Interface";
    private Tween _scaleTween;
    private Tween _leftLogoTween;
    private Tween _rightLogoTween;

    private void OnDisable()
    {
        ResetState();
    }

    private void ResetState()
    {
        _scaleTween.Kill();
        _leftLogoTween.Kill();
        _rightLogoTween.Kill();

        transform.localScale = Vector3.one;

        if (_LeftHover != null)
        {
            _LeftHover.gameObject.SetActive(false);
            _LeftHover.rectTransform.localRotation = Quaternion.identity;
            _LeftHover.rectTransform.localScale = Vector3.one;
        }

        if (_RightHover != null)
        {
            _RightHover.gameObject.SetActive(false);
            _RightHover.rectTransform.localRotation = Quaternion.identity;
            _RightHover.rectTransform.localScale = Vector3.one;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _scaleTween.Kill();
        _scaleTween = transform.DOScale(_HoverScale, _ScaleDuration).SetEase(_ScaleEase);

        if (_LeftHover != null) _LeftHover.gameObject.SetActive(true);
        if (_RightHover != null) _RightHover.gameObject.SetActive(true);

        _leftLogoTween.Kill();
        _rightLogoTween.Kill();

        if (_LeftHover != null)
        {
            _LeftHover.rectTransform.localRotation = Quaternion.identity;
            _LeftHover.rectTransform.localScale = Vector3.one;
            _leftLogoTween = DOTween.Sequence()
                .Append(_LeftHover.rectTransform
                    .DORotate(new Vector3(0f, 0f, _LogoRotation), _LogoRotationDuration)
                    .SetEase(Ease.InOutBack))
                .Join(_LeftHover.rectTransform
                    .DOScale(_LogoScale, _LogoRotationDuration)
                    .SetEase(Ease.OutExpo))
                .AppendInterval(_LogoRotationDelay)
                .SetLoops(-1, LoopType.Yoyo);
        }

        if (_RightHover != null)
        {
            _RightHover.rectTransform.localRotation = Quaternion.identity;
            _RightHover.rectTransform.localScale = Vector3.one;
            _rightLogoTween = DOTween.Sequence()
                .Append(_RightHover.rectTransform
                    .DORotate(new Vector3(0f, 0f, -_LogoRotation), _LogoRotationDuration)
                    .SetEase(Ease.InOutBack))
                .Join(_RightHover.rectTransform
                    .DOScale(_LogoScale, _LogoRotationDuration)
                    .SetEase(Ease.OutExpo))
                .AppendInterval(_LogoRotationDelay)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        _scaleTween.Kill();
        _scaleTween = DOTween.Sequence()
            .Append(transform.DOScale(_ClickScale, _ClickDuration).SetEase(_ClickEase))
            .Append(transform.DOScale(1f, _ClickDuration).SetEase(_ClickEase));

                    if (_ClickSound != null && Manager_Audio.Instance != null)
            Manager_Audio.Instance.PlayOneShot(_ClickSound, pVolume: _ClickVolume, pMixerGroup: _MixerGroup);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _leftLogoTween.Kill();
        _rightLogoTween.Kill();

        _scaleTween.Kill();
        _scaleTween = transform.DOScale(_OriginalScale, _ScaleDuration).SetEase(_ScaleEase);

        if (_LeftHover != null)
        {
            _LeftHover.rectTransform.localRotation = Quaternion.identity;
            _LeftHover.rectTransform.localScale = Vector3.one;
            _LeftHover.gameObject.SetActive(false);
        }

        if (_RightHover != null)
        {
            _RightHover.rectTransform.localRotation = Quaternion.identity;
            _RightHover.rectTransform.localScale = Vector3.one;
            _RightHover.gameObject.SetActive(false);
        }
    }
}