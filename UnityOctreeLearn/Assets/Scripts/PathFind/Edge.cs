using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 图中的边
/// </summary>
public class Edge
{
    //一条边的两个端点
    public readonly Node a, b;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public Edge(Node a, Node b)
    {
        this.a = a;
        this.b = b;
    }

    /// <summary>
    /// 重写Equals方法，用于判断是否是同一条边
    /// </summary>
    /// <param name="obj">要判断的另一个对象</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        return obj is Edge other && ((a == other.a && b == other.b) || (a == other.b && b == other.a));
    }

    /// <summary>
    /// 重写GetHashCode方法，获取一条边的哈希值，确保每条边的哈希值唯一
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return a.GetHashCode() ^ b.GetHashCode();
    }

    //
}
