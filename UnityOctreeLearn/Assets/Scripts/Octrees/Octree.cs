using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 八叉树
/// </summary>
public class Octree
{
    //根节点
    public OctreeNode root;
    //八叉树的总包围盒（AABB）
    //帮助我们跟踪树内的内容，以及每个节点内部会有什么
    public Bounds bounds;

    public Graph graph;
    //存储空叶子节点的列表
    private List<OctreeNode> emptyLeaves = new List<OctreeNode>();

    /// <summary>
    /// 八叉树构造函数
    /// </summary>
    /// <param name="worldObjects">世界空间中的所有游戏对象数组</param>
    /// <param name="minNodeSize">节点包围盒的最小边长</param>
    public Octree(GameObject[] worldObjects, float minNodeSize, Graph graph)
    {
        this.graph = graph;

        //计算总包围盒的大小
        CalculateBounds(worldObjects);
        //创建整颗八叉树
        CreateTree(worldObjects, minNodeSize);

        //现在我们知道了哪些节点是飞船不能通过的“禁飞区”，我们还需要知道树中哪些叶子节点是空的（飞船可以通过的）
        GetEmptyLeaves(root);

        //但是此时不是一个完整的图，只有子节点之间的边，而没有节点与节点之间的边，没有找出所有可能出现的边
        //解决方法：获取边
        GetEdges();
        Debug.Log(graph.edges.Count);
    }


    /// <summary>
    /// 传入一个我们要到达的目标位置的坐标，返回最近的八叉树节点
    /// </summary>
    /// <param name="position"></param>s
    /// <returns></returns>
    public OctreeNode FindClosestNode(Vector3 position){
        return FindClosestNode(root, position);
    }

    /// <summary>
    /// 传入一个我们要到达的目标位置的坐标，返回最近的八叉树节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="position">目标位置坐标</param>
    /// <returns></returns>
    public OctreeNode FindClosestNode(OctreeNode node, Vector3 position){
        OctreeNode found = null;
        //遍历当前节点的所有子节点
        for(int i = 0; i < node.children.Length; i++){
            //如果目标位置在当前子节点的包围盒内，则返回当前子节点
            if(node.children[i].bounds.Contains(position)){
                if(node.children[i].IsLeaf)
                {
                    //如果当前子节点是叶子节点，则直接返回当前子节点
                    found = node.children[i];
                    break;
                }
                else
                {
                    //如果当前子节点不是叶子节点，则继续递归判断其子节点
                    found = FindClosestNode(node.children[i], position);
                    if(found != null)
                    {
                        break;
                    }
                }
            }
        }
        return found;
    }


    /// <summary>
    /// 获取所有边
    /// </summary>
    private void GetEdges()
    {
        // 使用空间哈希或简单的邻居搜索优化 $O(n^2)$。
        // 考虑到目前的结构，我们先通过减少不必要的判定来缓解卡顿。
        for (int i = 0; i < emptyLeaves.Count; i++)
        {
            OctreeNode leaf = emptyLeaves[i];
            Bounds checkBounds = leaf.bounds;
            checkBounds.Expand(0.1f);

            // 只与后续的节点进行判定，减少一半的计算量，并避免 AddEdge 重复处理
            for (int j = i + 1; j < emptyLeaves.Count; j++)
            {
                OctreeNode otherLeaf = emptyLeaves[j];
                if (checkBounds.Intersects(otherLeaf.bounds))
                {
                    graph.AddEdge(leaf, otherLeaf);
                }
            }
        }
    }

    /// <summary>
    /// 获取八叉树中哪些叶子节点是空的（递归），然后在图中创建这些空叶子节点关联的图节点，以及这些图节点之间的边
    /// </summary>
    /// <param name="node"></param>
    private void GetEmptyLeaves(OctreeNode node)
    {
        //递归结束的两种情况
        //1.如果该八叉树节点是叶子节点并且该节点中没有包含游戏对象
        if(node.IsLeaf && node.objects.Count == 0)
        {
            //添加到空叶子节点列表
            emptyLeaves.Add(node);

            //在图中为其创建一个关联了该叶子节点的图节点
            graph.AddNode(node);

            return;
        }

        //2.如果该八叉树节点是叶子节点，但该节点包含了游戏对象在里面，则直接返回
        if (node.children == null) return;

        //如果该八叉树节点不是叶子节点，那么我们还要来继续判断其子节点
        foreach (OctreeNode child in node.children)
        {
            GetEmptyLeaves(child);
        }

        //使用双循环在图中创建每两个子节点所连成的边
        for (int i = 0; i < node.children.Length; i++)
        {
            for (int j = i + 1; j < node.children.Length; j++)
            {
                graph.AddEdge(node.children[i], node.children[j]);
            }
        }
    }


    /// <summary>
    /// 八叉树的总包围盒的大小，要包含世界空间中的所有游戏对象
    /// </summary>
    /// <param name="worldObjects">这些游戏对象上都挂载了Collider</param>
    private void CalculateBounds(GameObject[] worldObjects)
    {
        foreach(var obj in worldObjects)
        {
            bounds.Encapsulate(obj.GetComponent<Collider>().bounds);
        }

        //把这个包围盒变成一个正方体
        Vector3 size = Vector3.one * Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.6f;  //为什么要从0.5改变为0.6？————使所有节点都会稍微重叠，我们能够准确地判断哪些节点彼此相交
        bounds.SetMinMax(bounds.center - size, bounds.center + size);
    }

    private void CreateTree(GameObject[] worldObjects, float minNodeSize)
    {
        //创建根节点
        root = new OctreeNode(bounds, minNodeSize);

        //遍历所有的游戏对象，因为我们不想要细分所有的子节点，只会细分包含了游戏对象的子节点
        foreach (var obj in worldObjects)
        {
            root.Divide(obj);
        }
    }
}
