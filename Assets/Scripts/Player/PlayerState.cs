using System.Collections;
using UnityEngine;

//玩家状态基类，定义状态模式中单个状态的模板
public class PlayerState
{
    //状态机引用，用于本状态内部请求切换到其他状态
    protected PlayerStateMachine stateMachine;
    //玩家对象引用，用于访问玩家公开的属性与方法
    protected Player player;

    //缓存的刚体组件，用于物理运动
    protected Rigidbody rb;
    //缓存的动画组件，用于控制动画状态机
    protected Animator anim;

    //存储每一帧的原始输入，供子类逻辑使用
    protected float xInput;
    protected float zInput;
    //此状态对应的动画参数名，用于驱动Animator Controller
    private string animBoolName;

    //状态内部计时器，可用于攻击硬直、状态持续时间等
    protected float stateTimer;
    //动画完成标记，通常由Animation Event触发
    protected bool triggerCalled;

    //构造函数，初始化状态必备的引用
    public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;//将逻辑状态与特定的动画绑定
    }

    //当状态被状态机切换进入时调用
    public virtual void Enter()
    {
        //1.激活本状态对应的动画布尔参数，触发动画切换
        player.anim.SetBool(animBoolName, true);
        //2.缓存常用组件引用，方便子类使用，避免频繁查找
        rb = player.rb;
        anim = player.anim;
        //3.重置触发器标记，为本次状态运行做准备
        triggerCalled = false;
    }

    //此状态处于激活时，每帧被调用（由Player类的Update驱动）
    public virtual void Update()
    {
        //更新状态计时器（递减）
        stateTimer -= Time.deltaTime;
        //获取原始输入值，为子类逻辑提供数据
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        //设置通用动画参数，这些参数可能被多个状态使用
        //MoveSpeed参数控制移动类动画的混合树（如Idle到Run的过渡）
        Vector3 moveInput = player.GetMoveInput();
        float moveMagnitude = moveInput.magnitude;
        anim.SetFloat("MoveSpeed", moveMagnitude);
        //yVelocity参数用于切换跳跃与下落动画
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    //当状态被状态机切换离开时调用
    public virtual void Exit()
    {
        //停用本状态对应的动画布尔参数，通知Animator退出相关动画
        player.anim.SetBool(animBoolName, false);
    }

    //动画完成触发器，通常由附加在动画片段上的Animation Event调用
    public virtual void AnimationFinishTrigger()
    {
        //标记动画播放完毕，子状态可据此执行后续逻辑（如连招、状态切换）
        triggerCalled = true;
    }
}