using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{


    #region 인스펙터

    [Header("배틀 매니저 참조")]
    [SerializeField] private BattleManager _battleManager;

    [Header("게임 일시 정지시 UI")]
    [SerializeField] private GameObject _pausedUI;

    [Header("환경 설정 UI")]
    [SerializeField] private GameObject _settingUI;

    #endregion

    // 내부 변수

    private bool _paused;


    public void Paused()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (_paused == false)
            {
                _paused = true;
                _pausedUI.SetActive(true);
                Time.timeScale = 0;
                _battleManager.ChangeState(BattleState.Paused);

            }

            else
            {
                _paused = false;
                _pausedUI.SetActive(false);
                Time.timeScale = 1;
            }

        }
    }


    public void OnClickSetting() // 환경설정 UI
    {
        _pausedUI.SetActive(false);
        _settingUI.SetActive(true);

    }

    public void OnClickSave() // 게임 저장 UI
    {

    }

    public void OnClickQuit() // 게임 종료 UI
    {
        Application.Quit();
    }

    private void Awake()
    {
        
    }

    void Start()
    {
        _paused = false;

        if ( _pausedUI == null )
        {
            CPrint.Warn("UI 출력 불가, 인스펙터 확인");
            return;
        }
    }

    void Update()
    {
        Paused();

        
    }
}
