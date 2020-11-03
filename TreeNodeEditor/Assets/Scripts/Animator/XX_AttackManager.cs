using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Demos;
using UnityEngine;

public class XX_AttackManager : MonoBehaviour
{
    private CharacterThirdPerson characterController;

    private Animator animator;

    /// <summary>
    /// 动画遮罩层级
    /// </summary>
    int baseLayer
    {
        get { return animator.GetLayerIndex("Base Layer"); }
    }

    int upBodyLayer
    {
        get { return animator.GetLayerIndex("UpBody"); }
    }

    int downBodyLayer
    {
        get { return animator.GetLayerIndex("DownBody"); }
    }

    //动画树得参数
    private string attackHor = "attackHor";
    private string attackVer = "attackVer";

    #region StateInfo

    private List<XX_AnimationStateInfo> _stateInfos = new List<XX_AnimationStateInfo>();

    private XX_AnimationStateInfo _curStateInfo;

    private float _curStateTime;

    private XX_AnimationStateInfo _normalStateInfo;


    #endregion


    #region 层级权值参数

    private float tempUpLayerTime;
    private float upLayerTime = 0.5f;
    private float lastUpLayerWeight;
    private float endUpLayerWeight;
    private float tempDownLayerTime;
    private float downLayerTime = 0.5f;
    private float lastDownLayerWeight;
    private float endDownLayerWeight;

    #endregion

    public void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterThirdPerson>();
        InitStateInfos();
    }

    public void Update()
    {
        #region Update LayerWeight

        if (tempUpLayerTime > 0)
        {
            tempUpLayerTime -= Time.deltaTime;
            var t = 1 - tempUpLayerTime / upLayerTime;
            animator.SetLayerWeight(upBodyLayer, Mathf.Lerp(lastUpLayerWeight, endUpLayerWeight, t));
        }

        if (tempDownLayerTime > 0)
        {
            tempDownLayerTime -= Time.deltaTime;
            var t = 1 - tempDownLayerTime / downLayerTime;
            animator.SetLayerWeight(downBodyLayer, Mathf.Lerp(lastDownLayerWeight, endDownLayerWeight, t));
        }

        #endregion

        if (_curStateInfo != null)
        {
            _curStateTime += Time.deltaTime;
            if (_curStateTime > _curStateInfo.AnimationLength)
            {
                OnExitState(_curStateInfo);
            }
        }

        OnAttack();
    }

    void OnAttack()
    {
        bool canAttack = characterController.animState.attackIndex != 0;
        canAttack &= _curStateInfo == null ||(_curStateInfo != null && _curStateTime > _curStateInfo.BreakTime);

        if (!canAttack)
        {
            return;
        }

        //TODO 获取合适得状态
        SetCurStateInfo(_normalStateInfo);
        animator.SetFloat(attackHor,1);
        
        bool isAction = !characterController.animState.onGround;
        isAction &= characterController.animState.run;
        isAction &= characterController.animState.walk;
        isAction &= characterController.animState.crouch;

        SetUpBodyLayerWeight(1);
        SetDownBodyLayerWeight(isAction ? 0 : 1.0f);

    }

    /// <summary>
    /// 设置上部分身体权值
    /// </summary>
    /// <param name="weight"></param>
    void SetUpBodyLayerWeight(float weight)
    {
        endUpLayerWeight = weight;
        tempUpLayerTime = upLayerTime;
        lastUpLayerWeight = animator.GetLayerWeight(upBodyLayer);
    }

    /// <summary>
    /// 设置下部分身体权值
    /// </summary>
    /// <param name="weight"></param>
    void SetDownBodyLayerWeight(float weight)
    {
        endDownLayerWeight = weight;
        tempDownLayerTime = downLayerTime;
        lastDownLayerWeight = animator.GetLayerWeight(downBodyLayer);
    }

    void SetCurStateInfo(XX_AnimationStateInfo stateInfo)
    {
        _curStateInfo = stateInfo;
        _curStateTime = 0;
    }
    
    /// <summary>
    /// 当状态离开
    /// </summary>
    /// <param name="stateInfo"></param>
    void OnExitState(XX_AnimationStateInfo stateInfo)
    {
        SetUpBodyLayerWeight(0);
        SetDownBodyLayerWeight(0);
        animator.SetFloat(attackHor,0);
        animator.SetFloat(attackVer,0);
        _curStateTime = 0;
        _curStateInfo = null;
    }
    
    /// <summary>
    /// 初始化攻击动画
    /// </summary>
    void InitStateInfos()
    {
        _normalStateInfo = new XX_AnimationStateInfo(animator, "UpBody.OrientalSword_COMBOATTACK01", 1f);
        _stateInfos.Add(_normalStateInfo);
    }

   

}
