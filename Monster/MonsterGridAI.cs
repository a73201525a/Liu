using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum PracticeType
{
    Attack,//����
    Skill,//�˺�����
    GainSkill,//���ѷ�ʹ�õ����漼��
}

public enum GridMonsterAI
{
    Idle,//վ��AI���ڹ�����Χ�ڳ��ֵ��˵�ʱ�򣬲Ż�����������
    Initiative,//����Ѱ�ҵ��˵�AI
}

//������ʾ����
public enum AttackTipType
{
    Cube,//����
    Lozenge,//���� 
    Ranged,//Զ��
    Cross,//ʮ��
}

public enum AIAttackType
{
    weakness,//����Ѱ�ҿ��ƹ�ϵ�ĵ��˹���������˲���  �����˹���  �����������
    near,//����Ѱ������ĵ��˹���
    enchanter, //�Ƿ����ȹ�����ʦ

    siege,//���ȹ�������

    skill,//����ʹ�ü���
}
/// <summary>
/// ����
/// </summary>
public enum SolderType
{
    Null,
    infantry,//����
    cavalry,//���
    archer,//������
    enchanter,//��ʦ
}
public class MonsterGridAI
{
    List<Player> PlayerList;//���list
    List<BuildItemBase> BuildBases;//����List
    List<MonsterBase> monsterList;//����List

    //����Ŀ��
    Player player;

    //�������漼��Ŀ��
    MonsterBase monsterTagetr;
    //Ŀ�꽨��
    BuildItemBase build;
    //���ι���Ŀ����ǽ���
    bool isAttackBuild;
    //���ﱾ�غ����ȵĲ���
    PracticeType practiceType;

    //���غ�׼��ʹ�õļ���ID
    int curSkillID;

    //�ƶ������Թ���Ŀ����λ��
    int attackIndex;
    int attackIndeY;


    //�н�·���У������������Թ�����Ŀ�꣨��ʱĿ�꣩
    Player curAttackTagetPlayer;

    //�н�·���У����������Ĺ���Ŀ�꣨��ʱĿ�꣩
    MonsterBase curMonsterTaget;

    //�������
    MonsterBase monster;

    //ÿ�غ��ƶ�����
    int speedRange;

    //Ѳ�߷�Χ
    public int patrolRange;

    //ʩ����Χ
    int castingRange;

    //������Χ
    int attckRange;
    /// <summary>
    /// �ƶ��ٶ�
    /// </summary>
    float speed = 0.5f;
    /// <summary>
    /// ����tip ����
    /// </summary>
    public AttackTipType attackTipType;

    /// <summary>
    /// ʩ����ʾ ����
    /// </summary>
    AttackTipType skillTipType = AttackTipType.Lozenge;



    SolderType solderType;//����


    /// <summary>
    /// AI����
    /// </summary>
    GridMonsterAI aiType;
    /// <summary>
    ///  ��������
    /// </summary>
    AIAttackType aiAttackType;



