using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MonsterAI
{
    //�������
    MonsterBase monster;

    //Ѳ�����ĵ�
    public Vector3 patrolCenter;
    /// <summary>
    /// ƫ����
    /// </summary>
    Vector3 offset;

    //Ѳ�߷�Χ
    float patrolRange = 4f;


    public MonsterAI(MonsterBase monster, Vector3 patrolCenter)
    {
        this.monster = monster;
        this.patrolCenter = new Vector3(patrolCenter.x, monster.transform.position.y, patrolCenter.z);

       // monster.moveControl.Move(this.patrolCenter);
        oldPlayerPos = Vector3.zero;
    }

   

    /// <summary>
    ///����״̬
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
    /// ��·Ѳ��
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
    /// ս��״̬
    /// </summary>
    public void Fight()
    {

     
        if (monster.Fireing)
        {
            return;
        }
       
        float distance = Vector3.Distance(monster.transform.position, monster.attackTarget.transform.position);

        //Ѱ��player
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
    /// ����ս��״̬
    /// </summary>
    public void Attack()
    {
        monster.Fire();
        //await Task.Delay(monster.attackSpeed + monster.turnTime + 200);

    }

    /// <summary>
    /// ����
    /// </summary>
    /// <returns></returns>
    public void Hurt()
    {


    }


    /// <summary>
    /// ��������
    /// </summary>
    public void Destroy()
    {
        monster = null;
    }
}
