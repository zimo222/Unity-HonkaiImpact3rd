//玩家状态机，负责管理和切换玩家当前所处的状态（如 idle, move, jump）
public class PlayerStateMachine
{
    //当前正在运行的状态，只允许本状态机内部设置
    public PlayerState currentState { get; private set; }

    //初始化状态机，并进入指定的起始状态
    public void Initialize(PlayerState _startState)
    {
        currentState = _startState;//设定初始状态
        currentState.Enter();//调用初始状态的进入逻辑
    }

    //执行状态切换，这是状态机运行的核心
    public void ChangeState(PlayerState _newState)
    {
        currentState.Exit();//首先让当前状态执行清理逻辑
        currentState = _newState;//将内部指针指向新的状态
        currentState.Enter();//最后让新状态执行初始化逻辑
    }
}