using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 八叉树节点
/// </summary>
public class OctreeNode
{
    //完全或部分存在与该节点内部的八叉树对象的列表
    //可以使用哈希表来优化，查找速度更快
    public List<OctreeObject> objects = new List<OctreeObject>();

    private static int nextId;
    //每个节点自己的id
    public readonly int id;

    //节点的包围盒
    public Bounds bounds;
    //所有子节点的包围盒
    Bounds[] childBounds = new Bounds[8];
    //所有子节点数组
    public OctreeNode[] children;

    //属性：是否为叶子节点
    public bool IsLeaf => children == null;

    //节点包围盒的最小边长
    private float minNodeSize;

    /// <summary>
    /// 节点构造函数
    /// </summary>
    /// <param name="bounds">节点的包围盒</param>
    /// <param name="minNodeSize">节点包围盒的最小边长</param>
    public OctreeNode(Bounds bounds, float minNodeSize)
    {
        id = nextId++;

        this.bounds = bounds;
        this.minNodeSize = minNodeSize;

        //计算子节点的包围盒大小
        Vector3 newSize = bounds.size * 0.5f;//将0.5改变为0.6，实现松散二叉树————出现问题：运行后编辑器卡住没有出现结果
        //计算每个子节点包围盒的中心center
        Vector3 centerOffset = bounds.size * 0.25f;
        Vector3 parentCenter = bounds.center;

        for (int i = 0; i < 8; i++)
        {
            Vector3 childCenter = parentCenter;
            //通过位运算模拟出八个中心点
            childCenter.x += centerOffset.x * ((i & 1) == 0 ? -1 : 1);
            childCenter.y += centerOffset.y * ((i & 2) == 0 ? -1 : 1);
            childCenter.z += centerOffset.z * ((i & 4) == 0 ? -1 : 1);
            //创建子节点的包围盒
            childBounds[i] = new Bounds(childCenter, newSize);
        }
    }

    /// <summary>
    /// 接收游戏对象的分割函数，但是我们要接收的参数实际上是八叉树的对象OctreeObject
    /// 使用该游戏对象创建一个OctreeObject
    /// </summary>
    /// <param name="obj"></param>
    public void Divide(GameObject obj) => Divide(new OctreeObject(obj));

    /// <summary>
    /// 接收八叉树的对象OctreeObject的分割函数
    /// 分割当前节点的包围盒，然后添加游戏对象到子节点中（递归）
    /// </summary>
    /// <param name="octreeObject"></param>
    private void Divide(OctreeObject octreeObject)
    {
        //递归函数的结束条件：当前节点的边长小于或等于我们设置的最小尺寸
        if (bounds.size.x <= minNodeSize)
        {
            AddObject(octreeObject);
            return;
        }

        if(children == null)
        {
            children = new OctreeNode[8];
        }

        bool intersectedChild = false;

        for (int i = 0; i < 8; i++)
        {
            children[i] ??= new OctreeNode(childBounds[i], minNodeSize);

            //如果子节点的包围盒与游戏对象碰撞器的包围盒相交，那么我们就需要分割出该子节点，
            //并且不断地判断子节点的子节点的包围盒是否与游戏对象碰撞器的包围盒相交，继续分割......，直到包围盒最小或不相交
            if (octreeObject.Intersects(childBounds[i]))
            {
                children[i].Divide(octreeObject);
                intersectedChild = true;
            }
        }

        //如果检测了所有的子节点的包围盒都与该对象没有相交，则说明该对象不属于任何子节点，我们会把它添加到这个节点
        if (!intersectedChild)
        {
            AddObject(octreeObject);
        }
    }

    /// <summary>
    /// 将处于八叉树中的对象添加到List列表中
    /// </summary>
    /// <param name="octreeObject"></param>
    private void AddObject(OctreeObject octreeObject) => objects.Add(octreeObject);

    /// <summary>
    /// 为每个子节点绘制边界框
    /// </summary>
    public void DrawNode()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        //让与该对象相交的子节点包围盒看起来更加明显，我们将其颜色设置为红色
        foreach (OctreeObject obj in objects)
        {
            if (obj.Intersects(bounds))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        //只要该节点有子节点
        if(children != null)
        {
            foreach (OctreeNode child in children)
            {
                if (child != null) child.DrawNode();
            }
        }
    }
}
