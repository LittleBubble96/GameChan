
using UnityEngine;

public class XX_AnimationStateInfo
{
    /// <summary>
    /// 动画控制器
    /// </summary>
    private Animator _animator;
    
    /// <summary>
    /// 动画hash值
    /// </summary>
    public int AnimationStateHash;

    /// <summary>
    /// 动画名字
    /// </summary>
    public string AnimationName;

    /// <summary>
    /// 动画时长
    /// </summary>
    public float AnimationLength;

    /// <summary>
    /// 中断时间
    /// </summary>
    public float BreakTime;
    
    

    public XX_AnimationStateInfo(Animator animator,string animationState,float breakTime)
    {
        _animator = animator;
        AnimationName = animationState.Split('.')[1];
        AnimationStateHash = Animator.StringToHash(animationState);
        AnimationLength = GetLength();
        BreakTime = breakTime;
        if (BreakTime > AnimationLength)
        {
            Debug.LogError($"中断时间大于动画时间：中断时间为：{BreakTime}，动画时间为：{AnimationLength}");
        }
    }

    //获取动画时长
    private float GetLength()
    {
        float length = 0;
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(AnimationName))
            {
                length = clip.length;
                break;
            }
        }
        return length;
    }
    
}
