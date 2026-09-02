using System;
using System.Collections;
using System.Collections.Generic;
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
    

    public static Unit CreatePlayer()
    {
        Unit player = new Unit();
        player.name = "플레이어";
        player.species = "인간";
        player.isDead = false;
        player.MaxHP = 100.0f;
        player.CurrentHP = 100.0f;
        player.ATK = 70;
        player.DFS = 45;
        player.SkillCoolDown = 5.0f;

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
        skelleton.ExpReward = 50 + (currentstage * 20);


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
        Zombie.ExpReward = 60 + (currentstage * 20);

        Zombie.PrintStatsM();

        return Zombie;
    }

    public static void BossStat()
    {
        Unit Boss = new Unit();
        Boss.name = "언데드 킹";
        Boss.species = "언데드";
        Boss.PrintStatsM();
    }



    
  
}
