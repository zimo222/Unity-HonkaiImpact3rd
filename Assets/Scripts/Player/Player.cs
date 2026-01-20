using System.Collections;
using UnityEngine;

public class Player : Entity
{
    public float moveSpeed => MoveSpeed;  // 如果保持基类moveSpeed为protected

    [Header("Attack details")]
    public Vector3[] attackMovement;
    public float counterAttackDuration = 0.2f;

    public int comboCounter = 0;
    public bool canCombo = false; // 是否可以触发下一次连击
    public bool comboInput = false; // 是否有连击输入

    public float lastTimeAttacked;
    public float comboWindow = 5;

    public bool isBusy { get; private set; }

    [Header("Movement")]
    public float jumpForce = 30f;
    public float airControl = 0.8f;
    public float gravityMultiplier = 1f;

    [Header("Evade info")]
    public float evadeSpeed = 00f;
    public float evadeDuration = 1f;

    [Header("Dash info")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public Vector3 dashDir { get; private set; }

    [Header("Camera")]
    public Transform cameraPivot;
    public float cameraFollowSpeed = 10f;

    // Input
    private Vector3 moveInput;
    private Vector3 cameraRelativeMovement;

    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerStandByState standByState { get; private set; }
    public PlayerFastRunState fastRunState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerEvadeBackwardState evadeBackwardState { get; private set; }
    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();

        standByState = new PlayerStandByState(this, stateMachine, "StandBy");
        fastRunState = new PlayerFastRunState(this, stateMachine, "FastRun");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        evadeBackwardState = new PlayerEvadeBackwardState(this, stateMachine, "EvadeBackward");
        primaryAttackState = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(standByState);

        // 初始化相机（如果存在）
        if (cameraPivot == null)
            cameraPivot = Camera.main?.transform;
    }

    protected override void Update()
    {
        base.Update();

        // 获取输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 将输入转换为基于相机的方向
        if (cameraPivot != null)
        {
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveInput = forward * vertical + right * horizontal;
        }
        else
        {
            moveInput = new Vector3(horizontal, 0, vertical);
        }

        moveInput.Normalize();

        stateMachine.currentState.Update();

        CheckForDashInput();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (!IsGroundDetected())
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    public IEnumerator BusyFor(float seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    protected void CheckForDashInput()
    {
        if (IsObstacleInFront(1f))
            return;
        /*
        if (Input.GetKeyDown(KeyCode.LeftShift) && moveInput.magnitude > 0.1f)
        {
            dashDir = moveInput;
            stateMachine.ChangeState(dashState);
        }
        */
    }

    public Vector3 GetMoveInput() => moveInput;

    public bool IsMoving() => moveInput.magnitude > 0.1f;
    public void OnAttackAnimationEvent(string eventName)
    {
        if (stateMachine.currentState is PlayerPrimaryAttackState attackState)
        {
            switch (eventName)
            {
                case "EnableCombo":
                    EnableComboWindow();
                    break;
                case "DisableCombo":
                    DisableComboWindow();
                    break;
                case "DealDamage":
                    // 这里可以处理伤害判定
                    DealDamage();
                    break;
            }
        }
    }

    private void DealDamage()
    {
        // 实现伤害判定逻辑
        // 例如：创建检测区域，检测敌人等
    }

    // 动画事件调用这个方法
    public void EnableComboWindow()
    {
        canCombo = true;
    }

    // 动画事件调用这个方法
    public void DisableComboWindow()
    {
        canCombo = false;
    }

    public void ResetCombo()
    {
        comboCounter = 0;
        lastTimeAttacked = 0;
        canCombo = false;
        comboInput = false;
    }
}