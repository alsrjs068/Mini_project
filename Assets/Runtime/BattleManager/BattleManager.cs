using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using static BattleUnits;

public enum BattleState
{
    DungeonEnter,
    PlayerTurn,
    MonsterTurn,
    LevelUP,
    Judgement
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
    [SerializeField] private Button _skillButton;

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

    [Header("씬 로딩 화면")]
    [SerializeField] private GameObject _loading;

    [Header("스킬 활성화 UI")]
    [SerializeField] private GameObject _skillInfo;

    [Header("스킬 정보 UI")]
    [SerializeField] private TextMeshProUGUI _skillInfoText;

    [Header("스킬 활성화 frameUI")]
    [SerializeField] private GameObject _skillActiveUI;

    [Header("현재 스테이지")]
    [SerializeField] private int _currentStage = 1;

    [Header("경험치")]
    [SerializeField] private Slider _expSlider;
    [SerializeField] private TextMeshProUGUI _expText;

    [Header("스테이지 클리어 효과음")]
    [SerializeField] private AudioClip _stageClear;

    [Header("스테이지 실패 효과음")]
    [SerializeField] private AudioClip _stageFail;

    [Header("플레이어 레벨업 효과음")]
    [SerializeField] private AudioClip _LevelUpSFX;

    [Header("플레이어 선택 효과음")]
    [SerializeField] private AudioClip _SelectSFX;

    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject _targetSelectPrefab; // 타겟 선택시 이펙트
    #endregion


    // 내부 변수

