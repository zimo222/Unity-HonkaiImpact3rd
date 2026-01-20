using UnityEngine;

public class PlayerFastRunState : PlayerGroundedState
{
    public PlayerFastRunState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        Vector3 moveInput = player.GetMoveInput();

        if (moveInput.magnitude > 0.1f)
        {
            player.SetVelocity(moveInput, player.moveSpeed);
        }
        else
        {
            stateMachine.ChangeState(player.standByState);
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