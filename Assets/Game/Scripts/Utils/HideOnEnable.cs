using Rush.Game.Core;
using UnityEngine;
using UnityEngine.UI;

public class HideOnEnable : MonoBehaviour
{
    [SerializeField] private bool _FadeBlack;
    [SerializeField] private bool _DeactivateHiddenCardInstantly;

    #region _____________________________/ REFS

    [SerializeField] private Transform _CardToHide;

    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OnEnable()
    {
        Manager_Ui.Instance.Show(_CardToHide, _FadeBlack);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}