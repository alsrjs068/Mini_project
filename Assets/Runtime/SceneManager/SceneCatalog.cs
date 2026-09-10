using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneID
{
    Title = 0,
    Lobby = 1,
    Stage1 = 2,
    Stage2 = 3,
    Stage3 = 4,
    Stage4 = 5,
    Stage5 = 6,
    SelectStage = 7,
    Market = 8,
    EQEnforce = 9
}

[System.Serializable]
public class SceneEntries
{
    public SceneID Id;
    public string SceneName;
}

public class SceneCatalog : MonoBehaviour
{
    #region 인스펙터
    [Header("씬 카탈로그")]
    [SerializeField] private List<SceneEntries> _scenes = new List<SceneEntries>();

    [Header("옵션")]
    [SerializeField] private bool _buildOnAwake = true;

    #endregion

    // 내부 변수

    private readonly Dictionary<SceneID, string> _idToName = new Dictionary<SceneID, string>();

    private readonly Dictionary<string, SceneID> _nameToId = new Dictionary<string, SceneID>();

    public IReadOnlyList<SceneEntries> Entries => _scenes;


    private void Awake()
    {
        if (_buildOnAwake)
        {
            BuildMaps();
        }
    }

    [ContextMenu("BuildMaps (Rebuild Catalog")]
    public void BuildMaps()
    {
        // 이전에 빌드된 데이터를 먼저 지우고
        _idToName.Clear();
        _nameToId.Clear();

        for (int i = 0; i < _scenes.Count; i++)
        {
            SceneEntries e = _scenes[i];

            if (e == null) // 인스펙터에서 사이즈는 늘렸는데 아무것도 안넣거나 비어있는 경우
            {
                continue;
            }

            if (string.IsNullOrEmpty(e.SceneName)) // 씬 이름이 비어있거나 널인 경우 
            {
                CPrint.Warn($"엔트리가 비어있음 / 인스펙터 확인");

                continue;
            }

            // 중복체크
            if (_idToName.ContainsKey(e.Id))
            {
                CPrint.Warn($"ID가 중복되었다. {e.Id} / 기존 : {_idToName[e.Id]} / 신규 : {e.SceneName}");
                continue;
            }

            // 이름 중복 체크
            if (_nameToId.ContainsKey(e.SceneName))
            {
                continue;
            }

            _idToName.Add(e.Id, e.SceneName);
            _nameToId.Add(e.SceneName, e.Id);

            
        }

        CPrint.Title("씬 카탈로그 빌드");

        CPrint.Log($"리스트 카운트 {_scenes.Count}");
        CPrint.Log($"맵 카운트 (ID → Name) {_idToName.Count}");
        CPrint.Log($"맵 카운트 (Name → ID) {_nameToId.Count}");

    }

    public bool TryGetSceneName(SceneID id, out string sceneName)
    {
        return _idToName.TryGetValue(id, out sceneName);
    }

    public bool TryGetSceneId(string sceneName, out SceneID id)
    {
       
        return _nameToId.TryGetValue(sceneName, out id);
    }

    public List<SceneEntries> GetEntries()
    {
        return _scenes;
    }
}
