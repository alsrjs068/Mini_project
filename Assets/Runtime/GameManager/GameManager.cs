using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GameManager : MonoBehaviour
{


    #region 인스펙터

    [Header("플레이어")]
    [SerializeField] private Transform _player;

    [Header("배틀 매니저 참조")]
    [SerializeField] private BattleManager _battleManager;

    [Header("게임 일시 정지시 UI")]
    [SerializeField] private GameObject _pausedUI;

    [Header("환경 설정 UI")]
    [SerializeField] private GameObject _settingUI;

    [Header("현재 씬 : 타이틀")]
    [SerializeField] private bool _isTitleScene;

    [Header("게임 종료 UI")]
    [SerializeField] private GameObject _gameQuit;

    [Header("씬 로딩 화면")]
    [SerializeField] private GameObject _loading;

    [Header("던전 스테이지 입장 공간")]
    [SerializeField] private Transform _dungeonPotal;


   
    #endregion

    // 내부 변수

    private bool _paused;
    private bool _isTrans;
    

    public void Title()
    {
       
        if (Input.GetMouseButton(0) && _isTitleScene && _isTrans == false)
        {
            _isTrans = true;
            SceneFlowManager.Instance.LoadScene(SceneID.Lobby);
        }
    }


    public void Paused()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (_paused == false)
            {
                _paused = true;
                _pausedUI.SetActive(true);
                _gameQuit.SetActive(true);
                Time.timeScale = 0;

            }

            else
            {
                _paused = false;
                _pausedUI.SetActive(false);
                _gameQuit.SetActive(false);
                Time.timeScale = 1;
            }

        }
    }

    private void DungeonPotal()
    {
        float distance_ = 3f;
        
        if (_player == null || _dungeonPotal == null)
        {
            return;

        }

        if (_isTrans == true)
        {
            return;
        }

        float gapdis = Vector3.Distance(_player.position, _dungeonPotal.position);

        if (gapdis <= distance_ )
        {
            _isTrans = true;

            SceneFlowManager.Instance.LoadScene(SceneID.SelectStage);
        }
        

    }

    public void OnClickResume()
    {
        _paused = false;
        _pausedUI.SetActive(false);
        Time.timeScale = 1;
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
        _isTrans = false;

        if (_pausedUI != null)
        {
            _pausedUI.SetActive(false);
        }

        if( _settingUI != null)
        {
            _settingUI.SetActive(false);
        }


        if (_loading != null)
        {
            _loading.SetActive(false);
        }

    }

    void Update()
    {
        Paused();

        Title();

        DungeonPotal();
    }
}
