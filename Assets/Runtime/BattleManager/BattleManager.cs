using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public enum BattleState
{
    DungeonEnter,
    PlayerTurn,
    MonsterTurn,
    LevelUP,
    Judgement,
    Paused
}

public class BattleManager : MonoBehaviour
{

    #region 인스펙터

    [Header("플레이어와 몬스터")]
    [SerializeField] private BattleUnits _player;
    [SerializeField] private List<BattleUnits> _monsters = new List<BattleUnits>();

    [Header("스폰위치")]
    [SerializeField] private Transform _pSpawnPoint;
    [SerializeField] private List<Transform> _mSpawnPoint = new List<Transform>();

    [Header("스킬 UI")]
    [SerializeField] private GameObject _playerSkillUI;

    [Header("승리 UI")]
    [SerializeField] private GameObject _victoryUI;

    [Header("패배 UI")]
    [SerializeField] private GameObject _defeatUI;

    [Header("로비 씬으로 돌아가는 선택 UI")]
    [SerializeField] private GameObject _returnLobbyUI;

    [Header("스테이지 선택으로 돌아가는 선택 UI")]
    [SerializeField] private GameObject _retrunselecstageUI;

    [Header("스킬 강화 UI 넣기")]
    [SerializeField] private List<GameObject> _enforceSkillUI = new List<GameObject>();

    #endregion


    // 내부 변수

    private BattleState _currentState;
    private int _currentStage = 1;
    private BattleUnits _selectedTarget;
    private int _totalEXP = 0;
    private int _amount;
    

