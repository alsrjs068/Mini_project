using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    // 인스펙터

    [Header("버튼")]
    [SerializeField] private List<Button> _stages = new List<Button>();

    [Header("스테이지 클리어 이미지")]
    [SerializeField] private List<GameObject> _stageClear = new List<GameObject>();

    [Header("선택할 대상 표시 테두리")]
    [SerializeField] private GameObject _selectFrame;

    // 내부 변수

    private int _currentIndex = 0;


    public void EnterStage(int stageIndex)
    {
        if(_stages[stageIndex].interactable == false)
        {
            return;
        }

        switch (stageIndex)
        {
            case 0:
                {
                    SceneFlowManager.Instance.LoadScene(SceneID.Stage1);
                    break;
                }

            case 1:
                {
                    SceneFlowManager.Instance.LoadScene(SceneID.Stage2);
                    break;
                }

            case 2:
                {
                    SceneFlowManager.Instance.LoadScene(SceneID.Stage3);
                    break;
                }

            case 3:
                {
                    SceneFlowManager.Instance.LoadScene(SceneID.Stage4);
                    break;
                }

            case 4:
                {
                    SceneFlowManager.Instance.LoadScene(SceneID.Stage5);
                    break;
                }
        }
    }


    void Start()
    {
        int highestCleared = PlayerPrefs.GetInt("HighestClearedStage", 0);

        for (int i = 0; i < _stageClear.Count; i++)
        {
            if (i + 1 <= highestCleared)
            {
                _stageClear[i].SetActive(true);
            }

            else
            {
                _stageClear[i].SetActive(false);
            }

            if (i + 1 <= highestCleared + 1)
            {
                _stages[i].interactable = true;
            }

            else
            {
                _stages[i].interactable = false;
            }
        }

        if(_selectFrame != null)
        {
            _selectFrame.SetActive(false);
        }

    }

    void Update()
    {
        // 1. 좌우 이동 입력 감지
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _selectFrame.SetActive(true);
            if (_currentIndex > 0)
            {
                _currentIndex--;
                FocusFrame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _selectFrame.SetActive(true);
            if (_currentIndex < _stages.Count - 1)
            {
                _currentIndex++;
                FocusFrame();
            }
        }

        // 2. 키보드 결정 키입력 감지
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            EnterStage(_currentIndex);
        }
    }

    private void FocusFrame()
    {
        if (_selectFrame != null && _stages.Count > 0)
        {
            _selectFrame.transform.position = _stages[_currentIndex].transform.position;
        }
    }


}
