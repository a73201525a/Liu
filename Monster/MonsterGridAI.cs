using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum PracticeType
{
    Attack,//攻击
    Skill,//伤害技能
    GainSkill,//队友方使用的增益技能
}

public enum GridMonsterAI
{
    Idle,//站立AI（在攻击范围内出现敌人的时候，才会主动攻击）
    Initiative,//主动寻找敌人的AI
}

//攻击提示类型
public enum AttackTipType
{
    Cube,//方形
    Lozenge,//菱形 
    Ranged,//远程
    Cross,//十字
}

public enum AIAttackType
{
    weakness,//优先寻找克制关系的敌人攻击（骑兵克步兵  步兵克弓箭  弓箭克骑兵）
    near,//优先寻找最近的敌人攻击
    enchanter, //是否优先攻击法师

    siege,//优先攻击建筑

    skill,//优先使用技能
}
/// <summary>
/// 兵种
/// </summary>
public enum SolderType
{
    Null,
    infantry,//步兵
    cavalry,//骑兵
    archer,//弓箭手
    enchanter,//法师
}
public class MonsterGridAI
{
    List<Player> PlayerList;//玩家list
    List<BuildItemBase> BuildBases;//建筑List
    List<MonsterBase> monsterList;//怪物List

    //攻击目标
    Player player;

    //怪物增益技能目标
    MonsterBase monsterTagetr;
    //目标建筑
    BuildItemBase build;
    //本次攻击目标否是建筑
    bool isAttackBuild;
    //怪物本回合优先的操作
    PracticeType practiceType;

    //本回合准备使用的技能ID
    int curSkillID;

    //移动到可以攻击目标点的位置
    int attackIndex;
    int attackIndeY;


    //行进路线中，可能碰到可以攻击的目标（临时目标）
    Player curAttackTagetPlayer;

    //行进路线中，可能碰到的怪物目标（临时目标）
    MonsterBase curMonsterTaget;

    //怪物对象
    MonsterBase monster;

    //每回合移动距离
    int speedRange;

    //巡逻范围
    public int patrolRange;

    //施法范围
    int castingRange;

    //攻击范围
    int attckRange;
    /// <summary>
    /// 移动速度
    /// </summary>
    float speed = 0.5f;
    /// <summary>
    /// 攻击tip 类型
    /// </summary>
    public AttackTipType attackTipType;

    /// <summary>
    /// 施法提示 类型
    /// </summary>
    AttackTipType skillTipType = AttackTipType.Lozenge;



    SolderType solderType;//兵种


    /// <summary>
    /// AI类型
    /// </summary>
    GridMonsterAI aiType;
    /// <summary>
    ///  攻击类型
    /// </summary>
    AIAttackType aiAttackType;



    /// <summary>
    /// 设置可以攻击目标点的位置
    /// </summary>
    void SetFightTagert(int playerX, int playerY, int range, AttackTipType tipType)
    {
        int tmpX = playerX;
        int tmpY = playerY;


        List<MapHex> tiplist = HotMapController.Instance.GetAttackTip(playerX, playerY, range, tipType);
        int dis = 999;
        for (int i = 0; i < tiplist.Count; i++)
        {
            if (!HotMapController.Instance.IsObs(tiplist[i].X, tiplist[i].Y))
            {
                int tmpDis = Mathf.Abs(monster.monsterData.X - tiplist[i].X) + Mathf.Abs(monster.monsterData.Y - tiplist[i].Y);

                if (tmpDis < dis)
                {
                    dis = tmpDis;
                    tmpX = tiplist[i].X;
                    tmpY = tiplist[i].Y;
                }
            }
        }
        Debug.Log("tmpX:::" + tmpX);
        Debug.Log("tmpY:::" + tmpY);
        attackIndex = tmpX;
        attackIndeY = tmpY;
    }