    private BattleState _currentState;
    private BattleUnits _selectedTarget;
    private int _totalEXP = 0;
    private int _amount;
    private int _selectCardIndex;
    private List<BattleUnits> _selectedTargets = new List<BattleUnits>();
    private bool _isSkillMode = false;
    private int _highestStage = 0;
    private bool _isAttacking = false;
    private int _countLevelUps = 0;
    protected AudioSource _audioSource;

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

        }
            
                
    }


    public void Enter()
    {
        // 모든 유닛 스탯 불러오기 

        BattleUnits.LoadEnforceData();
        _player.InitStat(Stat.CreatePlayer(BattleUnits.Level));
        UpdateExpUI();

        foreach (var monster in _monsters)
        {
            if (monster.CompareTag("Skelleton"))
            {
                monster.InitStat(Stat.CreateSkelleton(_currentStage));
            }

            else if (monster.CompareTag("Zombie"))
            {
                monster.InitStat(Stat.CreateZombie(_currentStage));
            }

            else if (monster.CompareTag("MiddleBoss"))
            {
                monster.InitStat(Stat.MiddleBossStat(_currentStage));
            }

            else if (monster.CompareTag("MainBoss"))
            {
                monster.InitStat(Stat.MainBossStat(_currentStage));
            }

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
        _isAttacking = false;
        _selectedTargets.Clear();
        _playerSkillUI.SetActive(true);
        _skillButton.interactable = _player.CanUseSkill();
    }

    public void MonsterT()
    {
        StartCoroutine(MonsterTcrt());
    }

    public void LevelUP()
    {
        if (_LevelUpSFX != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_LevelUpSFX);
        }

        // 랜덤으로 중복되지 않게 숫자 뽑기
        int numA = Random.Range(0, 4);
        int numB;
        do
        {
            numB = Random.Range(0, 4);


        } while (numA == numB); // 이 조건이면 다시뽑기.

        _enforceSkillUI[numA].SetActive(true);
        _enforceSkillUI[numA].GetComponent<UnityEngine.UI.Button>().interactable = true;

        _enforceSkillUI[numB].SetActive(true);
        _enforceSkillUI[numB].GetComponent<UnityEngine.UI.Button>().interactable = true;

    }

    public void Judgement()
    {
        if(MonsterClear() == true)
        {
            _countLevelUps = _player.GainEXP(_totalEXP);

            UpdateExpUI();

            _highestStage = PlayerPrefs.GetInt("HighestClearedStage", 0);

            if(_currentStage > _highestStage)
            {
                _highestStage = _currentStage;
            }

            PlayerPrefs.SetInt("HighestClearedStage", _highestStage);
            SavePlayerData();
            PlayerPrefs.Save();

            if (_countLevelUps > 0)
            {
                ChangeState(BattleState.LevelUP);
            }

            else
            {
                if (_stageClear != null && _audioSource != null)
                {
                    _audioSource.PlayOneShot(_stageClear);
                }
                _victoryUI.SetActive(true);
                _defeatUI.SetActive(false);
                _returnLobbyUI.SetActive(true);
                _retrunselecstageUI.SetActive(true);
            }

            _totalEXP = 0; // 경험치 지급이 끝났으면 획득 경험치는 다시 0으로 초기화.
        }

        else if (_player.IsDead)
        {
            if (_stageFail != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_stageFail);
            }
            _defeatUI.SetActive(true);
            _returnLobbyUI.SetActive(true);
            _retrunselecstageUI.SetActive(true);
        }

        else
        {
            ChangeState(BattleState.PlayerTurn);
        }

    }

    
    public void SelectTarget(BattleUnits target)
    {

        if (_isAttacking == true) // 공격 중인지 확인
        {
            return;
        }

        if (_currentState != BattleState.PlayerTurn) // 플레이어 턴인지 확인
        {   
            return;
        }

        if (target.IsDead == true) // 선택한 타겟이 죽어있는지 확인
        {
            return;
        }

        // 선택된 타겟 클릭 시 선택 취소
        if (_selectedTargets.Contains(target))
        {
            
            _selectedTargets.Remove(target);
            return;
        }

        // 목표치까지 타겟 넣고
        if (_selectedTargets.Count < _player.GetTargetCount())
        {
            if(target == _player)
            {
                if (_isSkillMode)
                {
                    _selectedTargets.Add(target);
                    GameObject selectE = Instantiate(_targetSelectPrefab, target.transform.position + Vector3.up, Quaternion.identity);
                }

                else
                {
                    return;
                }
            }

            else
            {
                _selectedTargets.Add(target);
            }

        }


        // 현재 필드에 살아있는 몬스터 수 계산
        int aliveMonsterCount = 0;

        for (int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i].IsDead == false)
            {
                aliveMonsterCount++;
            }
        }

        // 목표로 채워야 할 타겟 수

        int maxSelectable;

        if (_selectedTargets.Contains(_player))
        {
            maxSelectable = aliveMonsterCount + 1;
        }

        else
        {
            maxSelectable = aliveMonsterCount;
        }

        int requiredCount = Mathf.Min(_player.GetTargetCount(), maxSelectable);

        // 아직 선택이 부족하다면 공격하지 않고 대기
        if (_selectedTargets.Count < requiredCount)
        {
            return;
        }

        _skillInfo.SetActive(false);
        _skillActiveUI.SetActive(false);

        // 목표 타겟 수가 모두 찼으므로 공격시작
        _isAttacking = true;

        StartCoroutine(Co_PlayerAttack(_isSkillMode));

        _isSkillMode = false;

    }

    IEnumerator MonsterTcrt() // 몬스터 턴 코루틴
    {

        yield return new WaitForSeconds(1.0f);

        foreach (var monster in _monsters)
        {
            if(_player.IsDead) // 플레이어가 죽어있는지 확인 -> 안죽었으면 공격 / 죽었으면 판단단계로 점프
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
                yield return new WaitForSeconds(2.0f); // 공격 애니메이션 출력 시간 기다리기
            }

        }

        ChangeState(BattleState.Judgement);

    }

    private IEnumerator Co_PlayerAttack(bool canSkill)
    {
        for (int i = 0; i < _selectedTargets.Count; i++)
        {
            BattleUnits currentTarget = _selectedTargets[i];

            if (currentTarget == _player) // 타겟 안에 플레이어가 있다면 플레이어는 회복
            {
                _player.HealSelf();
            }

            else
            {
                if (canSkill)
                {
                    _player.HealSkill(currentTarget);
                }

                else
                {
                    _player.Attack(currentTarget);
                    yield return new WaitForSeconds(0.8f);
                }
                    

            }
        }

        yield return new WaitForSeconds(1.2f);

        // 공격 후 리스트 초기화 및 턴 전환
        _selectedTargets.Clear();
        _playerSkillUI.SetActive(false);

        if (MonsterClear() == true)
        {
            foreach (var monster in _monsters)
            {
                _totalEXP += monster.ExpReward;
            }

            ChangeState(BattleState.Judgement);
        }
        else
        {
            ChangeState(BattleState.MonsterTurn);
        }
    }

    private bool MonsterClear() // 모든 몬스터를 죽인 것을 확인
    {
        for(int i = 0;  i < _monsters.Count; i++)
        {
            if(_monsters[i].IsDead != true)
            {
                return false;
            }
        }

        return true;
    }

    public void OnClickHealSkill()
    {
        // 이미 켜져 있다면 취소
        if (_isSkillMode == true)
        {
            _isSkillMode = false;
            _skillInfo.SetActive(false);
            _skillActiveUI.SetActive(false);
            _selectedTargets.Clear(); // 선택 중이던 타겟 초기화
            return;
        }

        // 쿨타임 충족 시 켜기
        if (_player.CanUseSkill())
        {
            _isSkillMode = true;
            _skillInfo.SetActive(true);
            UpdateSkillInfoUI();
            _skillActiveUI.SetActive(true);
        }
    }

    public void OnClickToLobby() // 로비로 가는 UI
    {
        
        if (_SelectSFX != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_SelectSFX);
        }

        Time.timeScale = 1f;
        SceneFlowManager.Instance.ResetLoading();

        if (_loading != null)
        {
            _loading.SetActive(true);
        }

        SceneFlowManager.Instance.LoadScene(SceneID.Lobby);
    }

    public void OnClickToSelectStage() // 스테이지 선택창으로 가는 UI
    {
        
        if (_SelectSFX != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_SelectSFX);
        }
        Time.timeScale = 1f;

        if (_loading != null)
        {
            _loading.SetActive(true);
        }

        SceneFlowManager.Instance.LoadScene(SceneID.SelectStage);
    }
    
    public void OnClickSkillEnfoce(int cardIndex) // 스킬 강화 선택 
    {

        _selectCardIndex = cardIndex;

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

        SavePlayerData();
        StartCoroutine(CardSelect());

    }

    public static void SavePlayerData() // 스킬 강화 정보 저장
    {
        PlayerPrefs.SetInt("PlayerLevel", BattleUnits._level);
        PlayerPrefs.SetFloat("_currentEXP", BattleUnits.CurrentEXP);

        PlayerPrefs.SetFloat("damageEnforce", BattleUnits.CurrentEnforce.damageEnforce);
        PlayerPrefs.SetFloat("declineCooldown", BattleUnits.CurrentEnforce.declineCooldown);
        PlayerPrefs.SetInt("BonusHeal", BattleUnits.CurrentEnforce.BonusHeal);
        PlayerPrefs.SetInt("targetCount", BattleUnits.CurrentEnforce.targetCount);
        PlayerPrefs.SetInt("criticalChance", BattleUnits.CurrentEnforce.criticalChance);

        PlayerPrefs.SetInt("Enforce_Resurrection", BattleUnits.CurrentEnforce.playerresurrection ? 1 : 0);
        PlayerPrefs.SetInt("isCritical", BattleUnits.CurrentEnforce.isCritical ? 1 : 0);
        PlayerPrefs.SetInt("Marked", BattleUnits.CurrentEnforce.Marked ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void UpdateExpUI() // 경험치, 레벨 UI
    {

        if (_player == null)
        {
            return;
        }

        if (_expSlider != null)
        {
            _expSlider.maxValue = BattleUnits.MaxEXP;
            _expSlider.value = BattleUnits.CurrentEXP;

        }

        if(_expText != null)
        {
            _expText.text = $"Lv. {BattleUnits._level} ({BattleUnits._currentEXP} / {BattleUnits._MaxEXP})";
        }

    }

    public void UpdateSkillInfoUI() // 스킬 강화 정보 UI
    {
        _skillInfoText.text = $"추가 피해 : +{BattleUnits.CurrentEnforce.damageEnforce}\n추가 회복 : +{BattleUnits.CurrentEnforce.BonusHeal}\n쿨타임 감소 : -{BattleUnits.CurrentEnforce.declineCooldown}초\n치명타 확률 : {BattleUnits.CurrentEnforce.criticalChance}%\n공격 가능 타겟 :{BattleUnits.CurrentEnforce.targetCount + 1}";
    }


    private IEnumerator CardSelect() // 카드 선택 코루틴
    {

        for (int i = 0; i < _enforceSkillUI.Count; i++)
        {
            if( i != _selectCardIndex)
            {
                _enforceSkillUI[i].SetActive(false);
            }

        }

        _enforceSkillUI[_selectCardIndex].GetComponent<UnityEngine.UI.Button>().interactable = false;
        _countLevelUps--;
        yield return new WaitForSeconds(3f); 

        _enforceSkillUI[_selectCardIndex].SetActive(false);

        if (_countLevelUps > 0)
        {
            LevelUP();
        }

        else
        {
            _victoryUI.SetActive(true);
            _returnLobbyUI.SetActive(true);
            _retrunselecstageUI.SetActive(true);
        }

    }
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
    }

    private void Start()
    {
        if (_player == null || _monsters.Count <= 0 || _pSpawnPoint == null || _mSpawnPoint.Count <= 0)
        {
            CPrint.Warn("인스펙터가 비어있다. 게임 실행 불가");
            return;
        }

        if (_expText == null)
        {
            CPrint.Warn("_expText 슬롯이 인스펙터에서 비어있다.");
        }

        BattleUnits.LoadEnforceData();
        
        // 스킬 UI 끄기
        if (_playerSkillUI != null)
        {
            _playerSkillUI.SetActive(false);
        }

        // 스킬 info 끄기

        if(_skillInfo != null && _skillActiveUI != null)
        {
            _skillInfo.SetActive(false);
            _skillActiveUI.SetActive(false);
        }

        // 결과창 UI 끄기
        if (_victoryUI != null)
        {
            _victoryUI.SetActive(false);
        }

        if (_defeatUI != null)
        {
            _defeatUI.SetActive(false);
        }

        // 씬 이동 선택 UI 끄기
        if (_returnLobbyUI != null)
        {
            _returnLobbyUI.SetActive(false);
        }

        if (_retrunselecstageUI != null)
        {
            _retrunselecstageUI.SetActive(false);
        }

        // 스킬 강화 카드 UI 끄기
        if (_enforceSkillUI != null)
        {
            for (int i = 0; i < _enforceSkillUI.Count; i++)
            {
                if (_enforceSkillUI[i] != null)
                {
                    _enforceSkillUI[i].SetActive(false);
                }
            }
        }

        
        // 로딩 UI 끄기
        if (_loading != null)
        {
            _loading.SetActive(false);
        }

        ChangeState(BattleState.DungeonEnter);
    }

}
