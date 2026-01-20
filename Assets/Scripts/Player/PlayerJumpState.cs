using UnityEngine;
public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) 
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 调试：记录跳跃信息
        //Debug.Log($"跳跃力: {player.jumpForce}, 帧率: {1 / Time.deltaTime:F1} FPS, 时间: {Time.time:F3}");
        player.SetVerticalVelocity(player.jumpForce);
    }

    public override void Update()
    {
        base.Update();
        
        // 空中移动
        Vector3 moveInput = player.GetMoveInput();
        if (moveInput.magnitude > 0.1f)
        {
            player.SetVelocity(moveInput, player.moveSpeed * player.airControl);
        }

        if (player.IsGroundDetected() && player.rb.velocity.y <= 0.1f) // 允许微小的正速度容差
        {
            // 根据输入决定切换到哪个状态
            if (moveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState(player.fastRunState); // 有输入就进入移动状态
            }
            else
            {
                stateMachine.ChangeState(player.standByState); // 无输入进入闲置状态
            }
        }
    }
}