    MonsterGridAITool monsterGridAITool;
    /// <summary>
    /// 怪物AI
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="player"></param>
    internal MonsterGridAI(MonsterBase monster, MonsterCfgTable monsterCfg)
    {
        this.monster = monster;

        solderType = (SolderType)monsterCfg.solderType;

        attackTipType = (AttackTipType)monsterCfg.attackTipType;

        patrolRange = monsterCfg.patrolRange;

        attckRange = monsterCfg.attackRange;
        aiType = (GridMonsterAI)monsterCfg.aiType;

        speedRange = monsterCfg.speedRange;

        aiAttackType = (AIAttackType)monsterCfg.aiAttackType;

        castingRange = monsterCfg.castingRange;

        monsterGridAITool = new MonsterGridAITool();

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="players">玩家list</param>
    /// <param name="buildList">建筑</param>
    /// <param name="monsterContorller">怪物控制</param>
    public void FixedUpadte(List<Player> players, List<BuildItemBase> buildList, List<MonsterBase> monsters)//每回合执行一次 （玩家结束回合后，到怪物回合）
    {

        PlayerList = players;
        BuildBases = buildList;
        monsterList = monsters;
        isAttackBuild = false;
        monster.monsterContorller.MonsterPointEntrer = true;//怪物自动回合,反键操作禁用

        Debug.Log("移动距离:" + speedRange);
        Debug.Log("巡逻距离:" + patrolRange);
        Debug.Log("攻击距离:" + attckRange);
        Debug.Log("施法距离:" + castingRange);
        Debug.Log("兵种::::" + solderType);
        Debug.Log("移动类型:::::" + aiType);
        Debug.Log("攻击类型:::::" + aiAttackType);
        Debug.Log("攻击提示类型:::::" + attackTipType);
        Debug.Log("PlayerList:::::" + PlayerList.Count);
        Debug.Log("BuildBases:::::" + BuildBases.Count);


        monsterGridAITool.IsPractice(monster, aiAttackType, out curSkillID, out practiceType);
        //  practiceType = PracticeType.GainSkill;
        //aiAttackType = AIAttackType.siege;
        bool isTarget = false;
        Debug.Log("practiceType::" + practiceType);
        if (practiceType == PracticeType.Skill)//优先用技能
        {

            isTarget = monsterGridAITool.CheckTargetSkill(PlayerList, monsterList, aiAttackType, practiceType, monster, out player, out monsterTagetr);//找技能目标
        }
        else if (practiceType == PracticeType.GainSkill)
        {
            isTarget = monsterGridAITool.CheckTargetSkill(PlayerList, monsterList, aiAttackType, practiceType, monster, out player, out monsterTagetr);//找技能目标
        }
        else if (practiceType == PracticeType.Attack && aiAttackType == AIAttackType.siege)//目标优先建筑
        {
            isTarget = monsterGridAITool.CheckBuildTarget(buildList, aiAttackType, solderType, monster, out build);
        }
        else if (practiceType == PracticeType.Attack)
        {
            isTarget = monsterGridAITool.CheckTarget(PlayerList, aiAttackType, solderType, monster, out player);//找目标
        }



        Debug.Log("是否找到了目标：：" + isTarget);

        if (isTarget)
        {


            if (practiceType == PracticeType.Attack && aiAttackType == AIAttackType.siege)
            {
                Debug.Log("攻击目标 build：：" + build.buildID);

                SetFightTagert(build.IndexX, build.IndexY, attckRange, attackTipType);
            }
            else if (practiceType == PracticeType.Attack)
            {
                Debug.Log("攻击目标：：" + player.playerData.playerJson.name);
                Debug.Log("攻击目标的兵种Type：：" + player.playerData.playerJson.solderType);

                SetFightTagert(player.playerData.MapIndexX, player.playerData.MapIndexY, attckRange, attackTipType);
            }
            else if (practiceType == PracticeType.Skill)
            {
                Debug.Log("施法目标：：" + player.playerData.playerJson.name);

                SetFightTagert(player.playerData.MapIndexX, player.playerData.MapIndexY, castingRange, skillTipType);
            }
            else if (practiceType == PracticeType.GainSkill)
            {
                Debug.Log("增益施法目标：：" + monsterTagetr.transform.gameObject.name);
                SetFightTagert(monsterTagetr.monsterData.X, monsterTagetr.monsterData.Y, castingRange, skillTipType);
            }

        }


        switch (aiType)//需不需要移动
        {
            case GridMonsterAI.Idle:
                {
                    Debug.Log("进入Idle AI 逻辑");
                    if (isTarget)
                    {
                        Debug.Log("发现目标");
                        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
                        {
                            Debug.Log("目标为优先施法");
                            if (!isSkillOrAttack())
                            {
                                Debug.Log("施法和攻击范围内没有目标  结束这个怪物的AI操作");
                                EndRound();
                            }
                        }
                        else
                        {
                            if (!isAttackOrSkill())
                            {
                                Debug.Log("施法和攻击范围内没有目标  结束这个怪物的AI操作");
                                EndRound();
                            }
                        }

                    }
                    else
                    {
                        Debug.Log("攻击和施法范围内均未发现目标 结束怪物的操作");
                        EndRound();
                    }
                }

                break;
            case GridMonsterAI.Initiative:
                {
                    Debug.Log("进入Initiative AI 逻辑");
                    if (isTarget)
                    {
                        Debug.Log("发现目标");
                        Patrol();
                    }
                    else
                    {
                        Debug.Log("巡逻范围内未发现目标 结束怪物的操作");
                        EndRound();
                    }
                }
                break;
        }
    }



    /// <summary>
    /// 施法距离是否足够
    /// </summary>
    /// <returns></returns>
    public bool Skill(bool isMove)
    {
        //非优先施法的单位，先移动 在判断是否施法
        if (practiceType == PracticeType.Attack && !isMove)
        {
            return false;
        }
        bool isSkill = false;
        List<MapHex> tmplist = HotMapController.Instance.GetAttackTip(monster.monsterData.X,
                   monster.monsterData.Y, castingRange, skillTipType);

        if (practiceType == PracticeType.Skill)
        {
            isSkill = CheckAttackPlayer(isMove, tmplist, false);
        }
        else if (practiceType == PracticeType.GainSkill)
        {

            isSkill = CheckMonsterSkill(isMove, tmplist);
        }
        else if (practiceType == PracticeType.Attack)//优先攻击的怪物 攻击距离不够 查看是否有法术可以施法
        {
            Debug.Log("优先攻击的怪物 攻击距离不够 查看是否有法术可以施法");
            if (monster.monsterData.monsterCfg.skillID.Length > 0)
            {
                curSkillID = monster.monsterData.monsterCfg.skillID[Random.Range(0, monster.monsterData.monsterCfg.skillID.Length)];
                isSkill = CheckAttackPlayer(isMove, tmplist, false);
            }
        }
        return isSkill;
    }

    bool CheckMonsterSkill(bool isMove, List<MapHex> tmplist)
    {
        for (int i = 0; i < tmplist.Count; i++)
        {
            if (tmplist[i].X == monsterTagetr.monsterData.X && tmplist[i].Y == monsterTagetr.monsterData.Y && !monsterTagetr.isDeath)
            {
                Debug.Log("增益施法目标对象在范围内");
                curMonsterTaget = monsterTagetr;
                return true;
            }
        }
        if (isMove)
        {
            Debug.Log("目标对象不在范围内，移动完成后 寻找范围内有没有其他可增益的目标对象");
            for (int i = 0; i < monsterList.Count; i++)
            {
                for (int j = 0; j < tmplist.Count; j++)
                {
                    if (tmplist[j].X == monsterList[i].monsterData.X && tmplist[j].Y == monsterList[i].monsterData.Y && !monsterList[i].isDeath)
                    {
                        Debug.Log("移动完成后 寻找范围内有没有其他可攻击对象,找到其他施法目标对象在范围内");
                        curMonsterTaget = monsterList[i];
                        return true;
                    }
                }

            }
        }

        Debug.Log("没有找到任何增益施法对象");
        return false;
    }


    /// <summary>
    /// 寻找能攻击或施法的目标
    /// </summary>
    bool CheckAttackPlayer(bool isMove, List<MapHex> tmplist, bool isAttackOrSkill)
    {
        if (build != null && isAttackOrSkill)
        {
            for (int i = 0; i < tmplist.Count; i++)
            {
                if (tmplist[i].X == build.IndexX && tmplist[i].Y == build.IndexY && !build.IsDeath)
                {
                    isAttackBuild = true;
                    Debug.Log("目标建筑在范围内");
                    return true;
                }

            }
        }
        if (player != null)
        {
            for (int i = 0; i < tmplist.Count; i++)
            {
                if (tmplist[i].X == player.playerData.MapIndexX && tmplist[i].Y == player.playerData.MapIndexY && !player.isDeath)
                {
                    Debug.Log("目标对象在范围内");
                    curAttackTagetPlayer = player;
                    return true;
                }

            }
        }

        if (isMove)
        {
            Debug.Log("目标对象不在范围内，移动完成后 寻找范围内有没有其他可攻击对象");
            for (int i = 0; i < PlayerList.Count; i++)
            {
                for (int j = 0; j < tmplist.Count; j++)
                {
                    if (tmplist[j].X == PlayerList[i].playerData.MapIndexX && tmplist[j].Y == PlayerList[i].playerData.MapIndexY && !PlayerList[i].isDeath)
                    {
                        Debug.Log("移动完成后 寻找范围内有没有其他可攻击对象,找到其他攻击目标对象在范围内");
                        curAttackTagetPlayer = PlayerList[i];
                        return true;
                    }
                }

            }

        }
        Debug.Log("没有找到任何攻击对象");
        return false;
    }

    /// <summary>
    /// 是否有攻击对象
    /// </summary>
    /// <returns></returns>
    public bool Attack(bool isMove)
    {
        //type 攻击范围类型不同 
        if (monster.isAttack)
        {
            Debug.Log("怪物被控制了攻击,无法攻击");
            return false;
        }

        List<MapHex> tmplist = HotMapController.Instance.GetAttackTip(monster.monsterData.X,
     monster.monsterData.Y, attckRange, attackTipType);

        return CheckAttackPlayer(isMove, tmplist, true);

    }


    /// <summary>
    /// 进入战斗
    /// </summary>
    public async void EnterFight(AttackTipType attacktype, bool isSkill = false)
    {
        ///=================战斗流程 测试用==================

        Debug.Log("进入战斗流程");
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.UpdatePlayer, monster.transform.position, UpdatePlayerType.UpdateCameraFollowTarget);
        monster.SendMsg(playerMsg);
        Debug.Log("相机位置更新到当前怪物战斗或者施法位置");
        await Task.Delay(1000);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.ShowHUDBlood);
        monster.SendMsg(hUDMsg);

