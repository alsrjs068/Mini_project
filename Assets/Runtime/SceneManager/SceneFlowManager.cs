using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
 
    #region 인스펙터

    // ID <-> Name 변환을 담당
    [Header("카탈로그")]
    [SerializeField] private SceneCatalog _catalog;

    // 연출 담당
    [Header("UI 전환")]
    [SerializeField] private SceneTranslationUI _TransitionUI;

    [Header("옵션 - 유지")]
    [SerializeField] private bool _dontDestroyOnLoad = true;

    [Header("씬 로딩 화면")]
    [SerializeField] private GameObject _loading;

    #endregion

    #region 내부 변수

    private static SceneFlowManager _instance;
    private int _cursorIndex = 0;
    private bool _isLoading = false;            

    public static SceneFlowManager Instance => _instance;
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            CPrint.Warn("중복이 감지되었습니다. 현재 오브젝트를 제거합니다");

            Destroy(gameObject);

            return;
        }

        // SceneSystem
        if (_dontDestroyOnLoad)
        {
            // 사용하기에 편리하기도 하다
            // 다만 게임 전체에서 계속 유지는 전역 시스템이 되기 떄문에 사용시 주의도 필요하다.
            // ㄴ PlayerManager / PlayerSIng... : 플레이어는 적합하지 않다.
            DontDestroyOnLoad(this.gameObject);
        }

        if (_catalog == null)
        {
            CPrint.Error("카탈로그가 비어있다. 현재 오브젝터를 제거합니다.");

            Destroy(gameObject);
            return;
        }

        _instance = this;

        _catalog.BuildMaps();

        SyncCursorToCurrentScene();
    }

    void Start()
    {
        if (_TransitionUI != null)
        {

            _TransitionUI.Initialize();
        }
    }

    void Update()
    {
        
        if (_catalog == null)
        {
            return;
        }

        if (_isLoading)
        {
            return;
        }

        
    }

    // 커서 동기화
    private void SyncCursorToCurrentScene()
    {
        // 인스펙터
        List<SceneEntries> entries = _catalog.GetEntries();

        if (entries == null || entries.Count == 0)
        {
            return;
        }

        string currentName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].SceneName == currentName)
            {
                _cursorIndex = i;

                // 로그 → 커서 싱크

                return;
            }
        }

        // 예외처리
        if (_cursorIndex == 0)
        {
            CPrint.Warn($"커서 싱크를 실패했다.{currentName}");
            return;
        }

    }
    public void ResetLoading()
    {
        _isLoading = false;
    }

    public void LoadScene(SceneID id)
    {
        // 카탈로그 조회

        if (_catalog.TryGetSceneName(id, out string sceneName) == false)

        {
            CPrint.Warn("로드 씬 실패 없는 ID입니다.");

            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            // id가 비어있는 경우

            return;
        }

        if (_loading != null)
        {
            _loading.SetActive(true);
        }

        // 전환
        StartCoroutine(Co_LoadSceneWithTransition(id, sceneName));

    }

    private IEnumerator Co_LoadSceneWithTransition(SceneID id, string sceneName)
    {
        if (_isLoading)
        {
            // 이미 로딩중이니 브레이크를 건다.
            yield break;
        }

        _isLoading = true;

        CPrint.Log($"id = {id} / sceneName = {sceneName}");

        if (_TransitionUI != null)
        {
            // 텍스트를 뽑고
            _TransitionUI.SetLoadingText("로딩중...");

            yield return _TransitionUI.Co_FadeTo(1f);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        op.allowSceneActivation = true;

        yield return null;

        if (_loading != null)
        {
            _loading.SetActive(false);
        }

        if (_TransitionUI != null)
        {
            yield return _TransitionUI.Co_FadeTo(0f);
            _TransitionUI.SetLoadingText("");
        }

        // 동기화

        SyncCursorToCurrentScene();

        CPrint.Success($"씬 로드 → {sceneName}");

        _isLoading = false;
    }

    private void ReloadCurrent()
    {
        string current = SceneManager.GetActiveScene().name;

        if (_catalog.TryGetSceneId(current, out SceneID id) == false) // 아이디 확인하고 씬 비교했는데 현재 씬에 이름이 없다 -> 리턴
        {
            return;
        }

        CPrint.Log($"리로드 {current}");

        LoadScene(id); // 모든 전환은 로드씬으로 통일한다.



    }

    // 인스턴스 정리
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
