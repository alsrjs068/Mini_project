using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESceneID
{
    Title,
    Lobby,
    SelectStage,
    Battle
}

[System.Serializable]
public class SceneEntry
{
    public ESceneID Id;
    public string SceneName;

}

public class SceneChange : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
