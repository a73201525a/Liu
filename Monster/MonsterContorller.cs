using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 怪物生成种类，有多少种怪物就有多少enum
/// </summary>
public class MonsterContorller : NPCBase
{
    private Dictionary<GameObject, MonsterBase> monsterDic;//所有怪物字典
    List<MonsterBase> monsters;//所有的怪物

    //怪物生成工具类
    private MonsterHelper monsterHelper;

    private int curID;//当前ID
    private int curLevel;//当前创建怪等级
    private Vector3 creatPos;//创建pos
    public RoundType monsterRound = RoundType.Player;//是否是怪物回合

    private Transform curMonsterParent;//当前正在创建的怪物的父对象
    public bool MonsterPointEntrer;//怪物能否反键操作
    List<Player> PlayerList;//玩家list
    private bool isfightEnd;//怪物完,战斗结束

    public bool CurrentProgress;//当前进度

    bool isGameing;
    public override void ProcessEvent(ILMsgBase tmpMsg)
    {
        //监听事件
        switch (tmpMsg.msgID)
        {
            case (ushort)ILNpcEvent.LoadMonster://加载怪物
                {
                    ILHunkAssetResBack monster = (ILHunkAssetResBack)tmpMsg;
                    CreatMonster(curID,curLevel, monster.value);
                }
                break;
            case (ushort)ILNpcEvent.CreatMonster://创建怪物
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    LoadMonsterInit(monsterMsg.monsterID,monsterMsg.buildIndex, monsterMsg.creatPos, monsterMsg.parent);
                }
                break;

            case (ushort)ILNpcEvent.MonsterByAttack:////怪物被攻击
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    MonsterByAttack(monsterMsg.monseterObj, monsterMsg.player);
                }
                break;

