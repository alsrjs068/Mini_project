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
    }


    public struct UnitStats
    {
        public float MaxHP;
        public float CurrentHP;
        public int ATK;
        public int DFS;
        public float SkillCoolDown;

    }


    static void PlayerStat()
    {
        Unit player = new Unit();
        player.name = "플레이어";
        player.species = "인간";
        player.boss = false;

        UnitStats playerstats= new UnitStats();
        playerstats.MaxHP = 100.0f;
        playerstats.CurrentHP = 100.0f;
        playerstats.ATK = 70;
        playerstats.DFS = 45;
        playerstats.SkillCoolDown = 10.0f;

        CPrint.Log($"플레이어 이름 : {player.name}");
        CPrint.Log($"종족 : {player.species}");

    }

    static void SkelletonStat()
    {

    }

    static void ZombieStat()
    {

    }

    static void BossStat()
    {

    }



    
  
}
