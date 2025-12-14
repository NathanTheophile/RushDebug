using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ChildrenLevitation : MonoBehaviour
{
    [SerializeField] private float levitationPower = 0.5f;
    [SerializeField] private float levitationSpeed = 1f;
    [SerializeField] private float maxStartDelay = 1f;
    [SerializeField] private Ease levitationEase = Ease.OutBack;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            StartCoroutine(StartLevitationWithRandomDelay(child));
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
}
