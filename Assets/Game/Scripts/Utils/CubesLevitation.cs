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

    [Header("Tornado")]
    [SerializeField] private float tornadoRotationDuration = 4f;
    [SerializeField] private float tornadoWaveAmplitude = 3f;
    [SerializeField] private float tornadoWaveFrequency = 1.5f;
    [SerializeField, Range(0f, 1f)] private float tornadoWaveAmplitudeJitter = 0.35f;

    private bool _HasSubscribedToGameEvents;
    private bool _HasTriggeredTornado;

    private void OnEnable()
    {
        SubscribeToGameEvents(true);
    }

    private void OnDisable()
    {
        SubscribeToGameEvents(false);
    }

    private void Start()
    {
        foreach (Transform child in transform)
        {
            StartCoroutine(StartLevitationWithRandomDelay(child));
        }

        SubscribeToGameEvents(true);
    }

    private void SubscribeToGameEvents(bool pSubscribe)
    {
        if (Manager_Game.Instance == null)
            return;

        if (pSubscribe)
        {
            if (_HasSubscribedToGameEvents)
                return;

            Manager_Game.Instance.onGameWon += TriggerTornadoAnimation;
            _HasSubscribedToGameEvents = true;
        }
        else
        {
            if (!_HasSubscribedToGameEvents)
                return;

            Manager_Game.Instance.onGameWon -= TriggerTornadoAnimation;
            _HasSubscribedToGameEvents = false;
        }
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

    private void TriggerTornadoAnimation()
    {
        if (_HasTriggeredTornado)
            return;

        _HasTriggeredTornado = true;

        foreach (Transform child in transform)
        {
            child.DOKill();
            StartTornadoTween(child);
        }
    }

    private void StartTornadoTween(Transform child)
    {
        Vector3 initialPosition = child.position;
        Vector2 flatDirection = new Vector2(initialPosition.x, initialPosition.z);
        float radius = flatDirection.magnitude;
        float baseAngleInDegrees = Mathf.Atan2(initialPosition.z, initialPosition.x) * Mathf.Rad2Deg;
        float baseY = initialPosition.y;
        float randomPhase = Random.Range(0f, Mathf.PI * 2f);
        float amplitudeMultiplier = Random.Range(1f - tornadoWaveAmplitudeJitter, 1f + tornadoWaveAmplitudeJitter);

        DOTween.To(() => 0f, angleOffsetDeg =>
        {
            float currentAngleDeg = baseAngleInDegrees + angleOffsetDeg;
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;
            float normalizedTurn = angleOffsetDeg / 360f;
            float yOffset = Mathf.Sin(normalizedTurn * Mathf.PI * 2f * tornadoWaveFrequency + randomPhase) * tornadoWaveAmplitude * amplitudeMultiplier;

            child.position = new Vector3(
                Mathf.Cos(currentAngleRad) * radius,
                baseY + yOffset,
                Mathf.Sin(currentAngleRad) * radius);
        }, 360f, tornadoRotationDuration)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }
}