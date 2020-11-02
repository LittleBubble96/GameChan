using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public delegate bool OnCondition();

public class ConversationNodeTranslation
{
    public int FromIndex;

    public int ToIndex;

    public OnCondition Condition;


    public ConversationNodeTranslation()
    {
        
    }


    public ConversationNodeTranslation(int fromIndex ,int toIndex)
    {
        this.FromIndex = fromIndex;
        this.ToIndex = toIndex;
    }

    public bool OnCondition()
    {
        if (Condition!=null)
        {
            return Condition();
        }
        return true;
    }


#if UNITY_EDITOR
    public NodeRect LineRect;

    private ConversationNodeController _controller;
    private ConversationNode _thisNode;

    /// <summary>
    /// 绘制节点内得元素
    /// </summary>
    /// <param name="thisNode"></param>
    /// <param name="controller"></param>
    public void DrawArea(ConversationNode thisNode, ConversationNodeController controller)
    {
        _controller = controller;
        _thisNode = thisNode;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("remove"))
        {
            _controller?.RemoveTranslation(this);
        }

        GUILayout.Label("TO " + (ToIndex == -1 ? "" : _controller?[ToIndex].Title));

        if (GUILayout.Button("Select"))
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < controller.WindowNodes.Count; i++)
            {
                if (controller[i] != _thisNode)
                {
                    menu.AddItem(new GUIContent(controller[i].Title), false, OnOptionNode, i);
                }
            }

            menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();
    }

    //添加传递得节点
    private void OnOptionNode(object data)
    {
        int node = (int) data;
        this.ToIndex = node;
    }

    public struct NodeRect
    {
        public Vector2 WidthVector;
        public Vector2 HeightVector;

        public Vector2 RectPosition;

        public NodeRect(Vector2 widthVector, Vector2 heightVector, Vector2 rectPosition)
        {
            WidthVector = widthVector;
            HeightVector = heightVector;
            RectPosition = rectPosition;
        }

        public bool Contain(Vector2 mousePos)
        {
            Vector2 dir = mousePos - RectPosition;
            Vector2 normalWidth = Vector3.Project(dir, WidthVector);
            float width = normalWidth.magnitude / WidthVector.magnitude;
            Vector2 normalHeight = Vector3.Project(dir, HeightVector);
            float height = normalHeight.magnitude / HeightVector.magnitude;
            return height <= 1 & width <= 1;
        }
    }
#endif
}