    /// <summary>
    /// ���ÿ��Թ���Ŀ����λ��
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
    /// ����AI
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
    /// <param name="players">���list</param>
    /// <param name="buildList">����</param>
    /// <param name="monsterContorller">�������</param>
    public void FixedUpadte(List<Player> players, List<BuildItemBase> buildList, List<MonsterBase> monsters)//ÿ�غ�ִ��һ�� ����ҽ����غϺ󣬵�����غϣ�
    {

        PlayerList = players;
        BuildBases = buildList;
        monsterList = monsters;
        isAttackBuild = false;
        monster.monsterContorller.MonsterPointEntrer = true;//�����Զ��غ�,������������

        Debug.Log("�ƶ�����:" + speedRange);
        Debug.Log("Ѳ�߾���:" + patrolRange);
        Debug.Log("��������:" + attckRange);
        Debug.Log("ʩ������:" + castingRange);
        Debug.Log("����::::" + solderType);
        Debug.Log("�ƶ�����:::::" + aiType);
        Debug.Log("��������:::::" + aiAttackType);
        Debug.Log("������ʾ����:::::" + attackTipType);
        Debug.Log("PlayerList:::::" + PlayerList.Count);
        Debug.Log("BuildBases:::::" + BuildBases.Count);


        monsterGridAITool.IsPractice(monster, aiAttackType, out curSkillID, out practiceType);
        //  practiceType = PracticeType.GainSkill;
        //aiAttackType = AIAttackType.siege;
        bool isTarget = false;
        Debug.Log("practiceType::" + practiceType);
        if (practiceType == PracticeType.Skill)//�����ü���
        {

            isTarget = monsterGridAITool.CheckTargetSkill(PlayerList, monsterList, aiAttackType, practiceType, monster, out player, out monsterTagetr);//�Ҽ���Ŀ��
        }
        else if (practiceType == PracticeType.GainSkill)
        {
            isTarget = monsterGridAITool.CheckTargetSkill(PlayerList, monsterList, aiAttackType, practiceType, monster, out player, out monsterTagetr);//�Ҽ���Ŀ��
        }
        else if (practiceType == PracticeType.Attack && aiAttackType == AIAttackType.siege)//Ŀ�����Ƚ���
        {
            isTarget = monsterGridAITool.CheckBuildTarget(buildList, aiAttackType, solderType, monster, out build);
        }
        else if (practiceType == PracticeType.Attack)
        {
            isTarget = monsterGridAITool.CheckTarget(PlayerList, aiAttackType, solderType, monster, out player);//��Ŀ��
        }



        Debug.Log("�Ƿ��ҵ���Ŀ�꣺��" + isTarget);

        if (isTarget)
        {


            if (practiceType == PracticeType.Attack && aiAttackType == AIAttackType.siege)
            {
                Debug.Log("����Ŀ�� build����" + build.buildID);

                SetFightTagert(build.IndexX, build.IndexY, attckRange, attackTipType);
            }
            else if (practiceType == PracticeType.Attack)
            {
                Debug.Log("����Ŀ�꣺��" + player.playerData.playerJson.name);
                Debug.Log("����Ŀ��ı���Type����" + player.playerData.playerJson.solderType);

                SetFightTagert(player.playerData.MapIndexX, player.playerData.MapIndexY, attckRange, attackTipType);
            }
            else if (practiceType == PracticeType.Skill)
            {
                Debug.Log("ʩ��Ŀ�꣺��" + player.playerData.playerJson.name);

                SetFightTagert(player.playerData.MapIndexX, player.playerData.MapIndexY, castingRange, skillTipType);
            }
            else if (practiceType == PracticeType.GainSkill)
            {
                Debug.Log("����ʩ��Ŀ�꣺��" + monsterTagetr.transform.gameObject.name);
                SetFightTagert(monsterTagetr.monsterData.X, monsterTagetr.monsterData.Y, castingRange, skillTipType);
            }

        }


        switch (aiType)//�費��Ҫ�ƶ�
        {
            case GridMonsterAI.Idle:
                {
                    Debug.Log("����Idle AI �߼�");
                    if (isTarget)
                    {
                        Debug.Log("����Ŀ��");
                        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
                        {
                            Debug.Log("Ŀ��Ϊ����ʩ��");
                            if (!isSkillOrAttack())
                            {
                                Debug.Log("ʩ���͹�����Χ��û��Ŀ��  ������������AI����");
                                EndRound();
                            }
                        }
                        else
                        {
                            if (!isAttackOrSkill())
                            {
                                Debug.Log("ʩ���͹�����Χ��û��Ŀ��  ������������AI����");
                                EndRound();
                            }
                        }

                    }
                    else
                    {
                        Debug.Log("������ʩ����Χ�ھ�δ����Ŀ�� ��������Ĳ���");
                        EndRound();
                    }
                }

                break;
            case GridMonsterAI.Initiative:
                {
                    Debug.Log("����Initiative AI �߼�");
                    if (isTarget)
                    {
                        Debug.Log("����Ŀ��");
                        Patrol();
                    }
                    else
                    {
                        Debug.Log("Ѳ�߷�Χ��δ����Ŀ�� ��������Ĳ���");
                        EndRound();
                    }
                }
                break;
        }
    }



