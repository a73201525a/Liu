
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��������
/// </summary>
public enum MonsterType
{
    LittleMonster,//С��
    Boss,//Boss
}
/// <summary>
/// ����״̬
/// </summary>
public enum MonsterStateType
{
    None,
    Idle,//idle
    RoadPatrol,//Ѳ��
    Fight,//ս��״̬
    Attack,//����
    Hurt,//����
}
public class MonsterData
{
    public int X;//����X
    public int Y;//����Y
    public AttackData attackData;
    /// <summary>
    /// ����Ѫ��
    /// </summary>
    public int MaxHp;
    /// <summary>
    /// ���ﵱǰѪ��
    /// </summary>
    public int CurHp;
    /// <summary>
    /// ����
    /// </summary>
    public int Defence;
    /// <summary>
    /// Ԫ�ط���
    /// </summary>
    public int[] ElementDefence;

    ///// <summary>
    ///// �ƶ��ٶ�
    ///// </summary>
    public float speed;//����
    public float turnSpeed;//���ٶ�
    public float sprintSpeed;//����ٶ�
    /// <summary>
    ///// ��������
    ///// </summary>
    public int attackRange;
    public int attackSpeed;//����
    public int level;//�ȼ�

    public int SkillLevel;
    /// <summary>
    /// �ӵ�id
    /// </summary>
    public int bulletID;
    /// <summary>
    /// ��������
    /// </summary>
    public MonsterType monsterType;
    public BulletCampType bulletCampType;
    /// <summary>
    /// ��������
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
    /// ���ݵȼ� ��Ʒ�� �����˺�ֵ
    /// </summary>
    public void CreatAttackData()
    {
        // ����=��������+��������X Ʒ��X ��ǰ�ȼ�
        int attack = Mathf.RoundToInt(monsterCfg.attack * monsterCfg.quality * level);
        MaxHp = monsterCfg.hp;
        CurHp = MaxHp;
        Defence = Mathf.RoundToInt(monsterCfg.defence * monsterCfg.quality * level);
        //  ElementDefence = monsterCfg.elementDefence;
        this.attackData = new AttackData(attack);


    }
    /// <summary>
    /// ��ȡbulletJson
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
    /// ��������
    /// </summary>
    public MonsterData monsterData;
    //��������
    public int monsterIndex;

    //���������
    public MonsterContorller monsterContorller;



    public bool Stop;//�Ƿ���ͣ��Ϸ



    //public Transform monsterFirePoint;//�����
    //public MMFeedbacks fireEffect;//��ǹЧ��

    //��������ʱ��
    public int DeathAnimationTime = 1500;
    //����Ŀ��
    public GameObject attackTarget;


    //gridAi
    public MonsterGridAI monsterGridAI;
    //�ƶ�����
    // public MonsterMove moveControl;

    //�������
    public MonsterAnimation monsterAnimation;

    //��ǰ����״̬
    public MonsterStateType curStausType;

    //����id
    public int cfgID;

    ////�������
    //public MonsterBase[] monsterArray;
    /// <summary>
    /// ����������Я���Ľ���
    /// </summary>
    // public int[] monsterRewardArr;
    /// <summary>
    /// Buff����
    /// </summary>
    //private BuffContorller BuffTool;

    ////����λ��վλƫ�Ƶ�
    //public Vector3 offset = Vector3.zero;

    ///// <summary>
    ///// ģ��mesh
    ///// </summary>
    private GameObject modelMesh;
    private int modelMeshIndex = 0;//��ɫģ��mesh����

    public bool enterPointer;//����Ƿ�������


    /// <summary>
    /// 
    /// </summary>
    /// <param name="monsterID">����id</param>
    /// <param name="buildIndex">��������</param>
    /// <param name="patrolCenter">Ѱ�����ĵ�</param>
    /// <param name="monster">������Ʒ</param>
    /// <param name="startPoint">��ʼλ��</param>

