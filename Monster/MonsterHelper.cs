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
    /// ��������
    /// </summary>
    /// <param name="id">��������id</param> 
    /// /// <param name="level">����ȼ�</param>
    /// <param name="monster">����gameObject</param>
    /// <param name="player">���gameObject</param>
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
              //  Debug.Log("δ���øù���ID��" + id);
                break;

        }
        return monsterBase;
    }
}
