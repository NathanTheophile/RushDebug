using DG.Tweening;
using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private Renderer _Renderer;

    private Tween _EmissionTween;

    private void OnEnable()
    {
        if (_Renderer == null)
            _Renderer = GetComponent<Renderer>();

        if (_Renderer == null)
            return;

        Material[] lMaterials = _Renderer.materials;

        if (lMaterials.Length < 2)
            return;

        Material lEmissionMaterial = lMaterials[1];
        Color lBaseEmission = lEmissionMaterial.GetColor("_EmissionColor");

        lBaseEmission = lBaseEmission.maxColorComponent > 0f
            ? lBaseEmission / lBaseEmission.maxColorComponent
            : Color.white;

        lEmissionMaterial.EnableKeyword("_EMISSION");
        lEmissionMaterial.SetColor("_EmissionColor", lBaseEmission * 0f);

        _EmissionTween = DOTween
            .Sequence()
            .Append(
                DOTween
                    .To(() => 0f, lValue => lEmissionMaterial.SetColor("_EmissionColor", lBaseEmission * lValue), 5f, 0.25f)
                    .SetLoops(5, LoopType.Yoyo))
            .Append(DOTween.To(() => 0f, lValue => lEmissionMaterial.SetColor("_EmissionColor", lBaseEmission * lValue), 2f, 0.25f));
    }

    private void OnDisable()
    {
        _EmissionTween?.Kill();
        _EmissionTween = null;
    }
}