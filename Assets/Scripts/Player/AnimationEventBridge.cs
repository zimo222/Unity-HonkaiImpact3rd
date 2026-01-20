using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    [Header("目标Player脚本")]
    public Player targetPlayer; // 拖入父对象上的Player脚本
    public PlayerPrimaryAttackState targetAttack; // 拖入父对象上的Player脚本

    // 动画事件调用这个方法
    public void AnimationTrigger()
    {
        if (targetPlayer != null)
        {
            Debug.Log($"[桥接] 传递动画事件到 {targetPlayer.gameObject.name}");
            targetPlayer.AnimationTrigger();
        }
        else
        {
            Debug.LogError("[桥接] 未设置目标Player脚本！");
        }
    }
    
    // 如果需要传递参数
    public void OnAnimationEvent(string eventName)
    {
        if (targetPlayer != null)
        {
            // 如果有对应方法可以调用
            targetPlayer.AnimationTrigger();
        }
    }
    public void OnAttackAnimationEvent(string eventName)
    {
        targetPlayer.OnAttackAnimationEvent(eventName);
    }    
    // 动画事件调用这个方法
    public void EnableCombo()
    {
        targetPlayer.EnableComboWindow();
    }

    // 动画事件调用这个方法
    public void DisableCombo()
    {
        targetPlayer.DisableComboWindow();
    }

    public void ResetCombo()
    {
        targetPlayer.ResetCombo();
    }

    private void DealDamage()
    {
        // 实现伤害判定逻辑
        // 例如：创建检测区域，检测敌人等
    }
}