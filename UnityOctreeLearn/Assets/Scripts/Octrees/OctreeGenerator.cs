using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 八叉树生成器
/// </summary>
public class OctreeGenerator : MonoBehaviour
{
    //世界空间中的游戏对象数组
    public GameObject[] objects;
    //每个节点包围盒的最小边长
    public float minNodeSize = 1f;
    //声明一个八叉树
    public Octree ot;

    //声明一个图，用于存储八叉树中的节点和边
    public readonly Graph waypoints = new Graph();

    private void Awake()
    {
        //创建一个八叉树对象
        ot = new Octree(objects, minNodeSize, waypoints);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(ot.bounds.center, ot.bounds.size);

        //绘制子节点的边界框
        ot.root.DrawNode();
        //绘制图
        ot.graph.DrawGraph();
    }
}
