using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public abstract class BattleUnits : MonoBehaviour // 추상화, 캡슐화 사용하기
{
    protected Animator _animator;
    protected Stat.Unit _stat;

    private int _level = 1;
    private int _currentEXP = 0;
    private int _MaxEXP = 100;
    private float _currentCoolDown = 0;

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

    public abstract IEnumerable TurnExchange();

    public struct HealEnforce
    {
        public int targetCount;
        public float damageEnforce;
        public int bonusShield;
        public float declineCooldown;
        public bool playerresurrection;
        public bool isCritical;
        public int bonusDamage;

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

            float Damage = _stat.ATK * 1.5f * (100f / (100f + _stat.DFS)) + 50;
            // 언데드 몬스터에게 성속성 힐 공격 -> 1.5배 추가 데미지 + 추가 고정 데미지

            target.TakeDamage((int)Damage);


            _stat.CurrentHP = Mathf.Min(_stat.MaxHP, _stat.CurrentHP + 50); // 플레이어 체력 회복

            _currentCoolDown = _stat.SkillCoolDown;

        }


    }

    private void OnMouseDown()
    {
        FindObjectOfType<BattleManager>()?.SelectTarget(this);   
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
