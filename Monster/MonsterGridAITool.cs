using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGridAITool
{
    /// <summary>
    /// �����Ϊ
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
            if (isAorR == 1 && gainSkillID.Length > 0)//ʹ������buff
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

    Player lastPlayer;//�ϴε�Ŀ�� ��¼һ��
    public bool CheckTarget(List<Player> players, AIAttackType aiAttackType, SolderType solderType, MonsterBase monster, out Player player)//Ѱ��Ŀ��
    {
        player = null;
        //����һ������б�
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
            Debug.Log("��ǰ���Ʊ���::" + playerSolderType);


            Player tmpPlayer;//�����player

            if (GetNearestPlayer(tmpPlayers, monster, out player))
            {
                tmpPlayer = player;
                // Debug.Log("tmpPlayers.Count::" + tmpPlayers.Count);
                while (tmpPlayers.Count > 0)
                {
                    //Debug.Log("playerType::dsdsd:::::" + player.playerData.playerJson.solderType);
                    if (player.playerData.playerJson.solderType == (int)playerSolderType)
                    {
                        Debug.Log("�ڷ�Χ�����˿��Ʊ���");
                        return true;
                    }

                    tmpPlayers.Remove(player);
                    GetNearestPlayer(tmpPlayers, monster, out player);
                }
                //Ѳ�߷�Χ֮��û�п��ƹ�ϵ�Ĺ��� ��������Ĺ���
                player = tmpPlayer;
                Debug.Log("û�п��ƹ�ϵ����" + player.transform.gameObject.name);
                Debug.Log("Ѳ�߷�Χ֮��û�п��ƹ�ϵ�Ĺ��� �����������ң���" + Utility.GetLanguage(player.playerData.playerJson.name));
                return true;
            }


            return false;
        }
        else
        {
            if (GetNearestPlayer(tmpPlayers, monster, out player))
            {
                Debug.Log("�����ң���" + player.transform.gameObject.name);
                Debug.Log("���������ƣ���" + Utility.GetLanguage(player.playerData.playerJson.name));

                if (lastPlayer != null && lastPlayer.NpcID == player.NpcID && !lastPlayer.isDeath)//����һֱ����ͬһĿ��
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
        Debug.Log("������غϲ��� practiceType��" + practiceType);
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
                if (lastSkillPlayer != null && lastSkillPlayer.NpcID == player.NpcID && !lastSkillPlayer.isDeath)//����һֱ����ͬһĿ��
                {
                    tmpPlayers.Remove(player);
                    if (!GetNearestPlayer(tmpPlayers, monster, out player))
                    {
                        player = lastSkillPlayer;
                    }
                }
                Debug.Log("�����ҵ�Ŀ����ң�" + player.NpcID);
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

                if (lastSkillMonster != null && lastSkillMonster.NpcID == monsterTager.NpcID && !lastSkillMonster.isDeath)//����һֱ����ͬһĿ��
                {
                    tmpMonsters.Remove(monsterTager);
                    if (!GetNearestMonster(tmpMonsters, monster, out monsterTager))
                    {
                        monsterTager = lastSkillMonster;
                    }
                }

                lastSkillMonster = monsterTager;
                Debug.Log("�����ҵ�Ŀ����� isDeath��" + monsterTager.isDeath);
                Debug.Log("�����ҵ�Ŀ����" + monsterTager.NpcID);
                Debug.Log("�����ҵ�Ŀ����� lastSkillMonster��" + lastSkillMonster);
                return true;
            }
        }

        return false;
    }


    public bool CheckBuildTarget(List<BuildItemBase> buildList, AIAttackType aiAttackType, SolderType solderType, MonsterBase monster, out BuildItemBase build)//Ѱ��Ŀ��
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
    /// ��ȡ������
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

            Debug.Log("�ҵ���������:" + player.NpcID);

        }
        if (player == null)
        {
            return false;
        }
        int x = Mathf.Abs(monster.monsterData.X - player.playerData.MapIndexX);
        int y = Mathf.Abs(monster.monsterData.Y - player.playerData.MapIndexY);
        if (x > monster.monsterGridAI.patrolRange || y > monster.monsterGridAI.patrolRange)
        {
            Debug.Log("��Ѳ�߷�Χ֮��û�й���");
            return false;
        }
        if (player != null)
        {
            return true;
        }
        Debug.Log("û���ҵ�Player");
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
                    Debug.Log("�ҵ�����Ĺ���");
                    monsterTagetr = tmpList[i];
                    lastDis = minDis;
                }
            }
        }

        //int x = Mathf.Abs(monster.monsterData.X - monsterTagetr.monsterData.X);
        //int y = Mathf.Abs(monster.monsterData.Y - monsterTagetr.monsterData.Y);
        //if (x > monster.monsterGridAI.patrolRange || y > monster.monsterGridAI.patrolRange)
        //{
        //    Debug.Log("��Ѳ�߷�Χ֮��û�й���");
        //    return false;
        //}

        if (monsterTagetr != null)
        {
            return true;
        }
        return false;
    }


}
