
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 怪物类型
/// </summary>
public enum MonsterType
{
    LittleMonster,//小怪
    Boss,//Boss
}
/// <summary>
/// 怪物状态
/// </summary>
public enum MonsterStateType
{
    None,
    Idle,//idle
    RoadPatrol,//巡逻
    Fight,//战斗状态
    Attack,//攻击
    Hurt,//受伤
}
public class MonsterData
{
    public int X;//格子X
    public int Y;//格子Y
    public AttackData attackData;
    /// <summary>
    /// 怪物血量
    /// </summary>
    public int MaxHp;
    /// <summary>
    /// 怪物当前血量
    /// </summary>
    public int CurHp;
    /// <summary>
    /// 防御
    /// </summary>
    public int Defence;
    /// <summary>
    /// 元素防御
    /// </summary>
    public int[] ElementDefence;

    ///// <summary>
    ///// 移动速度
    ///// </summary>
    public float speed;//移速
    public float turnSpeed;//角速度
    public float sprintSpeed;//冲刺速度
    /// <summary>
    ///// 攻击距离
    ///// </summary>
    public int attackRange;
    public int attackSpeed;//攻速
    public int level;//等级

    public int SkillLevel;
    /// <summary>
    /// 子弹id
    /// </summary>
    public int bulletID;
    /// <summary>
    /// 怪物类型
    /// </summary>
    public MonsterType monsterType;
    public BulletCampType bulletCampType;
    /// <summary>
    /// 怪物配置
    /// </summary>
    internal MonsterCfgTable monsterCfg;
    internal MonsterData(MonsterCfgTable monsterCfg, int level)
    {
        this.monsterCfg = monsterCfg;
        bulletCampType = BulletCampType.MonsterBullet;
        //  this.bulletID = monsterCfg.bulletData;
        monsterType = (MonsterType)monsterCfg.monsterType;
        this.level = level;
        CreatAttackData();
        attackRange = monsterCfg.attackRange;
        this.SkillLevel = monsterCfg.skillLevel;


        //speed = monsterCfg.speed;
        //turnSpeed = monsterCfg.turnSpeed;
        //sprintSpeed = monsterCfg.sprintSpeed;
        //attackRange = monsterCfg.attackRange;
        //attackSpeed = monsterCfg.attackSpeed;

    }
    /// <summary>
    /// 根据等级 和品质 计算伤害值
    /// </summary>
    public void CreatAttackData()
    {
        // 攻击=基础攻击+基础攻击X 品质X 当前等级
        int attack = Mathf.RoundToInt(monsterCfg.attack * monsterCfg.quality * level);
        MaxHp = monsterCfg.hp;
        CurHp = MaxHp;
        Defence = Mathf.RoundToInt(monsterCfg.defence * monsterCfg.quality * level);
        //  ElementDefence = monsterCfg.elementDefence;
        this.attackData = new AttackData(attack);


    }
    /// <summary>
    /// 读取bulletJson
    /// </summary>
    /// <param name="bulletID"></param>
    /// <returns></returns>
    private BulletJson bulletJson;
    public BulletJson curBulletJson
    {
        get
        {
            if (bulletJson == null)
            {
                bulletJson = Utility.DeserializationBullet(bulletID);
            }
            return bulletJson;
        }
    }

}

public class MonsterBase : NPCBase
{
    /// <summary>
    /// 怪物数据
    /// </summary>
    public MonsterData monsterData;
    //怪物索引
    public int monsterIndex;

    //怪物控制器
    public MonsterContorller monsterContorller;



    public bool Stop;//是否暂停游戏



    //public Transform monsterFirePoint;//开火点
    //public MMFeedbacks fireEffect;//开枪效果

    //死亡动画时长
    public int DeathAnimationTime = 1500;
    //攻击目标
    public GameObject attackTarget;


    //gridAi
    public MonsterGridAI monsterGridAI;
    //移动控制
    // public MonsterMove moveControl;

    //动画组件
    public MonsterAnimation monsterAnimation;

    //当前怪物状态
    public MonsterStateType curStausType;

