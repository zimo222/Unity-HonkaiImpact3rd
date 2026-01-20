using UnityEngine;

public class PlayerEvadeBackwardState : PlayerGroundedState
{
    private Vector3 evadeDirection;
    public PlayerEvadeBackwardState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 关键1：在状态开始时，确定本次闪避的固定方向
        // 取角色当前面向方向的反方向作为冲刺方向
        Vector3 backwardDirection = -player.facingDir;

        // 存储这个方向，以便在Update中持续使用，避免中途facingDir变化
        // 你可能需要在PlayerDashState类中声明一个 private Vector3 dashDirection; 来存储它
        evadeDirection = backwardDirection;

        // 关键2：初始施加一次反向速度
        player.SetVelocity(evadeDirection, player.evadeSpeed, false);

        //player.skill.clone.CreateClone(player.transform);

        stateTimer = player.dashDuration;
    }

    public override void Exit()
    {
        base.Exit();
        // 关键4：退出冲刺状态时，立即将水平速度归零，但保持垂直速度（如重力下落）
        // 方法：调用SetVelocity并传入零向量，这会将x和z速度归零，y速度保持不变
        player.SetVelocity(Vector3.zero, 0);
    }

    public override void Update()
    {
        base.Update();

        // 关键3：在Update中持续应用反向速度，保持冲刺
        // 使用Enter中存储的固定方向，而不是每帧重新计算
        player.SetVelocity(evadeDirection, player.evadeSpeed, false);

        if (stateTimer < 0)
        {        // 根据输入决定切换至移动还是闲置状态
            if (player.GetMoveInput().magnitude > 0.1f)
            {
                stateMachine.ChangeState(player.fastRunState);
            }
            else
            {
                stateMachine.ChangeState(player.standByState);
            }
        }
    }
}
