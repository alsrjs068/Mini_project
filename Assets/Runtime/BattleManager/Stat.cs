using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Stat : MonoBehaviour
{
    

    public struct Unit
    {
        public string name;
        public string species;
        public bool boss;
        public bool isDead;
        public float MaxHP;
        public float CurrentHP;
        public int ATK;
        public int DFS;
        public float SkillCoolDown;
        public int ExpReward;

        public int BonusATK;
        public int BonusDFS;
        public float BonusHP;
        public int BonusDamage;

        public int finalATK => ATK + BonusATK;
        public int finalDFS => DFS + BonusDFS;
        public float finalHP => MaxHP + BonusHP;
        

        public void PrintStatusP()
        {
            CPrint.Log($"이름 : {name} / 종족 : {species} / 체력 : {finalHP} / 공격력 : {finalATK} / 방어력 : {finalDFS}");
        }

        public void PrintStatsM()
        {
            CPrint.Log($"이름 : {name} / 종족 : {species} / 체력 : {MaxHP} / 공격력 : {ATK} / 방어력 : {DFS}");
        }
    }
    

    public static Unit CreatePlayer(int level)
    {
        Unit player = new Unit();
        player.name = "플레이어";
        player.species = "인간";
        player.isDead = false;
        player.MaxHP = 300.0f + (level - 1) * 100;
        player.CurrentHP = 300.0f + (level - 1) * 100;
        player.ATK = 70 + (level - 1) * 20;
        player.DFS = 45 + (level - 1) * 10;
        player.SkillCoolDown = 10.0f;

        player.PrintStatusP();

        return player;

    }

    public static Unit CreateSkelleton(int currentstage)
    {
        Unit skelleton = new Unit();
        skelleton.name = "스켈레톤";
        skelleton.species = "언데드";
        skelleton.boss = false;
        skelleton.isDead = false;
        skelleton.MaxHP = 100.0f + (currentstage * 50);
        skelleton.CurrentHP = 100.0f + (currentstage * 50);
        skelleton.ATK = 30 + (currentstage * 10);
        skelleton.DFS = 20 + (currentstage * 10);
        skelleton.ExpReward = 75 + (currentstage * 20);


        skelleton.PrintStatsM();

        return skelleton;
    }

    public static Unit CreateZombie(int currentstage) // 좀비는 스켈레톤과 다르게 피부라는 1차 방어막 존재 -> 스켈레톤보다 방어력 높게 설계
    {
        Unit Zombie = new Unit();
        Zombie.name = "좀비";
        Zombie.species = "언데드";
        Zombie.boss = false;
        Zombie.isDead = false;
        Zombie.MaxHP = 100.0f + (currentstage * 50);
        Zombie.CurrentHP = 100.0f + (currentstage * 50);
        Zombie.ATK = 30 + (currentstage * 10);
        Zombie.DFS = 20 + (currentstage * 20);
        Zombie.ExpReward = 80 + (currentstage * 20);

        Zombie.PrintStatsM();

        return Zombie;
    }

    public static Unit MiddleBossStat(int currentstage)
    {
        Unit MiddleBoss = new Unit();
        MiddleBoss.name = "언데드 건";
        MiddleBoss.MaxHP = 500.0f;
        MiddleBoss.boss = true;
        MiddleBoss.isDead = false;
        MiddleBoss.CurrentHP = 500.0f;
        MiddleBoss.ATK = 80;
        MiddleBoss.DFS = 50;
        MiddleBoss.ExpReward = 500;


        return MiddleBoss;
    }

    public static Unit MainBossStat(int currentstage)
    {
        Unit MainBoss = new Unit();
        MainBoss.name = "언데드 킹";
        MainBoss.species = "언데드";
        MainBoss.MaxHP = 500.0f;
        MainBoss.boss = true;
        MainBoss.isDead = false;
        MainBoss.CurrentHP = 1000.0f;
        MainBoss.ATK = 100;
        MainBoss.DFS = 100;
        MainBoss.ExpReward = 1000;
        MainBoss.PrintStatsM();

        return MainBoss;
    }



    
  
}
