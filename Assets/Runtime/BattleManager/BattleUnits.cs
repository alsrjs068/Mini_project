using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum SkillStat
{
    ATK,
    HP,
    SkillCoolDown,
    BonusDamage
}


public abstract class BattleUnits : MonoBehaviour, IDamageable, IAttackable // 추상화, 캡슐화, 인터페이스 사용하기
{
    // 인스펙터
    

    




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

    }

    
    public virtual void TakeDamage(int attackerATK)
    {
        float Damage = attackerATK * (100f / (100f + _stat.DFS));

        _stat.CurrentHP = _stat.CurrentHP - Damage;

        _animator.SetTrigger("tHit");

        if (_stat.CurrentHP <= 0)
        {
            _stat.CurrentHP = 0;

            _animator.SetTrigger("tDie");

            _stat.isDead = true;
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
    

    public abstract IEnumerator TurnExchange();

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
        _currentEnforce.declineCooldown -= newEnforce.declineCooldown;
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

        if (_currentCoolDown <= 0)
        {
            _animator.SetTrigger("tHeal");


            float Damage = _stat.finalATK * 1.5f * (100f / (100f + _stat.DFS)) + _currentEnforce.damageEnforce;
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


            _stat.CurrentHP = Mathf.Min(_stat.finalHP, _stat.CurrentHP + _currentEnforce.BonusHeal); // 플레이어 체력 회복

            _currentCoolDown = _stat.SkillCoolDown - _currentEnforce.declineCooldown;

        }

        else
        {
            return;
        }


    }

    private void OnMouseDown()
    {
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
