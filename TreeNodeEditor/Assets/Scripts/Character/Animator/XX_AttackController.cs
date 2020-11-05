using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Demos;
using UnityEngine;

public class XX_AttackController : MonoBehaviour
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
    private string isAttack = "IsAttack";

    #region StateInfo

    private List<XX_AnimationStateInfo> _stateInfos = new List<XX_AnimationStateInfo>();

    private XX_AnimationStateInfo _curStateInfo;
    private XX_AnimationStateInfo _normalNextStateInfo;

    private float _curStateTime;

    private XX_AnimationStateInfo normalStateInfo;
    private XX_AnimationStateInfo normal1StateInfo;
    private XX_AnimationStateInfo normal2StateInfo;

    /// <summary>
    /// 是否正在攻击中
    /// </summary>
    public bool IsAttacking;

    private bool? _isAction = null;
    #endregion


    #region 层级权值参数

    private float tempUpLayerTime;
    private float upLayerTime = 0.3f;
    private float lastUpLayerWeight;
    private float endUpLayerWeight;
    private float tempDownLayerTime;
    private float downLayerTime = 0.3f;
    private float lastDownLayerWeight;
    private float endDownLayerWeight;

    #endregion

    #region 攻击树 属性参数

    private float tempHorParameterTime;
    private float horParameterTime = 0.3f;
    private float lastHorParameter;
    private float endHorParameter;
    
    private float tempVerParameterTime;
    private float verParameterTime = 0.3f;
    private float lastVerParameter;
    private float endVerParameter;
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

        //up layer fade
        if (tempUpLayerTime > 0)
        {
            tempUpLayerTime -= Time.deltaTime;
            var t = 1 - tempUpLayerTime / upLayerTime;
            animator.SetLayerWeight(upBodyLayer, Mathf.Lerp(lastUpLayerWeight, endUpLayerWeight, t));
        }
        
        //down layer fade
        if (tempDownLayerTime > 0)
        {
            tempDownLayerTime -= Time.deltaTime;
            var t = 1 - tempDownLayerTime / downLayerTime;
            animator.SetLayerWeight(downBodyLayer, Mathf.Lerp(lastDownLayerWeight, endDownLayerWeight, t));
        }

        //attackHor fade
        if (tempHorParameterTime > 0)
        {
            tempHorParameterTime -= Time.deltaTime;
            var t = 1 - tempHorParameterTime / horParameterTime;
            animator.SetFloat(attackHor, Mathf.Lerp(lastHorParameter, endHorParameter, t));
        }
        
        //attackVer fade
        if (tempVerParameterTime > 0)
        {
            tempVerParameterTime -= Time.deltaTime;
            var t = 1 - tempVerParameterTime / verParameterTime;
            animator.SetFloat(attackVer, Mathf.Lerp(lastVerParameter, endVerParameter, t));
        }
        #endregion
        
        OnAttack();

        if (_curStateInfo != null)
        {
            _curStateTime += Time.deltaTime;
            if (_curStateTime > _curStateInfo.AnimationLength)
            {
                if (_normalNextStateInfo==_curStateInfo||_normalNextStateInfo==null)
                {
                    OnExitState(_curStateInfo);
                }
            }
        }

    }

    /// <summary>
    /// 攻击逻辑
    /// </summary>
    void OnAttack()
    {
        bool canAttack = characterController.animState.attackIndex != 0;
        canAttack &= _curStateInfo == null ||(_curStateInfo != null && _curStateTime >= _curStateInfo.AnimationLength);

        canAttack |= (_curStateInfo != null && _normalNextStateInfo != null && _curStateTime >= _curStateInfo.AnimationLength);

        if (IsAttacking)
        {
            if (characterController.animState.attackIndex==1 && IsCurAttackNormal())
            {
                _normalNextStateInfo = _curStateInfo == normalStateInfo ? normal1StateInfo :
                    _curStateInfo == normal1StateInfo ? normal2StateInfo :
                    _curStateInfo == normal2StateInfo ? normalStateInfo : null;
            }
            //判断是否在其他不同行为 例如 走，跳，蹲下
            bool isAction = !characterController.animState.onGround;
            isAction |= characterController.animState.run;
            isAction |= characterController.animState.walk;
            isAction |= characterController.animState.crouch;
            if (isAction!=_isAction)
            {
                _isAction = isAction;
                SetDownBodyLayerWeight(isAction ? 0 : 1.0f);
            }
        }
        if (!canAttack)
        {
            return;
        }

        //TODO 获取合适得状态
        //XX_AnimationStateInfo stateInfo;
        SetCurStateInfo(_normalNextStateInfo ?? normalStateInfo);
        SetAttackHorParameter(1);
        SetUpBodyLayerWeight(1);

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

    /// <summary>
    /// 设置AttackHor 参数
    /// </summary>
    void SetAttackHorParameter(float value)
    {
        endHorParameter = value;
        tempHorParameterTime = horParameterTime;
        lastHorParameter = animator.GetFloat(attackHor);
    }

    /// <summary>
    /// 设置AttackVer 参数
    /// </summary>
    void SetAttackVerParameter(float value)
    {
        endVerParameter = value;
        tempVerParameterTime = verParameterTime;
        lastVerParameter = animator.GetFloat(attackVer);
    }
    
    /// <summary>
    /// 设置当前状态
    /// </summary>
    /// <param name="stateInfo"></param>
    void SetCurStateInfo(XX_AnimationStateInfo stateInfo)
    {
        _curStateInfo = stateInfo;
        _curStateTime = 0;
        IsAttacking = true;
        animator.SetBool(isAttack,IsAttacking);
    }
    
    /// <summary>
    /// 当状态离开
    /// </summary>
    /// <param name="stateInfo"></param>
    void OnExitState(XX_AnimationStateInfo stateInfo)
    {
        SetUpBodyLayerWeight(0);
        SetDownBodyLayerWeight(0);
        SetAttackHorParameter(0);
        SetAttackVerParameter(0);

        _curStateTime = 0;
        _curStateInfo = null;
        IsAttacking = false;
        animator.SetBool(isAttack,IsAttacking);
        _isAction = null;
        _normalNextStateInfo = null;
    }

    /// <summary>
    /// 判断是否是普通攻击
    /// </summary>
    /// <returns></returns>
    public bool IsCurAttackNormal()
    {
        return _curStateInfo == normalStateInfo ||
               _curStateInfo == normal1StateInfo ||
               _curStateInfo == normal2StateInfo;
    }

    /// <summary>
    /// 初始化攻击动画
    /// </summary>
    void InitStateInfos()
    {
        normalStateInfo = new XX_AnimationStateInfo(animator, "UpBody.OrientalSword_COMBOATTACK01", 0.5f, true, 0f, 0.5f,0.5f);
        normal1StateInfo = new XX_AnimationStateInfo(animator, "UpBody.OrientalSword_COMBOATTACK01", 0.6f, true, 0.5f, 0.6f,0.6f);
        normal2StateInfo = new XX_AnimationStateInfo(animator, "UpBody.OrientalSword_COMBOATTACK01", 0.9f, true, 1.1f, 0.9f,0.9f);
        _stateInfos.Add(normalStateInfo);
        _stateInfos.Add(normal1StateInfo);
        _stateInfos.Add(normal2StateInfo);
    }

   

}
