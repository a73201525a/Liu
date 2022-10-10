using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterMove
{

    /// <summary>
    /// �������
    /// </summary>
    private MonsterBase monster;
    private NavMeshAgent agent;



    public MonsterMove(MonsterBase monster)
    {
        this.monster = monster;
        agent = monster.transform.gameObject.GetComponent<NavMeshAgent>();
        if (!agent.isOnNavMesh)
        {
            SetMonsterNavigation(monster.transform.position);
        }
        Debug.Log("monster.monsterData.speed::" + monster.monsterData.speed);
        agent.speed = monster.monsterData.speed / 10;
        agent.angularSpeed = monster.monsterData.turnSpeed;
    }
    /// <summary>
    /// ��ʼ�ƶ�
    /// </summary>
    public void StartMove()
    {
        agent.isStopped = false;
    }
    /// <summary>
    /// ֹͣ�ƶ�
    /// </summary>
    public void StopMove()
    {
        agent.isStopped = true;
    }

    /// <summary>
    /// �Ƿ񵽴��ƶ�λ��
    /// </summary>
    /// <returns></returns>
    public bool RemainingDistance()
    {
        //Debug.Log("agent.remainingDistance::" + agent.remainingDistance);
        return agent.remainingDistance <= 0.5f;
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="pos">�ƶ��������</param>
    /// 
    public void Move(Vector3 pos, float stopdis = 1f)
    {
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Run);
        agent.isStopped = false;
        agent.stoppingDistance = stopdis;
        agent.SetDestination(pos);
    }
  
    Vector3 moveDirectory = Vector3.zero;//��ת�Ƕ�
    /// <summary>
    /// ת�����ʱ�� ����ת��
    /// </summary>
    /// <param name="dir"></param>
    public int Turn(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.x, dir.z);

        angle = angle * Mathf.Rad2Deg;

        moveDirectory.y = angle;

        Quaternion lookQuaternion = Quaternion.Euler(moveDirectory);
        float turnSpeed;

        turnSpeed = monster.monsterData.turnSpeed;

        monster.transform.rotation = Quaternion.RotateTowards
           (monster.transform.rotation, lookQuaternion, turnSpeed * Time.deltaTime);
        //Debug.Log((turnSpeed * Time.deltaTime));
        //Debug.Log("turnSpeed * Time.deltaTime::" + (turnSpeed * Time.deltaTime)/ angle);
        return Mathf.Abs((int)(angle / turnSpeed * 1000));
    }
    /// <summary>
    /// �����ƶ���
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public void SetMonsterNavigation(Vector3 pos)
    {
        agent.Warp(pos);
    }

}
