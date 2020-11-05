
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

    /// <summary>
    /// 是否可行走
    /// </summary>
    public bool UnAction = false;

    /// <summary>
    /// 动画初始帧
    /// </summary>
    public float StartFrame;

    /// <summary>
    /// 可施放下一个攻击的时间
    /// </summary>
    public float GetNextAttackTime;
    
    /// <summary>
    /// 动画片段
    /// </summary>
    private AnimationClip _animationClip;
    
    
    
    public XX_AnimationStateInfo(Animator animator,string animationState,float breakTime,bool unAction,float getNextAttackTime)
    {
        _animator = animator;
        AnimationName = animationState.Split('.')[1];
        AnimationStateHash = Animator.StringToHash(animationState);
        BreakTime = breakTime;
        UnAction = unAction;
        GetNextAttackTime = getNextAttackTime;
        StartFrame = 0;
        //获取片段
        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals(AnimationName))
            {
                _animationClip = clip;
                break;
            }
        }

        AnimationLength = _animationClip.length;

        Debug.Log("animationState:" + AnimationLength);
        if (BreakTime > AnimationLength)
        {
            Debug.LogError($"中断时间大于动画时间：中断时间为：{BreakTime}，动画时间为：{AnimationLength}");
        }
    }
    
    public XX_AnimationStateInfo(Animator animator,string animationState,float breakTime,bool unAction,float startFrame,float animationLength,float getNextAttackTime)
    {
        _animator = animator;
        AnimationName = animationState.Split('.')[1];
        AnimationStateHash = Animator.StringToHash(animationState);
        BreakTime = breakTime;
        UnAction = unAction;
        StartFrame = startFrame;
        GetNextAttackTime = getNextAttackTime;

        //获取片段
        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals(AnimationName))
            {
                _animationClip = clip;
                break;
            }
        }

        AnimationLength = animationLength;

        if (BreakTime > AnimationLength)
        {
            Debug.LogError($"中断时间大于动画时间：中断时间为：{BreakTime}，动画时间为：{AnimationLength}");
        }
    }

    
    public void PlayRestartFrame(float frameIndex)
    {
        _animator.gameObject.SetActive(false);
        _animator.gameObject.SetActive(true);
        //更新动画到指定帧
        //_animator.Play(AnimationStateHash,la);
        _animator.Update(frameIndex);
    }

    public void PlayUpdate(float frameIndex,int layer)
    {
        _animator.Update(frameIndex);
    }
}
