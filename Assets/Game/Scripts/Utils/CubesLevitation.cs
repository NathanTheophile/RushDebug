using UnityEngine;
using DG.Tweening;
using System.Collections;
using Rush.Game;

public class ChildrenLevitation : MonoBehaviour
{
    [SerializeField] private float levitationPower = 0.5f;
    [SerializeField] private float levitationSpeed = 1f;
    [SerializeField] private float maxStartDelay = 1f;
    [SerializeField] private Ease levitationEase = Ease.OutBack;

    [Header("Win Ascension")]
    [SerializeField] private float ascensionHeight = 30f;
    [SerializeField] private float ascensionDuration = 3f;
    [SerializeField] private Ease ascensionEase = Ease.OutCubic;
    private Coroutine _SubscribeRoutine;
    private bool _HasSubscribedToGameEvents;
    private bool _HasTriggeredAscension;

    private void OnEnable()
    {
        EnsureSubscribed();    }

    private void OnDisable()
    {
        StopSubscribeRoutine();
        UnsubscribeFromGameEvents();    }

    private void Start()
    {
        foreach (Transform child in transform)
        {
            StartCoroutine(StartLevitationWithRandomDelay(child));
        }

        EnsureSubscribed();    }
    private void EnsureSubscribed()
    {
        if (_HasSubscribedToGameEvents || _SubscribeRoutine != null)
            return;

        _SubscribeRoutine = StartCoroutine(SubscribeWhenManagerReady());
    }

        private void StopSubscribeRoutine()
    {
        if (_SubscribeRoutine == null)
            return;

        StopCoroutine(_SubscribeRoutine);
        _SubscribeRoutine = null;
    }

    private IEnumerator SubscribeWhenManagerReady()
    {
        while (Manager_Game.Instance == null)
            yield return null;

        Manager_Game.Instance.onGameWonSequenceStarted += TriggerAscensionAnimation;
        _HasSubscribedToGameEvents = true;
        _SubscribeRoutine = null;
    }

    private void UnsubscribeFromGameEvents()
    {
        if (!_HasSubscribedToGameEvents || Manager_Game.Instance == null)
            return;

        Manager_Game.Instance.onGameWonSequenceStarted -= TriggerAscensionAnimation;
        _HasSubscribedToGameEvents = false;
    }

    private IEnumerator StartLevitationWithRandomDelay(Transform child)
    {
        float delay = Random.Range(0f, maxStartDelay);
        yield return new WaitForSeconds(delay);

        Vector3 initialPosition = child.position;
        float targetY = initialPosition.y - levitationPower;

        child.DOMoveY(targetY, levitationSpeed)
            .SetSpeedBased(true)
            .SetEase(levitationEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void TriggerAscensionAnimation()
    {
        if (_HasTriggeredAscension)
            return;

        _HasTriggeredAscension = true;

        foreach (Transform child in transform)
        {
            child.DOKill();
            StartCoroutine(StartAscensionAfterDelay(child));
        }
    }

    private IEnumerator StartAscensionAfterDelay(Transform child)
    {
        float delay = Random.Range(0f, 2f);
        yield return new WaitForSeconds(delay);

        Vector3 targetPosition = child.position + Vector3.up * ascensionHeight;
        child.DOMove(targetPosition, ascensionDuration)
            .SetEase(ascensionEase);
    }
}