    //配置id
    public int cfgID;

    ////怪物组合
    //public MonsterBase[] monsterArray;
    /// <summary>
    /// 怪物死亡后携带的奖励
    /// </summary>
    // public int[] monsterRewardArr;
    /// <summary>
    /// Buff控制
    /// </summary>
    //private BuffContorller BuffTool;

    ////怪物位置站位偏移点
    //public Vector3 offset = Vector3.zero;

    ///// <summary>
    ///// 模型mesh
    ///// </summary>
    private GameObject modelMesh;
    private int modelMeshIndex = 0;//角色模型mesh索引

    public bool enterPointer;//鼠标是否进入怪物


    /// <summary>
    /// 
    /// </summary>
    /// <param name="monsterID">怪物id</param>
    /// <param name="buildIndex">归属建筑</param>
    /// <param name="patrolCenter">寻罗中心点</param>
    /// <param name="monster">怪物物品</param>
    /// <param name="startPoint">初始位置</param>

    public MonsterBase(int monsterID, int level, GameObject monster, int UniqueID, Vector3 startPoint)
    {
        MonsterCfgTable monsterCfg = MonsterCfgTable.Instance.Get(monsterID);

        this.cfgID = monsterID;
        NpcID = UniqueID;

        this.transform = monster.transform;

        // this.monsterRewardArr = monsterCfg.rewardItemArray;

        //buff模型初始
        CreatBuff();

        isDeath = false;
        monsterData = new MonsterData(monsterCfg, level);

        UpdateLocation((int)startPoint.x, (int)startPoint.z);

        MapMsg mapMsg = new MapMsg(monsterData.X, monsterData.Y, true, (ushort)ILNpcEventMap.SetObs);
        SendMsg(mapMsg);
        //monsterFirePoint = monsterGameObject.transform.Find("FirePoint");

        //fireEffect = monsterGameObject.transform.Find("ShootFeedback/ShootFeedback").GetComponent<MMFeedbacks>();
        //动画控制
        monsterAnimation = new MonsterAnimation(this);

        //移动控制初始化
        //  moveControl = new MonsterMove(this);
        //ai控制初始化 
        curStausType = MonsterStateType.Idle;

        monsterGridAI = new MonsterGridAI(this, monsterCfg);

        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.CreatHUDBlood, transform, monsterData.CurHp, monsterData.MaxHp, UniqueID);
        SendMsg(hUDMsg);

        ILMonoBehaviour mono = transform.gameObject.AddComponent<ILMonoBehaviour>();



        mono.OnUpdate += Update;
        mono.OnAnimatorAction += monsterAnimation.AnimationEvent;