    /// <summary>
    /// ʩ�������Ƿ��㹻
    /// </summary>
    /// <returns></returns>
    public bool Skill(bool isMove)
    {
        //������ʩ���ĵ�λ�����ƶ� ���ж��Ƿ�ʩ��
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
        else if (practiceType == PracticeType.Attack)//���ȹ����Ĺ��� �������벻�� �鿴�Ƿ��з�������ʩ��
        {
            Debug.Log("���ȹ����Ĺ��� �������벻�� �鿴�Ƿ��з�������ʩ��");
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
                Debug.Log("����ʩ��Ŀ������ڷ�Χ��");
                curMonsterTaget = monsterTagetr;
                return true;
            }
        }
        if (isMove)
        {
            Debug.Log("Ŀ������ڷ�Χ�ڣ��ƶ���ɺ� Ѱ�ҷ�Χ����û�������������Ŀ�����");
            for (int i = 0; i < monsterList.Count; i++)
            {
                for (int j = 0; j < tmplist.Count; j++)
                {
                    if (tmplist[j].X == monsterList[i].monsterData.X && tmplist[j].Y == monsterList[i].monsterData.Y && !monsterList[i].isDeath)
                    {
                        Debug.Log("�ƶ���ɺ� Ѱ�ҷ�Χ����û�������ɹ�������,�ҵ�����ʩ��Ŀ������ڷ�Χ��");
                        curMonsterTaget = monsterList[i];
                        return true;
                    }
                }

            }
        }