            case (ushort)ILNpcEvent.MonsterCounterAttack://怪物被反击
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    MonsterByCounterAttack(monsterMsg.monseterObj, monsterMsg.player);

                }
                break;
            case (ushort)ILNpcEvent.MonsterBySkill://怪物被技能命中
                {
                    SkillDataMsg skillDataMsg = (SkillDataMsg)tmpMsg;
                    MonsterBySkill(skillDataMsg.Skill, skillDataMsg.endNpc.transform.gameObject, skillDataMsg.roundType, skillDataMsg.NPCID);
                }
                break;

            case (ushort)ILNpcEvent.MonsterStartRound://怪物回合开始
                {
                    monsterRound = RoundType.Monster;
                    NextStartRound();
                }
                break;
            case (ushort)ILNpcEvent.GetAllPlayerBack://怪物回合开始,获取所有玩家
                {
                    PlayerMsgBack playerMsg = (PlayerMsgBack)tmpMsg;

                    PlayerList = playerMsg.PlayerList;

                }
                break;
            case (ushort)ILNpcEvent.GetAllBuildBack://怪物回合开始,获取所有建筑
                {
                    BuildMsgBack Msg = (BuildMsgBack)tmpMsg;
                    NextMonsterRound(PlayerList, Msg.buildList);

                }
                break;
            case (ushort)ILNpcEvent.GetALlMonster://得到所有怪物
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    MonsterMsgBack msgBack = new MonsterMsgBack(monsterMsg.CallBack, GetAllMonsters());
                    SendMsg(msgBack);
                }
                break;
            case (ushort)ILNpcEvent.GetALlMonsterAndDeath://得到所有怪物
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    MonsterMsgBack msgBack = new MonsterMsgBack(monsterMsg.CallBack, GetAllMonsters(true));
                    SendMsg(msgBack);
                }
                break;
            case (ushort)ILNpcEvent.PointEntrer://怪物反键显示详情
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;
                    MonsterPointEntrer = monsterMsg.isPointerMonster;
                }
                break;
            case (ushort)ILNpcEvent.GameOver://游戏结束
                {

                    isGameing = false;
                }
                break;
            case (ushort)ILNpcSaveMap.SetMonsterData://设置怪物读档数据
                {
                    MonsterMsg monsterMsg = (MonsterMsg)tmpMsg;

                    SaveMapData[] mapDatas = monsterMsg.SaveMapData;

                    for (int i = 0; i < mapDatas.Length; i++)
                    {
                        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.RemoveHUDBlood, monsters[i].transform, monsters[i].monsterData.CurHp, monsters[i].monsterData.MaxHp, monsters[i].NpcID);
                        SendMsg(hUDMsg);
                    }



                    for (int i = 0; i < mapDatas.Length; i++)
                    {
                        monsters[i].isDeath = mapDatas[i].IsDeath;
                        monsters[i].monsterData.CurHp = mapDatas[i].CurHp;
                        monsters[i].NpcID = mapDatas[i].NpcID;

                        Debug.Log("monsters[i].NpcID:::" + monsters[i].NpcID);
                        Debug.Log("monsters[i].isDeath:::" + monsters[i].isDeath);
                        Debug.Log("monsters[i].dddd:::" + monsters[i].transform.gameObject.name);
                        if (monsters[i].isDeath)
                        {
                            MapMsg mapMsg = new MapMsg(monsters[i].monsterData.X, monsters[i].monsterData.Y, false, (ushort)ILNpcEventMap.SetObs);
                            SendMsg(mapMsg);

                            MapMsg mapMsg1 = new MapMsg((ushort)ILNpcEventMap.UpdateMap);
                            SendMsg(mapMsg1);

                            //monsters.Remove(monsterDic[monster.transform.gameObject]);
                            //monsterDic.Remove(monster.transform.gameObject);

                            monsters[i].HideGameObject();
                        }
                        else
                        {
                            HUDMsg hUDMsg1 = new HUDMsg((ushort)ILUIEvent.CreatHUDBlood, monsters[i].transform, monsters[i].monsterData.CurHp, monsters[i].monsterData.MaxHp, monsters[i].NpcID);
                            SendMsg(hUDMsg1);
                        }

                    }

                }
                break;

        }
    }

    public void Awake()
    {
        //注册消息
        msgIds = new ushort[]
        {
            (ushort)ILNpcEvent.LoadMonster,
            (ushort)ILNpcEvent.CreatMonster,
            (ushort)ILNpcEvent.RemoverDeathMonster,
            (ushort)ILNpcEvent.PointEntrer,
            (ushort)ILNpcEvent.MonsterByAttack,
            (ushort)ILNpcEvent.MonsterCounterAttack,
            (ushort)ILNpcEvent.MonsterBySkill,
            (ushort)ILNpcEvent.GetALlMonster,
            (ushort)ILNpcEvent.GetAllPlayerBack,
            (ushort)ILNpcEvent.MonsterStartRound,
            (ushort)ILNpcEvent.GetAllBuildBack,
            (ushort)ILNpcEvent.LoadMonserComb,
            (ushort)ILNpcEvent.GameOver,
            (ushort)ILNpcSaveMap.SetMonsterData,
            (ushort)ILNpcEvent.GetALlMonsterAndDeath,
        };
        RegistSelf(this, msgIds);
        //读取配置 Excle
        MonsterCfgTable.Initialize(Utility.TablePath + "MonsterCfgData");
        monsterDic = new Dictionary<GameObject, MonsterBase>();
        monsters = new List<MonsterBase>();
        monsterHelper = new MonsterHelper();
        isfightEnd = false;
        curMonsterParent = new GameObject("MonsterParent").transform;
        ILHunkAssetRes monsterobj = new ILHunkAssetRes("scenesone", "monster", (ushort)ILAssetEvent.GetResObject);
        SendMsg(monsterobj);
        /////////////创建怪物///////////// 测试
        ////
        //Init(arr);
    }
    /// <summary>
    /// 怪物初始化数据
    /// </summary>
    /// <param name=""></param>
    public void Init(int[] monster, Vector2[] pos,int level=1)
    {
        for (int i = 0; i < monster.Length; i++)
        {

            LoadMonsterInit(monster[i], level, new Vector3(pos[i].x, 0, pos[i].y), curMonsterParent);//1
        }
        isGameing = true;
    }

    /// <summary>
    /// 获取所有怪物
    /// </summary>
    /// <returns></returns>
    List<MonsterBase> GetAllMonsters(bool isAll = false)
    {
        List<MonsterBase> tmpMonsters = new List<MonsterBase>();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (!monsters[i].isDeath || isAll)
            {
                tmpMonsters.Add(monsters[i]);
            }

        }
        return tmpMonsters;
    }

    /// <summary>
    /// 技能命中
    /// </summary>
    /// <param name="SkillID"></param>
    /// <param name="monseterObj"></param>
    /// <param name="player"></param>
    public void MonsterBySkill(SkillItem SkillID, GameObject monseterObj, RoundType roundType, int NpcID)
    {
        monsterDic[monseterObj].BeBySkill(SkillID, roundType, NpcID);
    }
    /// <summary>
    /// 怪物被攻击
    /// </summary>
    /// <param name="monseterObj"></param>
    /// <param name="player"></param>
    public void MonsterByAttack(GameObject monseterObj, Player player)
    {

        MonsterBase monster = monsterDic[monseterObj];

        bool isCounter = monster.monsterGridAI.IsCounter(player);

        monster.BeHurt(player.playerData.attackData, monsterRound, isCounter, true, player.playerID);

        if (isCounter)
        {
            monsterDic[monseterObj].monsterGridAI.CounterAttack(player);
        }
    }

    /// <summary>
    /// 怪物被反击
    /// </summary>
    /// <param name="monseterObj"></param>
    /// <param name="player"></param>
    public void MonsterByCounterAttack(GameObject monseterObj, Player player)
    {
        Debug.Log("怪物自动战斗回合,被反击");
        monsterDic[monseterObj].BeHurt(player.playerData.attackData, monsterRound, false, true, player.playerID);
    }




    /// <summary>
    /// 怪物死亡后,爆出的奖励
    /// </summary>
    public void MonsterDeathReward(int[] reward, RoundType roundType, int player)
    {

        if (player == -1)//为中立的单位击杀
        {
            Debug.Log("为中立的单位击杀 player:" + player);
            return;
        }
        SettlementMsg msgBack = new SettlementMsg((ushort)ILUIEventCurrentFightEndRound.ShowRewardItem, reward, roundType, player);
        SendMsg(msgBack);
        ILUIManager.ShowView(UIPanelPath.FightRewardPanel, UILevel.Top, ILUIEventPanel.FightRewardPanel);

    }



    //当前进行行动的怪物索引
    public int index = -1;


    /// <summary>
    /// 单个怪物回合开始 先获取敌对数据
    /// </summary>
    public async void NextStartRound()
    {
        Debug.Log("===========================================================NextStartRound==================================================================");
        await Task.Delay(100);
        if (!isGameing)
        {
            return;
        }
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.GetAllPlayer, (ushort)ILNpcEvent.GetAllPlayerBack);
        SendMsg(playerMsg);
        BuildMsg buildMsg = new BuildMsg((ushort)AdventureBuildEvent.GetAllBuild, (ushort)ILNpcEvent.GetAllBuildBack);
        SendMsg(buildMsg);

    }


    /// <summary>
    /// 大回合结束
    /// </summary>
    public async void RoundEnd()
    {
        BuffMsg buffMsg = new BuffMsg((ushort)ILNpcBuff.RoundEnd);
        SendMsg(buffMsg);

        //如果有需要 可以等待一下

        BuildMsg buildMsg = new BuildMsg((ushort)AdventureBuildEvent.PointerEnter, false);//建筑反键启用
        SendMsg(buildMsg);
        //执行完毕 开始回合
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundStart);
        SendMsg(playerMsg);
    }




    /// <summary>
    /// 下一个怪物执行回合
    /// </summary>
    /// <param name="tmpPlayerList">玩家</param>
    /// <param name="buildItemBases">建筑</param>
    void NextMonsterRound(List<Player> tmpPlayerList, List<BuildItemBase> buildItemBases)
    {

        index++;

        if (MonsterRoundEnd())
        {
            return;
        }

        if (monsters[index].isDeath)
        {
            NextStartRound();
            return;
        }

        if (monsters[index].isControll)
        {
            NextStartRound();
            return;
        }

        monsters[index].monsterGridAI.FixedUpadte(tmpPlayerList, buildItemBases, GetAllMonsters());
    }

    //回合是否结束
    bool MonsterRoundEnd()
    {

        if (index == monsters.Count)
        {
            Debug.Log("所有怪物行动结束");

            RoundEnd();//更新怪物Buff状态
            index = -1;
            monsterRound = RoundType.Player;
            MonsterPointEntrer = false;
            return true;

        }
        return false;
    }
    /// <summary>
    /// 加载怪物
    /// </summary>
    /// <param name="id">怪物id</param>
    /// <param name="index">怪物索引</param>
    /// <param name="pos">生成坐标</param>

    /// <param name="parent">父对象</param>
    public void LoadMonsterInit(int id,int level, Vector3 pos, Transform parent)
    {
        curID = id;
        creatPos = pos;


        curMonsterParent = parent;
        //加载怪物
        ILHunkAssetRes monsterobj = new ILHunkAssetRes(false, (ushort)ILAssetEvent.GetResObject, "scenesone", "monster", curID.ToString(), (ushort)ILNpcEvent.LoadMonster);
        SendMsg(monsterobj);

    }
    //生成怪物
    public void CreatMonster(int ID,int level, Object tmpMonster)
    {
        ///创建
        GameObject tempMosnter = (GameObject)GameObject.Instantiate(tmpMonster);
        //tempMosnter.name = ID + "," + buildIndex;
        tempMosnter.transform.SetParent(curMonsterParent);
        //tempMosnter.transform.position = creatPos;
        int unquieID = NumericalTool.AddMonster();
     
      
        MonsterBase monsterBase = monsterHelper.CreatMonster(ID, level, tempMosnter, unquieID, creatPos);

        monsterBase.SetMosnterControl(this);
        //  ClickMonsterComb(monsterBase);
        monsterDic.Add(tempMosnter, monsterBase);
        monsters.Add(monsterBase);
    }

    /// <summary>
    /// 移除怪物
    /// </summary>
    /// <param name="monster"></param>
    public void RemoveMonster(MonsterBase monster)
    {
        BuffMsg buffMsg = new BuffMsg((ushort)ILNpcBuff.NpcDeath, monster.NpcID);
        SendMsg(buffMsg);

        MapMsg mapMsg = new MapMsg(monster.monsterData.X, monster.monsterData.Y, false, (ushort)ILNpcEventMap.SetObs);
        SendMsg(mapMsg);

        MapMsg mapMsg1 = new MapMsg((ushort)ILNpcEventMap.UpdateMap);
        SendMsg(mapMsg1);

        //monsters.Remove(monsterDic[monster.transform.gameObject]);
        //monsterDic.Remove(monster.transform.gameObject);

        monster.HideGameObject();

        isFightEnd(monster);
    }

    /// <summary>
    /// 战斗结束
    /// </summary>
    public async void isFightEnd(MonsterBase monster)
    {
        await Task.Delay(100);
        bool isOver = true;
        for (int i = 0; i < monsters.Count; i++)
        {
            if (!monsters[i].isDeath)
            {
                isOver = false;
            }
        }
        if (isOver)
        {
            Debug.Log("怪物全部击杀");
            isfightEnd = true;
            AdventureMsg adventureMsg = new AdventureMsg((ushort)ILUIEventCurrentFightEndRound.FightEnd, AdventureWinType.MonsterAllDeath);
            SendMsg(adventureMsg);
        }

        if (monster.monsterData.monsterType == MonsterType.Boss)
        {
            isfightEnd = true;
            Debug.Log("怪物Boss被击杀,战斗胜利");
            AdventureMsg adventureMsg = new AdventureMsg((ushort)ILUIEventCurrentFightEndRound.FightEnd, AdventureWinType.KillBoss);
            SendMsg(adventureMsg);

        }

    }

    /// <summary>
    /// 结束
    /// </summary>
    private void GameOver()
    {

        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].OnDestroy();
        }
        monsterDic.Clear();
        monsters.Clear();
        // fightMonster.Clear();
        monsterHelper = null;
        PlayerList = null;

        ILHunkAssetRes monsterobj = new ILHunkAssetRes("scenesone", "monster", (ushort)ILAssetEvent.ReleaseSingleBundle);
        SendMsg(monsterobj);
    }

    public override void OnDestroy()
    {
        GameOver();
        base.OnDestroy();
    }
}
