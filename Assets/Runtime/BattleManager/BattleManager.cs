using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        DungeonEnter,
        PlayerTurn,
        MonsterTurn,
        Calculation,
        LevelUP,
        Judgement,
        Paused
    }

    #region 인스펙터

    [Header("플레이어와 몬스터")]
    [SerializeField] private BattleUnits _player;
    [SerializeField] private List<BattleUnits> _monsters = new List<BattleUnits>();

    [Header("스폰위치")]
    [SerializeField] private Transform _pSpawnPoint;
    [SerializeField] private List<Transform> _mSpawnPoint = new List<Transform>();

    [Header("스킬UI")]
    [SerializeField] private GameObject _playerSkillUI;

    #endregion


    // 내부 변수

    private BattleState _currentState;
    private int _currentStage = 1;
    private BattleUnits _selectedTarget;

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

            case BattleState.Calculation:
                {
                    Calculate();
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
                    Paused();
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

    }

    public void Calculate()
    {

    }

    public void LevelUP()
    {


    }

    public void Judgement()
    {

    }

    public void Paused()
    {

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
            _playerSkillUI.SetActive(false);
            ChangeState(BattleState.MonsterTurn);
        }

        else
        {
            _selectedTarget.TakeDamage(_player.ATK);
        }

        _playerSkillUI.SetActive(false);
        ChangeState(BattleState.MonsterTurn);

        
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
