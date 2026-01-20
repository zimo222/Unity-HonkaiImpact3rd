using UnityEngine;

public class PlayerStandByState : PlayerGroundedState
{
    public PlayerStandByState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        Vector3 moveInput = player.GetMoveInput();

        if (moveInput.magnitude > 0.1f && !player.isBusy)
        {
            stateMachine.ChangeState(player.fastRunState);
        }

        if (Input.GetKeyDown(KeyCode.J) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.primaryAttackState);
        }
        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }
        if (Input.GetKeyDown(KeyCode.K) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.evadeBackwardState);
        }
        /*
        if (Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.primaryAttack);
        }
        */
    }
}