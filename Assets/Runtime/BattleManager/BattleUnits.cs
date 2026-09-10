using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public enum SkillStat
{
    ATK,
    HP,
    SkillCoolDown,
    BonusDamage
}


public class BattleUnits : MonoBehaviour, IDamageable, IAttackable // 추상화, 캡슐화, 인터페이스 사용하기
{
    // 인스펙터

    [Header("스켈레톤만 넣기")]
    [SerializeField] private GameObject _deathPrefab;

    [Header("체력 UI")]
    [SerializeField] private UnityEngine.UI.Slider _HpSlider;

    [Header("스킬 활성화 frameUI")]
    [SerializeField] private GameObject _skillActiveUI;


    // 내부 변수

    protected Animator _animator;
    protected Stat.Unit _stat;

    private int _level = 1;
    private float _currentEXP = 0f;
    private float _MaxEXP = 100.0f;
    private float _currentCoolDown = 0;
    private HealEnforce _currentEnforce;
    public bool isMarked = false;
    

    // 람다식

    public bool IsDead => _stat.isDead;
    public float CurrentHP => _stat.CurrentHP;
    public int ExpReward => _stat.ExpReward;
    public int ATK => _stat.ATK;
    

    public void InitStat(Stat.Unit stat)
    {
        _stat = stat;

        if(_HpSlider != null)
        {
            
            if (_stat.finalHP > 0)
            {
                _HpSlider.maxValue = _stat.finalHP;
            }
            else
            {
                _HpSlider.maxValue = _stat.MaxHP;
            }

            _HpSlider.value = _stat.CurrentHP;
        }

    }


    public virtual void TakeDamage(int attackerATK)
    {
        if (_stat.isDead)
        {
            return;
        }

        float Damage = attackerATK * (100f / (100f + _stat.DFS));

        _stat.CurrentHP = _stat.CurrentHP - Damage;

        _animator.SetTrigger("tHit");

        if (_HpSlider != null)
        {
            _HpSlider.value = _stat.CurrentHP;
        }

        if (_HpSlider != null)
        {
            _HpSlider.value = _stat.CurrentHP;

        }

        if (_stat.CurrentHP <= 0)
        {
            if (_currentEnforce.playerresurrection == true)
            {
                Resurrection();
            }

            else
            {
                OnDeath();
            }

        }
    }

    public virtual void OnDeath()
    {
        _stat.CurrentHP = 0;

        GetComponent<CapsuleCollider>().enabled = false;

        _animator.SetTrigger("tDie");

        _stat.isDead = true;

        if(CompareTag("Skelleton"))
        {
            if (_deathPrefab != null) // 스켈레톤의 사망 시 해당 프리팹으로 바꾸기
            {
                Instantiate(_deathPrefab, transform.position, transform.rotation);
            }

            gameObject.SetActive(false);
        }

        else
        {
            StartCoroutine(Co_Die());
        }

        
        
    }

    public virtual void Attack(IDamageable target)
    {
        target.TakeDamage(ATK);
        _animator.SetTrigger("tAttack");

        if (_currentEnforce.Marked == true)
        {
            (target as BattleUnits).isMarked = true;
        }

    }
    
    private IEnumerator Co_Die()
    {
        yield return new WaitForSeconds(1.5f);

        gameObject.SetActive(false);
    }

    public virtual IEnumerator TurnExchange()
    {
        yield break;
    }

    public struct HealEnforce
    {
        public int targetCount;
        public float damageEnforce;
        public float declineCooldown;
        public bool playerresurrection;
        public bool isCritical;
        public int BonusHeal;
        public int criticalChance;
        public bool Marked;

    }

    public void EnforcedSkill(HealEnforce newEnforce)
    {
        _currentEnforce.damageEnforce += newEnforce.damageEnforce;
        _currentEnforce.criticalChance += newEnforce.criticalChance;
        _currentEnforce.BonusHeal += newEnforce.BonusHeal;
        _currentEnforce.targetCount += newEnforce.targetCount;
        _currentEnforce.declineCooldown += newEnforce.declineCooldown;
        _currentEnforce.playerresurrection = _currentEnforce.playerresurrection || newEnforce.playerresurrection;
        _currentEnforce.isCritical = _currentEnforce.isCritical || newEnforce.isCritical;
        _currentEnforce.Marked = _currentEnforce.Marked || newEnforce.Marked;

    }

