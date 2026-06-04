using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Graph
{
    //声明一个字典，该字典的类型为<八叉树节点, 图中的节点>
    public readonly Dictionary<OctreeNode, Node> nodes = new Dictionary<OctreeNode, Node>();
    //为所有的边设置一个哈希集合，确保没有重复的边
    public readonly HashSet<Edge> edges = new HashSet<Edge>();


    //声明一个列表，用于存储最终路径
    private List<Node> pathList = new List<Node>();

    /// <summary>
    /// 获取最终路径的长度
    /// </summary>
    /// <returns></returns>
    public int GetPathLength() => pathList.Count;

    /// <summary>
    /// 获取最终路径中的节点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public OctreeNode GetPathNode(int index){
        if(pathList ==null) return null;

        if(index < 0 || index >= pathList.Count){
            Debug.LogError($"Index oout of bounds. Path length: {pathList.Count}, Index: {index}");
            return null;
        }
        return pathList[index].octreeNode;
    }

    //声明一个最大迭代次数，防止堆栈溢出
    private int maxIterations = 1000;

    /// <summary>
    /// A*算法的实现
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    public bool AStar(OctreeNode startNode, OctreeNode endNode)
    {
        pathList.Clear();
        // 重置所有节点，防止上一次寻路的结果干扰本次寻路
        foreach (var node in nodes.Values)
        {
            node.g = float.MaxValue;
            node.h = 0;
            node.f = float.MaxValue;
            node.from = null;
        }

        Node start = FindNode(startNode);
        Node end = FindNode(endNode);
        if (start == null || end == null){
            Debug.LogError("Start or End node not found in the graph");
            return false;
        }
        //声明一个有序集合，作为开放列表，用于存储所有未访问的节点，按照f值排序
        SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());
        HashSet<Node> closedSet = new HashSet<Node>();
        //声明一个迭代次数，用于记录当前的迭代次数，防止超过最大迭代次数
        int iterationCount = 0;
        
        start.g = 0;
        start.h = Heuristic(start, end);
        start.f = start.g + start.h;
        openSet.Add(start);

        while (openSet.Count > 0){
            //如果当前次数超过最大迭代次数，说明算法进入无限循环，直接返回false
            if(++iterationCount > maxIterations){
                Debug.LogError("A* algorithm iteration count exceeded the maximum allowed iterations");
                return false;
            }
            //从开放列表中选择f值最小的节点作为当前节点
            Node current = openSet.First();
            //将当前节点从开放列表中移除
            //因为当前节点已经被访问过了，所以不需要再访问了
            openSet.Remove(current);
            
            //如果当前节点是目标节点，说明路径找到，直接返回true
            if(current.Equals(end)){
                ReconstructPath(current);
                return true;
            }

            //将当前节点添加到已访问列表（关闭列表）中
            closedSet.Add(current);

            //遍历当前节点的所有相邻节点
            foreach(Edge edge in current.edges){
                Node neighbor = Equals(edge.a, current) ? edge.b : edge.a;
                if(closedSet.Contains(neighbor)) continue;

                //计算当前节点到相邻节点的g值
                //g值 = 当前节点的g值 + 从当前节点到相邻节点的边的权重
                float tentative_gScore = current.g + Heuristic(current, neighbor);

                if(tentative_gScore < neighbor.g){
                    // 如果已经在 openSet 中，更新其属性前必须先移除，
                    // 否则 SortedSet 的排序不会自动更新，导致集合逻辑混乱。
                    if (openSet.Contains(neighbor)) {
                        openSet.Remove(neighbor);
                    }
                    
                    neighbor.g = tentative_gScore;
                    neighbor.h = Heuristic(neighbor, end);
                    neighbor.f = neighbor.g + neighbor.h;
                    neighbor.from = current;
                    openSet.Add(neighbor);
                }
            }
        }

        Debug.LogError("No path found from start to end");
        return false;
    }

    /// <summary>
    /// 从当前节点开始，根据from指针回溯，构建最终路径
    /// </summary>
    /// <param name="current"></param>
    private void ReconstructPath(Node current)
    {
        pathList.Clear();
        while (current != null)
        {
            pathList.Add(current);
            current = current.from;
        }
        pathList.Reverse();
    }

    

    /// <summary>
    /// 计算两个节点之间的启发式距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float Heuristic(Node a, Node b)
    {
        //return Vector3.Distance(a.octreeNode.bounds.center, b.octreeNode.bounds.center);
        //计算两个节点之间的启发式距离，使用欧氏距离的平方
        return (a.octreeNode.bounds.center - b.octreeNode.bounds.center).sqrMagnitude;
    }

    /// <summary>
    /// 节点比较器，用于比较两个节点的f值
    /// </summary>
    public class NodeComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            if(x == null || y == null) return 0;
            int compar =  x.f.CompareTo(y.f);
            if(compar == 0)
            {
                compar = x.id.CompareTo(y.id);
            }
            return compar;
        }
    }

    /// <summary>
    /// 创建节点并添加到图中的函数，同时关联一个八叉树节点
    /// </summary>
    /// <param name="octreeNode"></param>
    public void AddNode(OctreeNode octreeNode)
    {
        if (!nodes.ContainsKey(octreeNode))
        {
            nodes.Add(octreeNode, new Node(octreeNode));
        }
    }

    /// <summary>
    /// 创建边并添加到图中
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void AddEdge(OctreeNode a, OctreeNode b)
    {
        Node nodeA = FindNode(a);
        Node nodeB = FindNode(b);

        if (nodeA == null || nodeB == null) return;

        var edge = new Edge(nodeA, nodeB);
        //如果这条边能够添加到边的哈希集合中，则说明这条边是一个新的边
        if (edges.Add(edge))
        {
            //将这条边添加到两个端点的边列表中
            nodeA.edges.Add(edge);
            nodeB.edges.Add(edge);
        }
    }

    /// <summary>
    /// 绘制图
    /// </summary>
    public void DrawGraph()
    {
        Gizmos.color = Color.red;
        // foreach (Edge edge in edges)
        // {
        //     Gizmos.DrawLine(edge.a.octreeNode.bounds.center, edge.b.octreeNode.bounds.center);
        // }
        foreach (var node in nodes.Values)
        {
            Gizmos.DrawWireSphere(node.octreeNode.bounds.center, 0.2f);
        }
    }

    /// <summary>
    /// 查找图节点的方法，传入一个关联该图节点的八叉树节点
    /// </summary>
    /// <param name="octreeNode"></param>
    /// <returns></returns>
    private Node FindNode(OctreeNode octreeNode)
    {
        nodes.TryGetValue(octreeNode, out Node node);
        return node;
    }
}