        mono.EventEnter += PointEnter;
        mono.EventExit += PointExit;
    }
    /// <summary>
    /// Buff操作
    /// </summary>
    private void CreatBuff()
    {

        Transform buffComopent = transform.Find("BuffCompent");

        buffArray = new GameObject[buffComopent.childCount];
        for (int i = 0; i < buffComopent.childCount; i++)
        {
            buffArray[i] = buffComopent.Find(i.ToString()).gameObject;
        }
        modelMesh = transform.GetChild(modelMeshIndex).gameObject;
    }

    private void Update()
    {
        if (monsterContorller.MonsterPointEntrer || monsterContorller.monsterRound == RoundType.Monster)
        {
            return;
        }
        if (enterPointer)
        {
            if (Input.GetMouseButtonDown(1))
            {
                //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                //{
                //    return;
                //}


                DeatailMsg deatailMsg1 = new DeatailMsg((ushort)ILUIEventPlayerPanel.ShowMonsterDetail, this);
                SendMsg(deatailMsg1);

                ShowGird();
                //  Debug.Log("================================================");
            }
        }

    }
    /// <summary>
    /// 显示攻击范围格子
    /// </summary>
    private async void ShowGird()
    {
        await Task.Delay(100);//避免玩家反键操作误点击怪物
        SendMsg((ushort)ILNpcEventMap.HideAttckTip);
        HotMapController.Instance.GetAttackTip(monsterData.X, monsterData.Y, monsterData.attackRange, monsterGridAI.attackTipType);
        SendMsg((ushort)ILNpcEventMap.ShowAttackTip);
    }
    #region Buff
    /// <summary>
    /// 中毒
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void Poison(Buff tmpBuff, bool isStart)
    {

        base.Poison(tmpBuff);
        if (isStart)
        {
            return;
        }

        SkillCfgTable skillCfg = SkillCfgTable.Instance.Get((int)tmpBuff.buff);

        AttackData attackData = new AttackData(tmpBuff.attack);
        BeHurt(attackData, RoundType.Buff, false, isStart, tmpBuff.CasterID);

    }
    /// <summary>
    /// 眩晕播放动画
    /// </summary>
    /// <param name="tmpBuff">buff</param>
    /// <param name="isAddBuff">是否添加buff</param>
    public override void Vertigo(Buff tmpBuff, bool isAddBuff)
    {
        base.Vertigo(tmpBuff, isAddBuff);
        monsterAnimation.SetAnimation(MonsterAnimationType.Vertigo);
    }
    /// <summary>
    /// 移除眩晕
    /// </summary>
    public override void RemoveVertigo(Buff tmpBuff)
    {
        base.RemoveVertigo(tmpBuff);
        monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
    }
    /// <summary>
    ///变羊
    /// </summary>
    /// <param name="tmpBuff">buff数据</param>
    /// <param name="isAddBuff">是否添加buff</param>
    public override void DeformationSheep(Buff tmpBuff, bool isAddBuff)
    {
        base.DeformationSheep(tmpBuff, isAddBuff);
        modelMesh.SetActive(false);
        buffArray[tmpBuff.buffIndex].GetComponentInChildren<ParticleSystem>().Play();

    }
    //移除变羊
    public override void RemoveDeformationSheep(Buff tmpBuff)
    {
        base.RemoveDeformationSheep(tmpBuff);

        buffArray[tmpBuff.buffIndex].GetComponentInChildren<ParticleSystem>().Play();

        modelMesh.SetActive(true);

    }
    /// <summary>
    /// 添加攻击力buff
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void AddAttack(Buff tmpBuff)
    {
        base.AddAttack(tmpBuff);
        monsterData.attackData.AddAttack += (tmpBuff.attack / 100f);
        Debug.Log("添加攻击力buff::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// 移除攻击力buff
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveAddAttack(Buff tmpBuff)
    {
        base.RemoveAddAttack(tmpBuff);
        monsterData.attackData.AddAttack -= (tmpBuff.attack / 100f);
        Debug.Log("移除攻击力buff::" + monsterData.attackData.AddAttack);

    }

    /// <summary>
    ///加防
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void AddDefence(Buff tmpBuff)
    {
        base.AddDefence(tmpBuff);
        monsterData.attackData.AddDefence += (tmpBuff.defence / 100f);
        Debug.Log("加防::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    ///  移除加防效果
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveDefence(Buff tmpBuff)
    {
        base.RemoveDefence(tmpBuff);
        monsterData.attackData.AddDefence -= (tmpBuff.defence / 100f);
        Debug.Log(" 移除加防效果::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    /// 减攻
    /// </summary>
    /// <param name="tmpBuff"></param>
    /// <returns></returns>
    public override void SubAttack(Buff tmpBuff)
    {
        base.SubAttack(tmpBuff);
        monsterData.attackData.AddAttack -= (tmpBuff.attack / 100f);
        Debug.Log("减攻::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// 移除减攻
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveSubAttack(Buff tmpBuff)
    {
        base.RemoveSubAttack(tmpBuff);
        monsterData.attackData.AddAttack += (tmpBuff.attack / 100f);
        Debug.Log("  移除减攻::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// 减防
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void SubDefence(Buff tmpBuff)
    {
        base.SubDefence(tmpBuff);
        monsterData.attackData.AddDefence -= (tmpBuff.defence / 100f);
        Debug.Log("减防::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    /// 移除减防
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveSubDefence(Buff tmpBuff)
    {
        base.RemoveSubDefence(tmpBuff);
        monsterData.attackData.AddDefence += (tmpBuff.defence / 100f);
        Debug.Log("   移除减防::" + monsterData.attackData.AddDefence);

    }

    #endregion

    /// <summary>
    /// 设置怪物控制
    /// </summary>
    /// <param name="monsterContorller"></param>
    public void SetMosnterControl(MonsterContorller monsterContorller)
    {
        this.monsterContorller = monsterContorller;
    }
    //更新格子位置
    public void UpdateLocation(int X, int Y)
    {
        monsterData.X = X;
        monsterData.Y = Y;
        transform.position = HotMapController.Instance.GetMapGridPos(X, Y);

    }
    /// <summary>
    /// 战斗时添加buff
    /// </summary>
    /// <param name="attackData"></param>  
    /// <param name="playerID">玩家ID</param>
    void AttackAddBuff(AttackData attackData, int playerID)
    {
        GameBuffType buff = NumericalTool.BuffCalculations(attackData);
        if (buff != GameBuffType.Null)
        {
            BuffMsg buffMsg = new BuffMsg((ushort)ILNpcBuff.ActiveBuff, buff, this, playerID, 1);
            SendMsg(buffMsg);
        }
    }

    /// <summary>
    /// 怪物受伤
    /// </summary>
    /// <param name="attackData">攻击数据</param>
    /// <param name="roundType">回合类型</param>
    /// <param name="isBuff">是否是buff</param>
    /// <param name="playerID">当前玩家对象id</param>
    public async void BeHurt(AttackData attackData, RoundType roundType, bool isCounter, bool isBuff, int playerID)
    {

        if (isBuff) AttackAddBuff(attackData, playerID);

        int damage = NumericalTool.DamageCalculations(attackData, monsterData.Defence, monsterData.ElementDefence);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.ReduceBlood, NpcID, damage);
        SendMsg(hUDMsg);

        monsterData.CurHp -= damage;
        if (monsterData.CurHp <= 0)
        {
            Death();
        }
        int waitTime = 500;

        if (isDeath)//死了领取怪物奖励
        {
            waitTime = 3000;
        }
        await Task.Delay(waitTime);

        if (isDeath)//怪物死了
        {
            monsterContorller.RemoveMonster(this);

            int[] tmp = MonsterCfgTable.Instance.Get(cfgID).rewardItemArray;

            if (tmp != null && tmp.Length > 0)
            {
                monsterContorller.MonsterDeathReward(tmp, roundType, playerID);//怪物领取奖励
                return;
            }
            else
            {
                if (roundType == RoundType.Player)
                {
                    PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, playerID);//玩家回合结束
                    SendMsg(playerMsg);
                    return;
                }
            }


        }

        if (!isCounter)
        {
            switch (roundType)
            {
                case RoundType.Player:

                    PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, playerID);//玩家回合结束
                    SendMsg(playerMsg);
                    break;
                case RoundType.Monster:
                    SendMsg((ushort)ILNpcEvent.MonsterStartRound);
                    break;
                default:
                    break;
            }
        }

    }

    /// <summary>
    /// 由技能造成伤害
    /// </summary>
    /// <param name="skillID">技能id</param> 
    /// <param name="roundType">回合类型</param>
    /// <param name="playerID">施法者</param>
    /// <returns></returns>
    public async void BeBySkill(SkillItem skillID, RoundType roundType, int TheCaster)
    {
        skillID.Perform(this, false, TheCaster);

        if (monsterData.CurHp <= 0)
        {
            Death();
        }
        int waitTime = 500;//等待时长

        if (isDeath)//死了领取怪物奖励
        {
            waitTime = 3000;
        }
        await Task.Delay(waitTime);
        if (isDeath)//死了领取怪物奖励
        {
            monsterContorller.RemoveMonster(this);
            int[] tmp = MonsterCfgTable.Instance.Get(cfgID).rewardItemArray;
            if (tmp.Length > 0)
            {
                monsterContorller.MonsterDeathReward(tmp, roundType, TheCaster);//怪物领取奖励
                return;
            }


        }

        //没死
        switch (roundType)
        {
            case RoundType.Player://玩家回合结束

                PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, TheCaster);//玩家回合结束
                SendMsg(playerMsg);
                break;
            case RoundType.Monster://怪物回合结束
                SendMsg((ushort)ILNpcEvent.MonsterStartRound);
                // monsterGridAI.EndRound();
                break;
            default:
                break;
        }


    }
    /// <summary>
    /// 鼠标进入事件
    /// </summary>
    /// <param name="eventData"></param>
    void PointEnter(GameObject obj)
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        enterPointer = true;
        ILUIManager.ShowView(UIPanelPath.DetailPanel, UILevel.Top, ILUIEventPanel.DetailPanel);
        DeatailMsg deatailMsg = new DeatailMsg((ushort)ILUIEvent.CreatDetailMosnterItem, this);
        SendMsg(deatailMsg);
    }

    /// <summary>
    ///  鼠标离开事件
    /// </summary>
    /// <param name="eventData"></param>
    void PointExit(GameObject obj)
    {
        enterPointer = false;
        ILUIManager.ShowView(UIPanelPath.DetailPanel, UILevel.Top, ILUIEventPanel.DetailPanel);
        DeatailMsg deatailMsg = new DeatailMsg((ushort)ILUIEventPlayerPanel.HideMonsterCommon);
        SendAwaitMsg(deatailMsg);
    }



    public int attackSpeed = 600;//攻速
    public int turnTime = 0;//转身所需时间
    public bool Fireing = false;//是否在开火中
    /// <summary>
    /// 开火动作
    /// </summary>
    public virtual async void Fire()
    {
        Fireing = true;
        monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
        Vector3 dir = attackTarget.transform.position - transform.position;
        tmpPlayerPos = attackTarget.transform.position;
        tmpMonsterPos = transform.position;
        // int time = moveControl.Turn(dir);
        // turnTime = time;
        // while (true)
        // {
        //moveControl.Turn(dir);
        //  await Task.Delay(1);
        //  time -= (int)(Time.deltaTime * 1000);
        // if (time <= 0)
        // {
        //   break;
        //   }
        // }
        monsterAnimation.SetAnimation(MonsterAnimationType.Shoot);
        await Task.Delay(attackSpeed);
        Fireing = false;
    }

    Vector3 tmpPlayerPos;
    Vector3 tmpMonsterPos;

    /// <summary>
    /// 发射子弹
    /// </summary>
    public virtual void Attack()
    {
        Debug.Log(" monster Attack");

        //投掷类型武器才需要计算落点，瞄准类型的武器需要确定瞄准方向
        if (monsterData.curBulletJson.areaType == SkillAreaType.OuterCircle_InnerCube ||
            monsterData.curBulletJson.areaType == SkillAreaType.OuterCircle_InnerSector)
        {

            tmpPlayerPos = tmpPlayerPos - tmpMonsterPos;
        }

        BulletMsg bulletMsg = new BulletMsg((ushort)ILNpcEvent.CreartBullet, monsterData.curBulletJson,
            tmpPlayerPos, tmpMonsterPos, monsterData.bulletCampType, monsterData.attackData);
        SendMsg(bulletMsg);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Death()
    {
        //  moveControl.StopMove();
        // UpdateLocation(monsterData.X, monsterData.Y);
        isDeath = true;
        monsterAnimation.SetAnimation(MonsterAnimationType.Deadth);
        transform.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(NpcID);
        writer.Write(cfgID);
        writer.Write(monsterData.CurHp);
        writer.Write(monsterData.X);
        writer.Write(monsterData.Y);
        writer.Write(isDeath);
    }

    public void Load()
    {

    }

    public override void OnDestroy()
    {
        monsterGridAI.Destory();
        monsterData = null;

        monsterAnimation = null;
        //moveControl = null;
        monsterGridAI = null;
        buffArray = null;

    }

}