    public void EnforceStat(SkillStat stat, float value)
    {
        switch (stat)
        {
            case SkillStat.ATK:
                _stat.ATK += (int)value;
                break;
            case SkillStat.HP:
                _stat.MaxHP += value;
                break;
            case SkillStat.SkillCoolDown:
                _stat.SkillCoolDown -= value;
                break;
            case SkillStat.BonusDamage:
                _stat.BonusDamage += (int)value;
                break;
        }

    }

    public bool CanUseSkill()
    {
        if (_currentCoolDown <= 0f)
        {
            return true;
        }

        return false;
        
    }
    public void HealSkill(BattleUnits target)
    {

        if (_stat.isDead)
        {
            return;
        }
        
        _animator.SetTrigger("tHeal");


            float Damage = _stat.finalATK * 1.5f + _currentEnforce.damageEnforce;
            // 언데드 몬스터에게 성속성 힐 공격 -> 1.5배 추가 데미지 + 추가 고정 데미지

            if((target as BattleUnits).isMarked == true) // 일반 공격시 표식이 생기는 옵션을 먹었을 때
            {
                Damage = Damage + 50f;

                (target as BattleUnits).isMarked = false;
            }

            if (_currentEnforce.isCritical) // 치명타가 가능한 옵션을 먹었을 때.
            {
                int randN = Random.Range(0, 100);

                if (randN < _currentEnforce.criticalChance)
                {
                    Damage = Damage * 1.5f;
                }

            }

            target.TakeDamage((int)Damage);

            _currentCoolDown = Mathf.Max(0.5f, _stat.SkillCoolDown - _currentEnforce.declineCooldown);

    }

    public void HealSelf()
    {
        if(_stat.isDead) // 플레이어가 사망한 상태인지 확인
        {
            return;
        }

        _animator.SetTrigger("tHeal"); // 애니메이션 출력하고

        float totalHeal = _stat.finalATK * 0.8f + _currentEnforce.BonusHeal;

        _stat.CurrentHP = Mathf.Min(_stat.finalHP, _stat.CurrentHP + totalHeal); // 체력 회복이 최대 체력을 넘지 않게 하고

        if(_HpSlider != null) // 슬라이드 체력 바 동기화
        {
            _HpSlider.value = _stat.CurrentHP;
        }

        _currentCoolDown = Mathf.Max(0.5f, _stat.SkillCoolDown - _currentEnforce.declineCooldown); // 쿨타임

    }

    public void Resurrection()
    {
        if (_stat.CurrentHP <= 0) // 플레이어가 체력이 0이라면
        {
            _currentEnforce.playerresurrection = false; // 부활 가능한 상태를 불가능으로 바꾸고
            _stat.CurrentHP = _stat.finalHP / 2f; // 플레이어의 체력을 최대 체력의 절반으로 만든다.

            if (_HpSlider != null) // 슬라이드 체력 바 동기화
            {
                _HpSlider.value = _stat.CurrentHP;
            }
        }
    }



    private void OnMouseDown()
    {
        if(Time.timeScale == 0 )
        {
            return;
        }

        FindObjectOfType<BattleManager>()?.SelectTarget(this);   
    }

    public bool GainEXP(int totalEXP)
    {
        _currentEXP += totalEXP;

        if( _currentEXP >= _MaxEXP) // 레벨업 했을 시
        {
            _level++;
            _currentEXP -= _MaxEXP;
            _MaxEXP = _MaxEXP * _level * 1.2f;
            return true;
        }

        return false;
    }

    public int GetTargetCount()
    {
        return _currentEnforce.targetCount + 1;
    }


    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();

        
    }

    protected virtual void Update()
    {
        if(_currentCoolDown > 0f)
        {
            _currentCoolDown -= Time.deltaTime;
        }
    }

    
}

public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsDead { get; }

}

public interface IAttackable
{
    void Attack(IDamageable target);
    int ATK { get; }

}

public interface IDeath
{
    void OnDeath();

}
