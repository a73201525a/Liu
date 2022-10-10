using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterAnimationType
{
    Null,
    Run,//奔跑
    Idle,//待机
    Shoot,//攻击
    Attack,//近战攻击
    Deadth,//死亡
    Vertigo,//眩晕
    Repellent,//击退
    Charm,//魅惑
    SpeedCut,//减速
    Skill,//技能释放
}
public class MonsterAnimation
{
    class MonsterAnimationName
    {
        public const string Run = "IsRun";
        public const string Idle = "IsIdle";
        public const string Shoot = "IsShoot";
        public const string Deadth = "IsDeadth";  
        public const string Attack = "IsAttack";
        public const string Vertigo = "IsVertigo"; 
        public const string Skill = "IsSkill";

    }
    private MonsterBase monster;
    private Animator animatorContorller;
    public MonsterAnimationType CurState = MonsterAnimationType.Null;



    public MonsterAnimation(MonsterBase monster)
    {
        this.monster = monster;
        animatorContorller = monster.transform.gameObject.GetComponent<Animator>();

        //animatorContorller.speed = monster.monsterData.attackSpeed/10;

    }
    public void AnimationEvent(string animName)
    {
        Debug.Log("animName::" + animName);
        //动画攻击事件
        if (animName == "Fire")
        {
            monster.Attack();
            //  monster.UpdateState();
        }
    }

    public void SetAnimation(MonsterAnimationType type)
    {
        if (CurState == type && type != MonsterAnimationType.Shoot)
        {
            return;
        }
        if (CurState == MonsterAnimationType.Shoot)//开枪过后 恢复正常速度
        {
            animatorContorller.speed = 1;//恢复正常速度
        }
        switch (type)
        {
            case MonsterAnimationType.Run://奔跑
                SetBool(MonsterAnimationName.Idle, false);
                SetBool(MonsterAnimationName.Vertigo, false);
                SetBool(MonsterAnimationName.Run, true);
                break;
            case MonsterAnimationType.Idle://待机
                SetBool(MonsterAnimationName.Run, false); 
                SetBool(MonsterAnimationName.Vertigo, false);
                SetBool(MonsterAnimationName.Idle, true);
                break;
            case MonsterAnimationType.Shoot://攻击
                animatorContorller.speed = monster.monsterData.attackSpeed / 10;
                SetTrigger(MonsterAnimationName.Shoot);
                break; 
            case MonsterAnimationType.Attack://攻击
                SetTrigger(MonsterAnimationName.Attack);
                break;
            case MonsterAnimationType.Deadth://死亡
                SetBool(MonsterAnimationName.Deadth, true);
                SetBool(MonsterAnimationName.Run, false);
                SetBool(MonsterAnimationName.Idle, false);
                break;
            case MonsterAnimationType.Vertigo://眩晕
                SetBool(MonsterAnimationName.Idle, false);
                SetBool(MonsterAnimationName.Vertigo, true);
                break;
            case MonsterAnimationType.Charm://魅惑
                SetBool(MonsterAnimationName.Idle, true);
                break;
            case MonsterAnimationType.SpeedCut://减速
                SetBool(MonsterAnimationName.Idle, true);
                break;
            case MonsterAnimationType.Repellent://击退
                SetBool(MonsterAnimationName.Idle, true);
                break; 
            case MonsterAnimationType.Skill://击退
                SetTrigger(MonsterAnimationName.Skill);
                SetBool(MonsterAnimationName.Idle, true);
                break;
            default:
                Debug.Log($"没有此类型动画:{type}");
                break;
        }
        CurState = type;
       // Debug.Log("CurState:::::" + CurState);
    }
    //暂停动画
    public void StopAnimation()
    {
        animatorContorller.speed = 0;
    }

    public void StartAnimation()
    {
        animatorContorller.speed = 1;
    }

    void SetBool(string name, bool tmpBo)
    {
            animatorContorller.SetBool(name, tmpBo);
    }

    void SetTrigger(string name)
    {
            animatorContorller.SetTrigger(name);
    }

}