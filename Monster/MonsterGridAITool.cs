using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGridAITool
{
    /// <summary>
    /// 随机行为
    /// </summary>
    /// <param name="aiAttackType"></param>
    /// <param name="curSkillID"></param>
    /// <param name="practiceType"></param>
    public void IsPractice(MonsterBase monster, AIAttackType aiAttackType, out int curSkillID, out PracticeType practiceType)
    {
        curSkillID = 0;
        practiceType = 0;
        if (aiAttackType == AIAttackType.skill)
        {

            int[] skillID = monster.monsterData.monsterCfg.skillID;
            int[] gainSkillID = monster.monsterData.monsterCfg.gainSkillID;

            int isAorR = Random.Range(0, 2);
            Debug.Log("isAorR:::" + isAorR);
            if (isAorR == 1 && gainSkillID.Length > 0)//使用增益buff
            {
                curSkillID = gainSkillID[Random.Range(0, gainSkillID.Length)];
                practiceType = PracticeType.GainSkill;
            }

            if (isAorR == 0 && skillID.Length > 0)
            {
                curSkillID = skillID[Random.Range(0, skillID.Length)];
                practiceType = PracticeType.Skill;
            }
            int tmp = Random.Range(0, 6);
            Debug.Log("Random.Range(0, 6)::" + tmp);
            if (tmp >= 4)
            {
                practiceType = PracticeType.Attack;
            }
        }
        else
        {
            practiceType = PracticeType.Attack;
        }
    }

    Player lastPlayer;//上次的目标 记录一下
    public bool CheckTarget(List<Player> players, AIAttackType aiAttackType, SolderType solderType, MonsterBase monster, out Player player)//寻找目标
    {
        player = null;
        //复制一下玩家列表
        List<Player> tmpPlayers = new List<Player>();
        for (int j = 0; j < players.Count; j++)
        {
            Player tmpPlaer = players[j];
            if (!tmpPlaer.isDeath)
            {
                tmpPlayers.Add(tmpPlaer);
            }

        }

        if (aiAttackType == AIAttackType.weakness)
        {
            SolderType playerSolderType;
            if (aiAttackType == AIAttackType.enchanter)
            {
                playerSolderType = SolderType.enchanter;
            }
            else
            {
                playerSolderType = Utility.GetSolder(solderType);
            }
            Debug.Log("当前克制兵种::" + playerSolderType);


            Player tmpPlayer;//最近的player

            if (GetNearestPlayer(tmpPlayers, monster, out player))
            {
                tmpPlayer = player;
                // Debug.Log("tmpPlayers.Count::" + tmpPlayers.Count);
                while (tmpPlayers.Count > 0)
                {
                    //Debug.Log("playerType::dsdsd:::::" + player.playerData.playerJson.solderType);
                    if (player.playerData.playerJson.solderType == (int)playerSolderType)
                    {
                        Debug.Log("在范围内找了克制兵种");
                        return true;
                    }

                    tmpPlayers.Remove(player);
                    GetNearestPlayer(tmpPlayers, monster, out player);
                }
                //巡逻范围之内没有克制关系的怪物 攻击最近的怪物
                player = tmpPlayer;
                Debug.Log("没有克制关系：：" + player.transform.gameObject.name);
                Debug.Log("巡逻范围之内没有克制关系的怪物 攻击最近的玩家：：" + Utility.GetLanguage(player.playerData.playerJson.name));
                return true;
            }


            return false;
        }
        else
        {
            if (GetNearestPlayer(tmpPlayers, monster, out player))
            {
                Debug.Log("最近玩家：：" + player.transform.gameObject.name);
                Debug.Log("最近玩家名称：：" + Utility.GetLanguage(player.playerData.playerJson.name));

                if (lastPlayer != null && lastPlayer.NpcID == player.NpcID && !lastPlayer.isDeath)//避免一直攻击同一目标
                {
                    tmpPlayers.Remove(player);
                    if (!GetNearestPlayer(tmpPlayers, monster, out player))
                    {
                        player = lastPlayer;
                    }
                }
                lastPlayer = player;

                return true;
            }
            return false;

        }

    }

    Player lastSkillPlayer;
    MonsterBase lastSkillMonster;
    public bool CheckTargetSkill(List<Player> playerList, List<MonsterBase> monsterList, AIAttackType aiAttackType,
        PracticeType practiceType, MonsterBase monster, out Player player, out MonsterBase monsterTager)
    {
        player = null;
        monsterTager = null;
        Debug.Log("怪物这回合操作 practiceType：" + practiceType);
        if (practiceType == PracticeType.Skill)
        {

            List<Player> tmpPlayers = new List<Player>();
            for (int j = 0; j < playerList.Count; j++)
            {
                Player tmpPlaer = playerList[j];
                if (!tmpPlaer.isDeath)
                {
                    tmpPlayers.Add(tmpPlaer);
                }

            }

            if (GetNearestPlayer(tmpPlayers, monster, out player))
            {
                if (lastSkillPlayer != null && lastSkillPlayer.NpcID == player.NpcID && !lastSkillPlayer.isDeath)//避免一直攻击同一目标
                {
                    tmpPlayers.Remove(player);
                    if (!GetNearestPlayer(tmpPlayers, monster, out player))
                    {
                        player = lastSkillPlayer;
                    }
                }
                Debug.Log("怪物找到目标玩家：" + player.NpcID);
                lastSkillPlayer = player;

                return true;
            }
        }
        else if (practiceType == PracticeType.GainSkill)
        {

            List<MonsterBase> tmpMonsters = new List<MonsterBase>();
            for (int j = 0; j < monsterList.Count; j++)
            {
                MonsterBase tmpPlaer = monsterList[j];
                if (!tmpPlaer.isDeath)
                {
                    tmpMonsters.Add(tmpPlaer);
                }

            }

            if (GetNearestMonster(tmpMonsters, monster, out monsterTager))
            {

                if (lastSkillMonster != null && lastSkillMonster.NpcID == monsterTager.NpcID && !lastSkillMonster.isDeath)//避免一直攻击同一目标
                {
                    tmpMonsters.Remove(monsterTager);
                    if (!GetNearestMonster(tmpMonsters, monster, out monsterTager))
                    {
                        monsterTager = lastSkillMonster;
                    }
                }

                lastSkillMonster = monsterTager;
                Debug.Log("怪物找到目标怪物 isDeath：" + monsterTager.isDeath);
                Debug.Log("怪物找到目标怪物：" + monsterTager.NpcID);
                Debug.Log("怪物找到目标怪物 lastSkillMonster：" + lastSkillMonster);
                return true;
            }
        }

        return false;
    }


    public bool CheckBuildTarget(List<BuildItemBase> buildList, AIAttackType aiAttackType, SolderType solderType, MonsterBase monster, out BuildItemBase build)//寻找目标
    {
        build = null;
        for (int i = 0; i < buildList.Count; i++)
        {
            if (buildList[i].IsBoss && (buildList[i].buildAttType == BuildAttType.player || buildList[i].buildAttType == BuildAttType.neutral))
            {
                build = buildList[i];
                return true;
            }
        }
        if (build == null)
        {
            for (int i = 0; i < buildList.Count; i++)
            {
                if (buildList[i].buildAttType == BuildAttType.player)
                {
                    build = buildList[i];
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 获取最近玩家
    /// </summary>
    /// <param name="player"></param>
    public bool GetNearestPlayer(List<Player> tmpList, MonsterBase monster, out Player player)
    {
        player = null;
        int lastDis = 9999;

        if (tmpList.Count > 0)
        {

            for (int i = 0; i < tmpList.Count; i++)
            {
                int Lx = Mathf.Abs(monster.monsterData.X - tmpList[i].playerData.MapIndexX);
                int Ly = Mathf.Abs(monster.monsterData.Y - tmpList[i].playerData.MapIndexY);
                int minDis = Lx + Ly;

                if (minDis < lastDis)
                {
                    player = tmpList[i];
                    lastDis = minDis;
                }
            }

            Debug.Log("找到最近的玩家:" + player.NpcID);

        }
        if (player == null)
        {
            return false;
        }
        int x = Mathf.Abs(monster.monsterData.X - player.playerData.MapIndexX);
        int y = Mathf.Abs(monster.monsterData.Y - player.playerData.MapIndexY);
        if (x > monster.monsterGridAI.patrolRange || y > monster.monsterGridAI.patrolRange)
        {
            Debug.Log("在巡逻范围之内没有怪物");
            return false;
        }
        if (player != null)
        {
            return true;
        }
        Debug.Log("没有找到Player");
        return false;
    }


    public bool GetNearestMonster(List<MonsterBase> tmpList, MonsterBase monster, out MonsterBase monsterTagetr)
    {
        monsterTagetr = null;
        int lastDis = 9999;

        if (tmpList.Count > 0)
        {

            for (int i = 0; i < tmpList.Count; i++)
            {
                if (tmpList[i].NpcID == monster.NpcID)
                {
                    continue;
                }
                int Lx = Mathf.Abs(monster.monsterData.X - tmpList[i].monsterData.X);
                int Ly = Mathf.Abs(monster.monsterData.Y - tmpList[i].monsterData.Y);
                int minDis = Lx + Ly;
                if (minDis < lastDis)
                {
                    Debug.Log("找到最近的怪物");
                    monsterTagetr = tmpList[i];
                    lastDis = minDis;
                }
            }
        }

        //int x = Mathf.Abs(monster.monsterData.X - monsterTagetr.monsterData.X);
        //int y = Mathf.Abs(monster.monsterData.Y - monsterTagetr.monsterData.Y);
        //if (x > monster.monsterGridAI.patrolRange || y > monster.monsterGridAI.patrolRange)
        //{
        //    Debug.Log("在巡逻范围之内没有怪物");
        //    return false;
        //}

        if (monsterTagetr != null)
        {
            return true;
        }
        return false;
    }


}
