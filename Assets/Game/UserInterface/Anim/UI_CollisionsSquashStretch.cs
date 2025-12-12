using DG.Tweening;
using UnityEngine;

namespace Rush.UI
{
    public class UI_CollisionSquashStretch : MonoBehaviour
    {
        [Header("Tween Settings")]
        [SerializeField, Min(0f)] private float _TweenDuration = 0.45f;
        [SerializeField, Min(1)] private int _LoopCount = 3;
        [SerializeField] private Vector3 _StretchScale = new Vector3(1.35f, 0.65f, 1.35f);
        [SerializeField] private Vector3 _SquashScale = new Vector3(0.8f, 1.2f, 0.8f);
        [SerializeField] private Ease _Ease = Ease.OutQuad;

        private Sequence _sequence;
        private Vector3 _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale;
            
            Vector3 stretchScale = Vector3.Scale(_baseScale, _StretchScale);
            Vector3 squashScale = Vector3.Scale(_baseScale, _SquashScale);

            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOScale(stretchScale, _TweenDuration * 0.4f).SetEase(_Ease));
            _sequence.Append(transform.DOScale(squashScale, _TweenDuration * 0.35f).SetEase(Ease.InOutSine));
            _sequence.Append(transform.DOScale(_baseScale, _TweenDuration * 0.25f).SetEase(Ease.OutBounce));
            _sequence.SetLoops(_LoopCount, LoopType.Restart);
            _sequence.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}