        Debug.Log("û���ҵ��κ�����ʩ������");
        return false;
    }


    /// <summary>
    /// Ѱ���ܹ�����ʩ����Ŀ��
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
                    Debug.Log("Ŀ�꽨���ڷ�Χ��");
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
                    Debug.Log("Ŀ������ڷ�Χ��");
                    curAttackTagetPlayer = player;
                    return true;
                }

            }
        }

        if (isMove)
        {
            Debug.Log("Ŀ������ڷ�Χ�ڣ��ƶ���ɺ� Ѱ�ҷ�Χ����û�������ɹ�������");
            for (int i = 0; i < PlayerList.Count; i++)
            {
                for (int j = 0; j < tmplist.Count; j++)
                {
                    if (tmplist[j].X == PlayerList[i].playerData.MapIndexX && tmplist[j].Y == PlayerList[i].playerData.MapIndexY && !PlayerList[i].isDeath)
                    {
                        Debug.Log("�ƶ���ɺ� Ѱ�ҷ�Χ����û�������ɹ�������,�ҵ���������Ŀ������ڷ�Χ��");
                        curAttackTagetPlayer = PlayerList[i];
                        return true;
                    }
                }

            }

        }
        Debug.Log("û���ҵ��κι�������");
        return false;
    }

    /// <summary>
    /// �Ƿ��й�������
    /// </summary>
    /// <returns></returns>
    public bool Attack(bool isMove)
    {
        //type ������Χ���Ͳ�ͬ 
        if (monster.isAttack)
        {
            Debug.Log("���ﱻ�����˹���,�޷�����");
            return false;
        }

        List<MapHex> tmplist = HotMapController.Instance.GetAttackTip(monster.monsterData.X,
     monster.monsterData.Y, attckRange, attackTipType);

        return CheckAttackPlayer(isMove, tmplist, true);

    }


    /// <summary>
    /// ����ս��
    /// </summary>
    public async void EnterFight(AttackTipType attacktype, bool isSkill = false)
    {
        ///=================ս������ ������==================

        Debug.Log("����ս������");
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.UpdatePlayer, monster.transform.position, UpdatePlayerType.UpdateCameraFollowTarget);
        monster.SendMsg(playerMsg);
        Debug.Log("���λ�ø��µ���ǰ����ս������ʩ��λ��");
        await Task.Delay(1000);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.ShowHUDBlood);
        monster.SendMsg(hUDMsg);

        MapMsg mapMsg = new MapMsg((ushort)ILNpcEventMap.ShowAttackTip);
        monster.SendMsg(mapMsg);


        await Task.Delay(1000);
        monster.SendMsg((ushort)ILNpcEventMap.HideAttckTip);//���ع�����ʾ

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
            Debug.Log("����ʩ������");
            //==============TODO::��Ҫʩ������===========
            monster.monsterAnimation.SetAnimation(MonsterAnimationType.Skill);
            Debug.Log("ʩ����Ч����");
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
        else//������
        {
            if (isAttackBuild)
            {
                ChooseAnimation(build.buildGameObject.transform.position, build.IndexX, build.IndexY);
                await Task.Delay(2000);
                Debug.Log("���Ź�������");
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);
                await Task.Delay(3000);
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
                //��ұ�����
                BuildMsg msg = new BuildMsg((ushort)AdventureBuildEvent.BuildByAttack, build.buildGameObject, monster.NpcID, RoundType.Monster);//��ҹ����з�����
                monster.SendMsg(msg);
            }
            else
            {
                ChooseAnimation(curAttackTagetPlayer.transform.position, curAttackTagetPlayer.playerData.MapIndexX, curAttackTagetPlayer.playerData.MapIndexY);
                await Task.Delay(2000);
                Debug.Log("���Ź�������");
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);
                await Task.Delay(3000);
                monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
                //��ұ�����
                PlayerDataMsg playerDataMsg = new PlayerDataMsg((ushort)ILNpcEvent.PlayerByAttack, curAttackTagetPlayer.playerID, monster);
                monster.SendMsg(playerDataMsg);

            }

        }



    }

    void ChooseAnimation(Vector3 pos, int X, int Y)
    {
        monster.transform.DOLookAt(pos, 0.1f);
        Debug.Log("��ʾ����ѡ��Ŀ�궯��");
        MapMsg mapMsg = new MapMsg(X, Y, (ushort)ILNpcEventMap.ShowAttackTargetTip);
        monster.SendMsg(mapMsg);
    }
    /// <summary>
    /// �ܷ񷴻�
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
    /// ���ﷴ��
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public async void CounterAttack(Player player)
    {
        await Task.Delay(1000);

        if (monster.isDeath)
        {
            //PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.PlayerRoundEnd, player.NpcID);//��һغϽ���
            //monster.SendMsg(playerMsg);
            return;
        }

        monster.transform.LookAt(player.transform.position);
        Debug.Log("���ﲥ�ŷ�������");
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Attack);//���ﷴ��
        await Task.Delay(3000);
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);

        //��ұ�����
        PlayerDataMsg playerDataMsg = new PlayerDataMsg((ushort)ILNpcEvent.PlayerCounterAttack, player.playerID, monster);
        monster.SendMsg(playerDataMsg);


    }
    /// <summary>
    /// ������Χ���
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="attackRange"></param>
    /// <param name="attackTip"></param>
    /// <param name="player">�������Ŀ��</param>
    /// <returns></returns>
    bool CounterAttackAttackRange(Player player)
    {

        List<MapHex> tmpList = HotMapController.Instance.GetAttackTip(monster.monsterData.X, monster.monsterData.Y, attckRange, attackTipType);

        for (int i = 0; i < tmpList.Count; i++)
        {
            if (tmpList[i].X == player.playerData.MapIndexX && tmpList[i].Y == player.playerData.MapIndexY)
            {
                Debug.Log("�����ڷ�����Χ��");
                monster.transform.LookAt(player.ModelTool.PlayerGameObject.transform);

                return true;
            }
        }
        Debug.Log("���ﲻ�ڷ�����Χ��");
        return false;
    }
    /// <summary>
    /// Ѳ��
    /// </summary>
    public void Patrol()
    {

        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
        {
            Debug.Log("����Ϊ����ʩ������");
            if (isSkillOrAttack())
            {
                return;
            }
        }
        else
        {
            Debug.Log("����Ϊ���ȹ�������");
            if (isAttackOrSkill())
            {
                return;
            }
        }
        Debug.Log("Ŀ�겻�ڹ�����ʩ����Χ�� ����Ѳ���߼�");
        Debug.Log("monster.isMove:::" + monster.isMove);
        if (monster.isMove)
        {
            //����ֹ�ƶ� �����ƶ� Ѱ���������Ŀ��
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
                if (!isAttackOrSkill(true))  //�ƶ���ɺ� ִ�й����ж�
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
    /// �ƶ�����
    /// </summary>
    public async void RoadPatrol()
    {

        await Task.Delay(1000);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.HideHUDBlood);
        monster.SendMsg(hUDMsg);
        PlayerMsg playerMsg = new PlayerMsg((ushort)ILNpcEvent.UpdatePlayer, monster.transform.position, UpdatePlayerType.UpdateCameraFollowTarget);
        monster.SendMsg(playerMsg);
        Debug.Log("���λ�ø��µ���ǰ����λ��");
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
            Debug.Log("û��·�߿��Ե�Ŀ��� ԭ�ز���");
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
                    Debug.Log("Ŀ����Ȼ���ڹ�����ʩ����Χ֮�� �����������");
                    EndRound();
                    return;
                }
            }
            else
            {
                if (!isAttackOrSkill(true))  //�ƶ���ɺ� ִ�й����ж�
                {
                    Debug.Log("Ŀ����Ȼ���ڹ�����ʩ����Χ֮�� �����������");
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
    /// �ƶ�������� ����Ѱ·
    /// </summary>
    public void EndPatrol()
    {
        Debug.Log("Ѱ·����");
        monster.SendMsg((ushort)ILNpcEventMap.HideMapTip);
        monster.UpdateLocation((int)tmpLocalPos.x, (int)tmpLocalPos.y);
        // monster.monsterGameObject.transform.LookAt(player.transform.position);
        monster.monsterAnimation.SetAnimation(MonsterAnimationType.Idle);
        HUDMsg hUDMsg = new HUDMsg((ushort)ILUIEvent.ShowHUDBlood);
        monster.SendMsg(hUDMsg);
        Debug.Log("�ٴ��ж��Ƿ���Թ�������ʩ��");
        if (practiceType == PracticeType.Skill || practiceType == PracticeType.GainSkill)
        {
            if (!isSkillOrAttack(true))
            {
                Debug.Log("Ŀ����Ȼ���ڹ�����ʩ����Χ֮�� �����������");
                EndRound();
                return;
            }
        }
        else
        {
            if (!isAttackOrSkill(true))  //�ƶ���ɺ� ִ�й����ж�
            {
                Debug.Log("Ŀ����Ȼ���ڹ�����ʩ����Χ֮�� �����������");
                EndRound();
                return;
            }
        }


    }

    /// <summary>
    /// ��ҹ�������ʩ�������Ƿ��㹻
    /// </summary>
    /// isMove �ƶ�ǰ �����ƶ�����
    public bool isAttackOrSkill(bool isMove = false)
    {
        if (Attack(isMove))
        {
            Debug.Log("Ŀ���ڹ�����Χ�� ��������");
            EnterFight(attackTipType);
            return true;
        }
        else if (Skill(isMove))
        {
            Debug.Log("Ŀ����ʩ����Χ�� ������������");
            EnterFight(skillTipType, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ���ʩ�����߹��������Ƿ��㹻
    /// </summary>Move �ƶ�ǰ�����ƶ���
    public bool isSkillOrAttack(bool isMove = false)
    {
        if (Skill(isMove))
        {
            Debug.Log("Ŀ����ʩ����Χ�� ������������");
            EnterFight(skillTipType, true);
            return true;
        }
        else if (Attack(isMove))
        {
            Debug.Log("Ŀ���ڹ�����Χ�� ��������");
            EnterFight(attackTipType);
            return true;
        }
        return false;
    }


    public void EndRound()//�غϽ���
    {
        monster.monsterContorller.NextStartRound();
        //��ť���� �ر��Զ������غ�==========

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
