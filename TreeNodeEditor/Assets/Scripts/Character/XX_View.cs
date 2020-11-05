
using UnityEngine;

public class XX_View
{
    /// <summary>
    /// 初始半径
    /// </summary>
    public float StartRadiu;

    /// <summary>
    /// 远端半径
    /// </summary>
    public float EndRadiu;

    /// <summary>
    /// 距离
    /// </summary>
    public float Duration;

    /// <summary>
    /// 观看者
    /// </summary>
    public Transform ViewTransfram;

    /// <summary>
    /// 是否再视野中
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsInsideView(Transform target)
    {
        bool value = true;
        value &= Vector3.Distance(target.position, ViewTransfram.position) < Duration;
        if (!value)
            return false;
        //value &= Vector3.Project()
        return value;
    }
}
