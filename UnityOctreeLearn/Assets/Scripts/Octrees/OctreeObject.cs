using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 八叉树中的物体
/// </summary>
public class OctreeObject
{
    //八叉树中的物体的包围盒
    //帮助我们判断某物是否完全存在与节点内部，或者与多个节点相交
    private Bounds bounds;

    /// <summary>
    /// 构造函数，我们以一个物体为例
    /// </summary>
    /// <param name="obj"></param>
    public OctreeObject(GameObject obj)
    {
        bounds = obj.GetComponent<Collider>().bounds;
    }

    /// <summary>
    /// 判断该游戏对象的包围盒是否与其他的包围盒有相交的部分
    /// </summary>
    /// <param name="boundsToCheck"></param>
    /// <returns></returns>
    public bool Intersects(Bounds boundsToCheck)
    {
        return bounds.Intersects(boundsToCheck);
    }
}