    public MonsterBase(int monsterID, int level, GameObject monster, int UniqueID, Vector3 startPoint)
    {
        MonsterCfgTable monsterCfg = MonsterCfgTable.Instance.Get(monsterID);

        this.cfgID = monsterID;
        NpcID = UniqueID;

        this.transform = monster.transform;

        // this.monsterRewardArr = monsterCfg.rewardItemArray;

        //buffģ�ͳ�ʼ
        CreatBuff();

        isDeath = false;
        monsterData = new MonsterData(monsterCfg, level);

        UpdateLocation((int)startPoint.x, (int)startPoint.z);

        MapMsg mapMsg = new MapMsg(monsterData.X, monsterData.Y, true, (ushort)ILNpcEventMap.SetObs);
        SendMsg(mapMsg);
        //monsterFirePoint = monsterGameObject.transform.Find("FirePoint");

        //fireEffect = monsterGameObject.transform.Find("ShootFeedback/ShootFeedback").GetComponent<MMFeedbacks>();
        //��������
        monsterAnimation = new MonsterAnimation(this);

        //�ƶ����Ƴ�ʼ��
        //  moveControl = new MonsterMove(this);
        //ai���Ƴ�ʼ�� 
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
    /// Buff����
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
    /// ��ʾ������Χ����
    /// </summary>
    private async void ShowGird()
    {
        await Task.Delay(100);//������ҷ���������������
        SendMsg((ushort)ILNpcEventMap.HideAttckTip);
        HotMapController.Instance.GetAttackTip(monsterData.X, monsterData.Y, monsterData.attackRange, monsterGridAI.attackTipType);
        SendMsg((ushort)ILNpcEventMap.ShowAttackTip);
    }
    #region Buff
    /// <summary>
    /// �ж�
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
    /// ѣ�β��Ŷ���
    /// </summary>
    /// <param name="tmpBuff">buff</param>
    /// <param name="isAddBuff">�Ƿ����buff</param>
    public override void Vertigo(Buff tmpBuff, bool isAddBuff)
    {
        base.Vertigo(tmpBuff, isAddBuff);
        monsterAnimation.SetAnimation(MonsterAnimationType.Vertigo);
    }
    /// <summary>
    /// �Ƴ�ѣ��
    /// </summary>
    public override void RemoveVertigo(Buff tmpBuff)
    {
        base.RemoveVertigo(tmpBuff);
        monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
    }
    /// <summary>
    ///����
    /// </summary>
    /// <param name="tmpBuff">buff����</param>
    /// <param name="isAddBuff">�Ƿ����buff</param>
    public override void DeformationSheep(Buff tmpBuff, bool isAddBuff)
    {
        base.DeformationSheep(tmpBuff, isAddBuff);
        modelMesh.SetActive(false);
        buffArray[tmpBuff.buffIndex].GetComponentInChildren<ParticleSystem>().Play();

    }
    //�Ƴ�����
    public override void RemoveDeformationSheep(Buff tmpBuff)
    {
        base.RemoveDeformationSheep(tmpBuff);

        buffArray[tmpBuff.buffIndex].GetComponentInChildren<ParticleSystem>().Play();

        modelMesh.SetActive(true);

    }
    /// <summary>
    /// ��ӹ�����buff
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void AddAttack(Buff tmpBuff)
    {
        base.AddAttack(tmpBuff);
        monsterData.attackData.AddAttack += (tmpBuff.attack / 100f);
        Debug.Log("��ӹ�����buff::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// �Ƴ�������buff
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveAddAttack(Buff tmpBuff)
    {
        base.RemoveAddAttack(tmpBuff);
        monsterData.attackData.AddAttack -= (tmpBuff.attack / 100f);
        Debug.Log("�Ƴ�������buff::" + monsterData.attackData.AddAttack);

    }

    /// <summary>
    ///�ӷ�
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void AddDefence(Buff tmpBuff)
    {
        base.AddDefence(tmpBuff);
        monsterData.attackData.AddDefence += (tmpBuff.defence / 100f);
        Debug.Log("�ӷ�::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    ///  �Ƴ��ӷ�Ч��
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveDefence(Buff tmpBuff)
    {
        base.RemoveDefence(tmpBuff);
        monsterData.attackData.AddDefence -= (tmpBuff.defence / 100f);
        Debug.Log(" �Ƴ��ӷ�Ч��::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="tmpBuff"></param>
    /// <returns></returns>
    public override void SubAttack(Buff tmpBuff)
    {
        base.SubAttack(tmpBuff);
        monsterData.attackData.AddAttack -= (tmpBuff.attack / 100f);
        Debug.Log("����::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// �Ƴ�����
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveSubAttack(Buff tmpBuff)
    {
        base.RemoveSubAttack(tmpBuff);
        monsterData.attackData.AddAttack += (tmpBuff.attack / 100f);
        Debug.Log("  �Ƴ�����::" + monsterData.attackData.AddAttack);

    }
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void SubDefence(Buff tmpBuff)
    {
        base.SubDefence(tmpBuff);
        monsterData.attackData.AddDefence -= (tmpBuff.defence / 100f);
        Debug.Log("����::" + monsterData.attackData.AddDefence);

    }
    /// <summary>
    /// �Ƴ�����
    /// </summary>
    /// <param name="tmpBuff"></param>
    public override void RemoveSubDefence(Buff tmpBuff)
    {
        base.RemoveSubDefence(tmpBuff);
        monsterData.attackData.AddDefence += (tmpBuff.defence / 100f);
        Debug.Log("   �Ƴ�����::" + monsterData.attackData.AddDefence);

    }

    #endregion

    /// <summary>
    /// ���ù������
    /// </summary>
    /// <param name="monsterContorller"></param>
    public void SetMosnterControl(MonsterContorller monsterContorller)
    {
        this.monsterContorller = monsterContorller;
    }
    //���¸���λ��
    public void UpdateLocation(int X, int Y)
    {
        monsterData.X = X;
        monsterData.Y = Y;
        transform.position = HotMapController.Instance.GetMapGridPos(X, Y);

    }
    /// <summary>
    /// ս��ʱ���buff
    /// </summary>
    /// <param name="attackData"></param>  
    /// <param name="playerID">���ID</param>
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
    /// ��������
    /// </summary>
    /// <param name="attackData">��������</param>
    /// <param name="roundType">�غ�����</param>
    /// <param name="isBuff">�Ƿ���buff</param>
    /// <param name="playerID">��ǰ��Ҷ���id</param>
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

        if (isDeath)//������ȡ���ｱ��
        {
            waitTime = 3000;
        }
        await Task.Delay(waitTime);

        if (isDeath)//��������
        {
            monsterContorller.RemoveMonster(this);

            int[] tmp = MonsterCfgTable.Instance.Get(cfgID).rewardItemArray;

            if (tmp != null && tmp.Length > 0)
            {
                monsterContorller.MonsterDeathReward(tmp, roundType, playerID);//������ȡ����
                return;
            }
            else
            {
                if (roundType == RoundType.Player)
                {
                    PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, playerID);//��һغϽ���
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

                    PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, playerID);//��һغϽ���
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
    /// �ɼ�������˺�
    /// </summary>
    /// <param name="skillID">����id</param> 
    /// <param name="roundType">�غ�����</param>
    /// <param name="playerID">ʩ����</param>
    /// <returns></returns>
    public async void BeBySkill(SkillItem skillID, RoundType roundType, int TheCaster)
    {
        skillID.Perform(this, false, TheCaster);

        if (monsterData.CurHp <= 0)
        {
            Death();
        }
        int waitTime = 500;//�ȴ�ʱ��

        if (isDeath)//������ȡ���ｱ��
        {
            waitTime = 3000;
        }
        await Task.Delay(waitTime);
        if (isDeath)//������ȡ���ｱ��
        {
            monsterContorller.RemoveMonster(this);
            int[] tmp = MonsterCfgTable.Instance.Get(cfgID).rewardItemArray;
            if (tmp.Length > 0)
            {
                monsterContorller.MonsterDeathReward(tmp, roundType, TheCaster);//������ȡ����
                return;
            }


        }

        //û��
        switch (roundType)
        {
            case RoundType.Player://��һغϽ���

                PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, TheCaster);//��һغϽ���
                SendMsg(playerMsg);
                break;
            case RoundType.Monster://����غϽ���
                SendMsg((ushort)ILNpcEvent.MonsterStartRound);
                // monsterGridAI.EndRound();
                break;
            default:
                break;
        }


    }
    /// <summary>
    /// �������¼�
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
    ///  ����뿪�¼�
    /// </summary>
    /// <param name="eventData"></param>
    void PointExit(GameObject obj)
    {
        enterPointer = false;
        ILUIManager.ShowView(UIPanelPath.DetailPanel, UILevel.Top, ILUIEventPanel.DetailPanel);
        DeatailMsg deatailMsg = new DeatailMsg((ushort)ILUIEventPlayerPanel.HideMonsterCommon);
        SendAwaitMsg(deatailMsg);
    }



    public int attackSpeed = 600;//����
    public int turnTime = 0;//ת������ʱ��
    public bool Fireing = false;//�Ƿ��ڿ�����
    /// <summary>
    /// ������
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
    /// �����ӵ�
    /// </summary>
    public virtual void Attack()
    {
        Debug.Log(" monster Attack");

        //Ͷ��������������Ҫ������㣬��׼���͵�������Ҫȷ����׼����
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
    /// ����
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


