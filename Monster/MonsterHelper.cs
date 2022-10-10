using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHelper
{

    public MonsterHelper()
    {
        //monsterParent = new GameObject("MonsterParent");
        //player = GameObject.Find("Player");
    }
    ///// <summary>
    ///// 
    ///// </summary>
    //public void SetCurrentValue()
    //{

    //}
    /// <summary>
    /// 怪物生成
    /// </summary>
    /// <param name="id">怪物配置id</param> 
    /// /// <param name="level">怪物等级</param>
    /// <param name="monster">怪物gameObject</param>
    /// <param name="player">玩家gameObject</param>
    /// <returns></returns>
    public MonsterBase CreatMonster(int id,int level,  GameObject monster, int uniqueID,Vector3 startPoint)
    {
        MonsterBase monsterBase = null;
        switch (id)
        {
            //case 70001:
            //    {
            //        monsterBase = new Monster10001(id, monsterIndex, buildindex, center, monster, uniqueID);
            //    }
            //    break;
            //case 70002:
            //    {
            //        monsterBase = new Monster10001(id, monsterIndex, buildindex, center, monster,  uniqueID);

            //    }
            //    break;
            //case 70003:
            //    {
            //        monsterBase = new Monster10001(id, monsterIndex, buildindex, center, monster, uniqueID);
            //    }
            //    break;
            default:
                monsterBase = new MonsterBase(id, level,monster, uniqueID, startPoint);
              //  Debug.Log("未配置该怪物ID：" + id);
                break;

        }
        return monsterBase;
    }
}