    public void ChangeState(BattleState newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case BattleState.DungeonEnter:
                {
                    Enter();
                    break;
                }

            case BattleState.PlayerTurn:
                {
                    PlayerT();
                    break;
                }

            case BattleState.MonsterTurn:
                {
                    MonsterT();
                    break;
                }

            case BattleState.LevelUP:
                {
                    LevelUP();
                    break;
                }

            case BattleState.Judgement:
                {
                    Judgement();
                    break;
                }

            case BattleState.Paused:
                {
                    break;
                }

        }
            
                
    }


    public void Enter()
    {
        // 모든 유닛 스탯 불러오기 

        _player.InitStat(Stat.CreatePlayer());

        foreach (var monster in _monsters)
        {
            monster.InitStat(Stat.CreateSkelleton(_currentStage));
        }

        // 스폰 포인트 지정에 시작하면 위치로 이동시키기

        _player.transform.position = _pSpawnPoint.position;

        for (int i = 0; i < _monsters.Count; i++)
        {
            _monsters[i].transform.position = _mSpawnPoint[i].position;
        }

        ChangeState(BattleState.PlayerTurn);

    }

    
    public void PlayerT() // 스킬의 쿨타임을 UI에 표시하고, 사용할 수 있는지 없는지만 보여준다.
    {
        _playerSkillUI.SetActive(true);
    }

    public void MonsterT()
    {
        StartCoroutine(MonsterTcrt());
    }

    public void LevelUP()
    {
        // 랜덤으로 중복되지 않게 숫자 뽑기
        int numA = Random.Range(0, 4);
        int numB;
        do
        {
            numB = Random.Range(0, 4);


        } while (numA == numB); // 이 조건이면 다시뽑기.

        _enforceSkillUI[numA].SetActive(true);
        _enforceSkillUI[numB].SetActive(true);

    }

    public void Judgement()
    {
        if(MonsterClear() == true)
        {
            bool isLevelUp = _player.GainEXP(_totalEXP);

            if (isLevelUp == true)
            {
                ChangeState(BattleState.LevelUP);
            }

            else
            {
                _victoryUI.SetActive(true);
            }

            _totalEXP = 0; // 경험치 지급이 끝났으면 획득 경험치는 다시 0으로 초기화.
        }

        else if (_player.IsDead)
        {
            _defeatUI.SetActive(true);
        }

        else
        {
            ChangeState(BattleState.PlayerTurn);
        }

    }

    
    public void SelectTarget(BattleUnits target)
    {
        if(_currentState == BattleState.PlayerTurn && target.IsDead == false)
        {
            _selectedTarget = target;
        }

        else
        {
            return;
        }

        if (_player.CanUseSkill())
        {
            _player.HealSkill(_selectedTarget);

            if( _selectedTarget.IsDead == true )
            {
                _totalEXP = _totalEXP + _selectedTarget.ExpReward;
            }

            if(MonsterClear() == true)
            {
                ChangeState(BattleState.Judgement);
            }
        }

        else
        {
            _player.Attack(_selectedTarget);

            if (_selectedTarget.IsDead == true)
            {
                _totalEXP = _totalEXP + _selectedTarget.ExpReward;
            }

            if (MonsterClear() == true)
            {
                ChangeState(BattleState.Judgement);
            }
        }

        _playerSkillUI.SetActive(false);
        ChangeState(BattleState.MonsterTurn);

        
    }

    IEnumerator MonsterTcrt()
    {
        
        foreach (var monster in _monsters)
        {
            if(_player.IsDead) // 플레이어가 죽어있는지 확인. 안죽었으면 공격 / 죽었으면 판단단계로 점프
            {
                ChangeState(BattleState.Judgement);
                yield break;
            }

            if (monster.IsDead) // 죽었으면 실행 X
            {
                continue;
            }

            else
            {
                monster.Attack(_player);
                yield return new WaitForSeconds(1.5f); // 공격 애니메이션 출력 시간 기다리기
            }


        }

        ChangeState(BattleState.PlayerTurn);

    }

    private bool MonsterClear() // 모든 몬스터를 죽인 것을 확인.
    {
        foreach(var monster in _monsters)
        {
            if(monster.IsDead == false)
            {
                break;
            }

            else
            {
                return true;
            }
        }

        return false;
    }

    public void OnClickToLobby() // 로비로 가는 UI
    {

    }

    public void OnClickToSelectStage() // 스테이지 선택창으로 가는 UI
    {

    }
    
    public void OnClickSkillEnfoce(int cardIndex)
    {
        switch (cardIndex)
        {
            case 0:
                {
                    BattleUnits.HealEnforce newEnforce0 = new BattleUnits.HealEnforce
                    {
                        playerresurrection = true,
                        damageEnforce = 30,
                        BonusHeal = 15
                    };
                    _player.EnforcedSkill(newEnforce0);
                    break;
                }
            
            case 1:
                {
                    BattleUnits.HealEnforce newEnforce1 = new BattleUnits.HealEnforce
                    {
                        criticalChance = 20,
                        isCritical = true
                    };
                    _player.EnforcedSkill(newEnforce1);
                    break;

                }

            case 2:
                {
                    BattleUnits.HealEnforce newEnforce2 = new BattleUnits.HealEnforce
                    {
                        Marked = true,
                        declineCooldown = 1
                    };
                    _player.EnforcedSkill(newEnforce2);
                    break;
                }

            case 3:
                {
                    BattleUnits.HealEnforce newEnforce3 = new BattleUnits.HealEnforce
                    {
                        BonusHeal = 30,
                        targetCount = 1
                    };
                    _player.EnforcedSkill(newEnforce3);
                    break;
                }

        }

        foreach(var card in _enforceSkillUI)
        {
            card.SetActive(false);
        }

        _currentStage++;
        ChangeState(BattleState.DungeonEnter);


    }
    
    

    


    private void Start()
    {
        if (_player == null || _monsters.Count <= 0 || _pSpawnPoint == null || _mSpawnPoint.Count <= 0)
        {
            CPrint.Warn("인스펙터가 비어있다. 게임 실행 불가");
            return;
        }

        ChangeState(BattleState.DungeonEnter);
    }

}
