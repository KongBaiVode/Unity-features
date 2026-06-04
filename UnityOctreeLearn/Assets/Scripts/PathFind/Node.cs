using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 图中的节点（由八叉树中的节点来表示）
/// </summary>
public class Node
{
    private static int nextId;
    //每一个节点都有自己的id
    public readonly int id;


    //因为我们要使用Astar算法，需要计算寻路消耗：f = g + h
    //所以进行以下声明：
    //f-寻路消耗，g-起点到该节点的实际消耗，h-该节点到终点的预估消耗
    public float f, g, h;
    //该节点的上一个节点（记录从哪个节点到达该节点）
    public Node from;




    //每一个节点都应该知道自己有多少条边，我们将这些边存储在一个列表中
    public List<Edge> edges = new List<Edge>();

    //每一个节点也应该知道自己与哪个八叉树节点相关联
    public OctreeNode octreeNode;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="octreeNode">与自己关联的八叉树节点</param>
    public Node(OctreeNode octreeNode)
    {
        this.id = nextId++;
        this.octreeNode = octreeNode;
    }

    /// <summary>
    /// 重写Equals方法，通过检查id来判断obj和自己是同一个节点
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) => obj is Node other && id == other.id;

    /// <summary>
    /// 重写GetHashCode方法，使用一个Node的id的哈希值来表示该Node的哈希值
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => id.GetHashCode();
    
}
