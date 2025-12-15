#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Rush.Game.Core;
using Rush.Game;

public class UI_Btn_Navigation : MonoBehaviour
{
    #region _____________________________/ BTN VALUES
    
    public enum BtnTransitions { Show, Hide, Switch, Quit }

    [Header("Navigation")]
    [SerializeField] private BtnTransitions _BtnType;
    [SerializeField] private bool _LevelUnloader, _Retry, _StartGame;
    [SerializeField] private bool _FadeBlack;
    
    #endregion

    #region _____________________________/ REFS

    [Header("References")]
    [SerializeField] private Button _Button;
    [SerializeField] private Transform _CardToShow;
    [SerializeField] private Transform _CardToHide;

    #endregion

    #region _____________________________| UNITY

    private void Init() {
        if (_Button == null) _Button = GetComponent<Button>();
        if (_CardToHide == null) _CardToHide = transform.parent; }

    void Start()
    {
        Init();

        if (_Button == null) return;
        _Button.onClick.AddListener(OnClick);
    }

    #endregion

    #region _____________________________| UI METHODS

    private void OnClick()
    {
        switch (_BtnType)
        {
            case BtnTransitions.Show:   Show();      break;
            case BtnTransitions.Hide:   Hide();      break;
            case BtnTransitions.Switch: Switch();    break;
            case BtnTransitions.Quit:   QuitGame();  break;
            default: break;
        }

        if (ShouldDelayActions())
            StartCoroutine(ExecuteActionsAfterBlackMidpoint());
        else
            ExecuteAdditionalActions();
    }

    private bool ShouldDelayActions()
    {
        return _FadeBlack && (_LevelUnloader || _Retry || _StartGame);
    }

    private void Show()     => Manager_Ui.Instance.Show(_CardToShow, _FadeBlack);

    private void Hide()     => Manager_Ui.Instance.Hide(_CardToHide, _FadeBlack);

    private void Switch()   => Manager_Ui.Instance.Switch(_CardToShow, _CardToHide, _FadeBlack);

    void QuitGame()         => Application.Quit();

    IEnumerator ExecuteActionsAfterBlackMidpoint()
    {
        if (Manager_Ui.Instance != null)
            yield return Manager_Ui.Instance.WaitForBlackMidpoint();

        ExecuteAdditionalActions();
    }

    private void ExecuteAdditionalActions()
    {
        if (_Retry) Retry();
        if (_LevelUnloader) UnloadLevel();
        if (_StartGame) Play();
    }

    void UnloadLevel()      => Manager_Game.Instance.UnloadCurrentLevel();

    void Play() => Manager_Game.Instance.StartGame();

    void Retry() => Manager_Game.Instance.Retry();

    #endregion
}