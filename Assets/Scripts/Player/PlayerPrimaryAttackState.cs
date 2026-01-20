using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{

    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 重置输入
        xInput = 0;
        zInput = 0;

        // 如果是连续攻击，保持原有comboCounter
        // 如果是新的一轮连击，重置comboCounter
        bool isNewCombo = false;

        if (Time.time >= player.lastTimeAttacked + player.comboWindow)
        {
            player.comboCounter = 0;
            isNewCombo = true;
        }
        else if (player.comboCounter > 4) // 如果超过最大连击数
        {
            player.comboCounter = 0;
            isNewCombo = true;
        }

        // 设置动画参数
        player.anim.SetInteger("ComboCounter", player.comboCounter);
        player.anim.SetBool("IsNewCombo", isNewCombo);

        // 重置连击相关标志
        player.canCombo = false;
        player.comboInput = false;

        // 播放攻击动画
        PlayAttackAnimation();

        // 状态计时器（攻击硬直时间）
        stateTimer = 0.3f; // 基础硬直时间

        // 记录攻击时间
        player.lastTimeAttacked = Time.time;
    }

    private void PlayAttackAnimation()
    {
        // 根据comboCounter播放不同的动画
        // Attack1对应comboCounter=0, Attack2对应comboCounter=1, 以此类推
        string animationName = "Attack" + (player.comboCounter + 1);
        player.anim.Play(animationName);

        // 根据连击数设置不同的硬直时间
        // 越高的连击数，硬直时间越长
        float[] comboDurations = { 0.5f, 0.6f, 0.7f, 0.8f, 1.0f };
        float comboDuration = player.comboCounter < comboDurations.Length ?
            comboDurations[player.comboCounter] : 1.0f;

        // 设置动画事件监听
        player.anim.Update(0); // 立即更新一帧以确保动画开始播放
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", 0.15f);

        // 如果在退出时没有触发连击，则重置连击计数
        if (!player.comboInput)
        {
            // 可以保留当前连击数一段时间，或者直接重置
            // comboCounter = 0; // 可选
        }
    }

    public override void Update()
    {
        base.Update();

        // 检查连击输入
        if (Input.GetKeyDown(KeyCode.J)) // 或者您使用的攻击按钮
        {
            player.comboInput = true;

            // 如果在可以连击的窗口内，增加连击计数
            if (player.canCombo && player.comboCounter < 4) // 最大连击数为5，对应comboCounter 0-4
            {
                player.comboCounter++;
                stateMachine.ChangeState(player.primaryAttackState);
                return;
            }
        }

        // 攻击硬直时间结束后停止移动
        if (stateTimer < 0)
        {
            player.SetZeroVelocity();
        }

        // 动画播放完成后切换状态
        if (triggerCalled)
        {
            CheckNextState();
        }
    }

    private void CheckNextState()
    {
        Vector3 moveInput = player.GetMoveInput();
        if (moveInput.magnitude > 0.1f)
        {
            stateMachine.ChangeState(player.fastRunState);
        }
        else
        {
            stateMachine.ChangeState(player.standByState);
        }
    }
}