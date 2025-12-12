#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

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

        switch (_BtnType)
        {
            case BtnTransitions.Show:   _Button.onClick.AddListener(Show);      break;
            case BtnTransitions.Hide:   _Button.onClick.AddListener(Hide);      break;
            case BtnTransitions.Switch: _Button.onClick.AddListener(Switch);    break;
            case BtnTransitions.Quit:   _Button.onClick.AddListener(QuitGame);  break;
            default: break;
        }

        if (_Retry) _Button.onClick.AddListener(Retry);
        if (_LevelUnloader) _Button.onClick.AddListener(UnloadLevel);
        if (_StartGame) _Button.onClick.AddListener(Play);
    }

    #endregion

    #region _____________________________| UI METHODS
   
    private void Show()     => Manager_Ui.Instance.Show(_CardToShow);

    private void Hide()     => Manager_Ui.Instance.Hide(_CardToHide);

    private void Switch()   => Manager_Ui.Instance.Switch(_CardToShow, _CardToHide);

    void QuitGame()         => Application.Quit();

    void UnloadLevel()      => Manager_Game.Instance.UnloadCurrentLevel();

    void Play() => Manager_Game.Instance.StartGame();

    void Retry() => Manager_Game.Instance.Retry();

    #endregion
}