        MapMsg mapMsg = new MapMsg((ushort)ILNpcEventMap.ShowAttackTip);
        monster.SendMsg(mapMsg);


        await Task.Delay(1000);
        monster.SendMsg((ushort)ILNpcEventMap.HideAttckTip);//隐藏攻击提示

        monster.UpdateLocation(monster.monsterData.X, monster.monsterData.Y);

        if (isSkill)
        {
            if (practiceType == PracticeType.GainSkill)
            {
                ChooseAnimation(curMonsterTaget.transform.position, curMonsterTaget.monsterData.X, curMonsterTaget.monsterData.Y);
            }
            else
            {
                ChooseAnimation(curAttackTagetPlayer.transform.position, curAttackTagetPlayer.playerData.MapIndexX, curAttackTagetPlayer.playerData.MapIndexY);
            }


            await Task.Delay(2000);
            Debug.Log("播放施法动画");
            //==============TODO::需要施法动作===========
            monster.monsterAnimation.SetAnimation(MonsterAnimationType.Skill);
            Debug.Log("施法特效播放");
            Debug.Log("curSkillID:::" + curSkillID);
            //curSkillID = 1008;
            if (practiceType == PracticeType.GainSkill)
            {
                CreatSkillMsg skillMsg = new CreatSkillMsg((ushort)ILNpcSKill.CreatSkill, curSkillID, monster.monsterData.SkillLevel, monster, curMonsterTaget, null, SkillAttackObject.monster, SkillAttackObject.monster);
                monster.SendMsg(skillMsg);

            }
            else
            {
                CreatSkillMsg skillMsg = new CreatSkillMsg((ushort)ILNpcSKill.CreatSkill, curSkillID, monster.monsterData.SkillLevel, monster, curAttackTagetPlayer, null, SkillAttackObject.monster, SkillAttackObject.player);
                monster.SendMsg(skillMsg);
            }


            await Task.Delay(2000);
            monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);



        }
        else//物理攻击
        {
            if (isAttackBuild)
            {
                ChooseAnimation(build.buildGameObject.transform.position, build.IndexX, build.IndexY);
                await Task.Delay(2000);
                Debug.Log("播放攻击动画");
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);
                await Task.Delay(3000);
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
                //玩家被攻击
                BuildMsg msg = new BuildMsg((ushort)AdventureBuildEvent.BuildByAttack, build.buildGameObject, monster.NpcID, RoundType.Monster);//玩家攻击敌方建筑
                monster.SendMsg(msg);
            }
            else
            {
                ChooseAnimation(curAttackTagetPlayer.transform.position, curAttackTagetPlayer.playerData.MapIndexX, curAttackTagetPlayer.playerData.MapIndexY);
                await Task.Delay(2000);
                Debug.Log("播放攻击动画");
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);
                await Task.Delay(3000);
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
                //玩家被攻击
                PlayerDataMsg playerDataMsg = new PlayerDataMsg((ushort)ILNpcEvent.PlayerByAttack, curAttackTagetPlayer.playerID, monster);
                monster.SendMsg(playerDataMsg);

            }

        }



    }

    void ChooseAnimation(Vector3 pos, int X, int Y)
    {
        monster.transform.DOLookAt(pos, 0.1f);
        Debug.Log("显示攻击选中目标动画");
        MapMsg mapMsg = new MapMsg(X, Y, (ushort)ILNpcEventMap.ShowAttackTargetTip);
        monster.SendMsg(mapMsg);
    }
    /// <summary>
    /// 能否反击
    /// </summary>
    /// <returns></returns>
    public bool IsCounter(Player player)
    {
        if (monster.isDeath)
        {
            return false;
        }

        if (monster.isAttack)
        {

            return false;
        }
        if (monster.isControll)
        {

            return false;
        }
        if (!CounterAttackAttackRange(player))
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// 怪物反击
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public async void CounterAttack(Player player)
    {
        await Task.Delay(1000);

        if (monster.isDeath)
        {
            //PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, player.NpcID);//玩家回合结束
            //monster.SendMsg(playerMsg);
            return;
        }

        monster.transform.LookAt(player.transform.position);
        Debug.Log("怪物播放反击动画");
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);//怪物反击
        await Task.Delay(3000);
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);

        //玩家被反击
        PlayerDataMsg playerDataMsg = new PlayerDataMsg((ushort)ILNpcEvent.PlayerCounterAttack, player.playerID, monster);
        monster.SendMsg(playerDataMsg);


    }
    /// <summary>
    /// 反击范围检测
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="attackRange"></param>
    /// <param name="attackTip"></param>
    /// <param name="player">反击玩家目标</param>
    /// <returns></returns>
    bool CounterAttackAttackRange(Player player)
    {

        List<MapHex> tmpList = HotMapController.Instance.GetAttackTip(monster.monsterData.X, monster.monsterData.Y, attckRange, attackTipType);

        for (int i = 0; i < tmpList.Count; i++)
        {
            if (tmpList[i].X == player.playerData.MapIndexX && tmpList[i].Y == player.playerData.MapIndexY)
            {
                Debug.Log("怪物在反击范围内");
                monster.transform.LookAt(player.ModelTool.PlayerGameObject.transform);

                return true;
            }
        }
        Debug.Log("怪物不在反击范围内");
        return false;
    }
    /// <summary>
    /// 巡逻
    /// </summary>
    public void Patrol()
    {

        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
        {
            Debug.Log("怪物为优先施法怪物");
            if (isSkillOrAttack())
            {
                return;
            }
        }
        else
        {
            Debug.Log("怪物为优先攻击怪物");
            if (isAttackOrSkill())
            {
                return;
            }
        }
        Debug.Log("目标不在攻击和施法范围内 进入巡逻逻辑");
        Debug.Log("monster.isMove:::" + monster.isMove);
        if (monster.isMove)
        {
            //被禁止移动 跳过移动 寻找最近攻击目标
            if (aiAttackType == AIAttackType.skill)
            {
                if (!isSkillOrAttack(true))
                {
                    EndRound();
                    return;
                }
            }
            else
            {
                if (!isAttackOrSkill(true))  //移动完成后 执行攻击判断
                {
                    EndRound();
                    return;
                }
            }
            return;
        }
        RoadPatrol();

    }

    Vector2 tmpLocalPos;

    /// <summary>
    /// 移动动画
    /// </summary>
    public async void RoadPatrol()
    {

        await Task.Delay(1000);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.HideHUDBlood);
        monster.SendMsg(hUDMsg);
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.UpdatePlayer, monster.transform.position, UpdatePlayerType.UpdateCameraFollowTarget);
        monster.SendMsg(playerMsg);
        Debug.Log("相机位置更新到当前怪物位置");
        await Task.Delay(1000);

        MapMsg mapMsg1 = new MapMsg(monster.monsterData.X, monster.monsterData.Y, false, (ushort)ILNpcEventMap.SetObs);
        monster.SendMsg(mapMsg1);
        monster.SendMsg((ushort)ILNpcEventMap.UpdateMap);

        HotMapController.Instance.GetTip(monster.monsterData.X, monster.monsterData.Y, speedRange);
        monster.SendMsg((ushort)ILNpcEventMap.ShowMapTip);
        await Task.Delay(1000);
        List<MapHex> poslist = HotMapController.Instance.GetPath(monster.monsterData.X, monster.monsterData.Y, attackIndex, attackIndeY, speedRange);

        if (poslist.Count == 1)
        {
            Debug.Log("没有路线可以到目标点 原地不动");
            monster.SendMsg((ushort)ILNpcEventMap.HideMapTip);
            HUDMsg hMsg = new HUDMsg((ushort)ILUIEvent.ShowHUDBlood);
            monster.SendMsg(hMsg);


            MapMsg obs = new MapMsg(monster.monsterData.X, monster.monsterData.Y, true, (ushort)ILNpcEventMap.SetObs);
            monster.SendMsg(obs);
            monster.SendMsg((ushort)ILNpcEventMap.UpdateMap);

            if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
            {
                if (!isSkillOrAttack(true))
                {
                    Debug.Log("目标依然不在攻击和施法范围之内 结束怪物操作");
                    EndRound();
                    return;
                }
            }
            else
            {
                if (!isAttackOrSkill(true))  //移动完成后 执行攻击判断
                {
                    Debug.Log("目标依然不在攻击和施法范围之内 结束怪物操作");
                    EndRound();
                    return;
                }
            }
            return;
        }
        MapMsg mapMsgtmp = new MapMsg(poslist[poslist.Count - 1].X, poslist[poslist.Count - 1].Y, true, (ushort)ILNpcEventMap.SetObs);
        monster.SendMsg(mapMsgtmp);
        monster.SendMsg((ushort)ILNpcEventMap.UpdateMap);

        Vector3[] path = new Vector3[poslist.Count];
        for (int i = 0; i < poslist.Count; i++)
        {
            path[i] = poslist[i].transform.position;
        }
        tmpLocalPos = new Vector2(poslist[poslist.Count - 1].X, poslist[poslist.Count - 1].Y);
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Run);
        //Debug.Log("path.Length*speed:::::::"+ path.Length * speed);
        monster.transform.DOPath(path, path.Length * speed).SetLookAt(0.1f).OnComplete(EndPatrol);


    }

    /// <summary>
    /// 移动动画完成 结束寻路
    /// </summary>
    public void EndPatrol()
    {
        Debug.Log("寻路结束");
        monster.SendMsg((ushort)ILNpcEventMap.HideMapTip);
        monster.UpdateLocation((int)tmpLocalPos.x, (int)tmpLocalPos.y);
        // monster.monsterGameObject.transform.LookAt(player.transform.position);
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.ShowHUDBlood);
        monster.SendMsg(hUDMsg);
        Debug.Log("再次判断是否可以攻击或者施法");
        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
        {
            if (!isSkillOrAttack(true))
            {
                Debug.Log("目标依然不在攻击和施法范围之内 结束怪物操作");
                EndRound();
                return;
            }
        }
        else
        {
            if (!isAttackOrSkill(true))  //移动完成后 执行攻击判断
            {
                Debug.Log("目标依然不在攻击和施法范围之内 结束怪物操作");
                EndRound();
                return;
            }
        }


    }

    /// <summary>
    /// 玩家攻击或者施法距离是否足够
    /// </summary>
    /// isMove 移动前 还是移动后检测
    public bool isAttackOrSkill(bool isMove = false)
    {
        if (Attack(isMove))
        {
            Debug.Log("目标在攻击范围内 发动攻击");
            EnterFight(attackTipType);
            return true;
        }
        else if (Skill(isMove))
        {
            Debug.Log("目标在施法范围内 发动法术攻击");
            EnterFight(skillTipType, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 玩家施法或者攻击距离是否足够
    /// </summary>Move 移动前还是移动后
    public bool isSkillOrAttack(bool isMove = false)
    {
        if (Skill(isMove))
        {
            Debug.Log("目标在施法范围内 发动法术攻击");
            EnterFight(skillTipType, true);
            return true;
        }
        else if (Attack(isMove))
        {
            Debug.Log("目标在攻击范围内 发动攻击");
            EnterFight(attackTipType);
            return true;
        }
        return false;
    }


    public void EndRound()//回合结束
    {
        monster.monsterContorller.NextStartRound();
        //按钮测试 关闭自动结束回合==========

    }
    public void Destory()
    {
        //monsterContorller = null;
        //monster = null;
        player = null;

        PlayerList = null;

        BuildBases = null;
    }
}
