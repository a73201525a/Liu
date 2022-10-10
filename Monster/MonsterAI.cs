using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MonsterAI
{
    //怪物对象
    MonsterBase monster;

    //巡逻中心点
    public Vector3 patrolCenter;
    /// <summary>
    /// 偏移量
    /// </summary>
    Vector3 offset;

    //巡逻范围
    float patrolRange = 4f;


    public MonsterAI(MonsterBase monster, Vector3 patrolCenter)
    {
        this.monster = monster;
        this.patrolCenter = new Vector3(patrolCenter.x, monster.transform.position.y, patrolCenter.z);

       // monster.moveControl.Move(this.patrolCenter);
        oldPlayerPos = Vector3.zero;
    }

   

    /// <summary>
    ///更新状态
    /// </summary>
    public void Upadte()
    {
        switch (monster.curStausType)
        {
            case MonsterStateType.Idle:
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
                break;
            case MonsterStateType.RoadPatrol:
                //RoadPatrol();
                break;
            case MonsterStateType.Fight:
                Fight();
                break;
            case MonsterStateType.Hurt:
                Hurt();
                break;
        }
    }
    /// <summary>
    /// 道路巡逻
    /// </summary>
    public void RoadPatrol()
    {
        //if (monster.moveControl.RemainingDistance())
        //{
        //    float x = Random.Range(-patrolRange, patrolRange);
        //    float y = Random.Range(-patrolRange, patrolRange);
        //    offset = new Vector3(x, 0f, y);
        //    Vector3 point = patrolCenter + offset;
        //    monster.moveControl.Move(point);
        //}
    }

    Vector3 oldPlayerPos;
    /// <summary>
    /// 战斗状态
    /// </summary>
    public void Fight()
    {

     
        if (monster.Fireing)
        {
            return;
        }
       
        float distance = Vector3.Distance(monster.transform.position, monster.attackTarget.transform.position);

        //寻找player
        if (distance > monster.monsterData.attackRange)
        {
            if (Vector3.Distance(monster.attackTarget.transform.position, oldPlayerPos) > 2)
            {
                oldPlayerPos = monster.attackTarget.transform.position;
              //  monster.moveControl.Move(monster.attackTarget.transform.position);
            }
          
        }
        else
        {
            //monster.moveControl.StopMove();
            Attack();
        }
    }





    /// <summary>
    /// 怪物战斗状态
    /// </summary>
    public void Attack()
    {
        monster.Fire();
        //await Task.Delay(monster.attackSpeed + monster.turnTime + 200);

    }

    /// <summary>
    /// 受伤
    /// </summary>
    /// <returns></returns>
    public void Hurt()
    {


    }


    /// <summary>
    /// 结束调用
    /// </summary>
    public void Destroy()
    {
        monster = null;
    }
}
