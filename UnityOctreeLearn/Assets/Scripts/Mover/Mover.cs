using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Mover : MonoBehaviour
{
    //声明一个速度，用于控制移动的速度
    private float speed = 20f;
    //声明一个精度，用于控制移动的精度
    private float accuracy = 1f;
    //声明一个转弯速度，用于控制转弯的速度
    private float turnSpeed = 10f;

    //声明一个当前的图的waypoint索引，用于记录当前的waypoint
    private int currentWaypoint;
    //声明一个目标位置，用于记录当前的目标位置
    private Vector3 destination;
    //声明一个当前的节点，用于记录当前的节点
    private OctreeNode currentNode;

    public OctreeGenerator octreeGenerator;
    //声明一个图，用于存储八叉树中的节点和边
    private Graph graph;

    void Start()
    {
        //获取八叉树的图
        graph = octreeGenerator.waypoints;
        //获取飞船当前位置最近的节点
        currentNode = GetClosestNode(transform.position);
        GetRandomDestination();
    }

    void Update()
    {
        if(graph == null) return;

        //检查当前移动任务是否已经结束（或从未开始，并触发新目标的寻找。
        if(graph.GetPathLength() == 0 || currentWaypoint >= graph.GetPathLength()){
            GetRandomDestination();
            return;
        }

        //如果当前的位置与目标位置的距离小于精度，说明已经到达目标位置
        if(Vector3.Distance(currentNode.bounds.center, transform.position) < accuracy){
            currentWaypoint++;
            Debug.Log($"Arrived at waypoint {currentWaypoint}");
        }

        //如果当前的waypoint索引小于路径的长度，说明还没有到达目标位置
        //则更新目标位置为下一个waypoint的位置
        if(currentWaypoint < graph.GetPathLength()){
            //更新当前的节点为下一个waypoint的节点
            currentNode = graph.GetPathNode(currentWaypoint);
            //更新目标位置为下一个waypoint的位置
            destination = currentNode.bounds.center;

            //计算当前的方向
            Vector3 direction = (destination - transform.position).normalized;
            //根据方向旋转模型
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            //根据方向移动模型
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else{
            //如果当前的waypoint索引大于等于路径的长度，说明已经到达目标位置
            //则随机选择一个新的目标节点
            GetRandomDestination();
        }
    }

    /// <summary>
    /// 获取最近的节点
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private OctreeNode GetClosestNode(Vector3 position)
    {
        // OctreeNode closestNode = null;
        // //初始化最近的距离为无穷大
        // //因为最近的距离必须小于等于无穷大，所以最近的距离必须大于等于0
        // float closestDistanceSqr = Mathf.Infinity;
        // //遍历图中的所有节点
        // foreach(var nodePair in graph.nodes){
        //     //获取当前的节点
        //     //计算当前的节点与目标位置的距离的平方
        //     OctreeNode node = nodePair.Key;
        //     float distanceSqr = (node.bounds.center - position).sqrMagnitude;
        //     //如果当前的节点与目标位置的距离的平方小于最近的距离的平方，说明当前的节点是最近的节点
        //     if(distanceSqr < closestDistanceSqr){
        //         //更新最近的距离为当前的节点与目标位置的距离的平方
        //         //更新最近的节点为当前的节点
        //         closestDistanceSqr = distanceSqr;
        //         closestNode = node;
        //     }
        // }
        // return closestNode;

        //使用八叉树的FindClosestNode方法，获取最近的节点
        return octreeGenerator.ot.FindClosestNode(position);
    }

    /// <summary>
    /// 获取随机的目标节点
    /// </summary>
    private void GetRandomDestination()
    {
        OctreeNode destinationNode;
        do{
            //随机选择一个节点作为目标节点
            destinationNode = graph.nodes.ElementAt(Random.Range(0, graph.nodes.Count)).Key;
        }while(!graph.AStar(currentNode, destinationNode));
        //设置当前的waypoint索引为0
        currentWaypoint = 0;

    }

    void OnDrawGizmos(){
        //如果图为空，或者路径为空，说明没有waypoint，直接返回
        if(graph == null || graph.GetPathLength() == 0) return;

        Gizmos.color = Color.red;
        //绘制第一个waypoint的位置
        Gizmos.DrawWireSphere(graph.GetPathNode(0).bounds.center, 5f);
        Gizmos.color = Color.blue;
        //绘制最后一个waypoint的位置
        Gizmos.DrawWireSphere(graph.GetPathNode(graph.GetPathLength() - 1).bounds.center, 5f);

        Gizmos.color = Color.green;
        for(int i = 0; i < graph.GetPathLength(); i++){
            Gizmos.DrawWireSphere(graph.GetPathNode(i).bounds.center, 4.5f);
            if(i < graph.GetPathLength() - 1){
                Vector3 start = graph.GetPathNode(i).bounds.center;
                Vector3 end = graph.GetPathNode(i + 1).bounds.center;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}