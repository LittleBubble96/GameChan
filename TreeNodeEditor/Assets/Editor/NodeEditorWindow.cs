using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public enum ENodeType
{
    ConversationNode,
    DeleteNode,
    MakeTranslation,
}

public class NodeEditorWindow : EditorWindow
{
    /// <summary>
    /// 控制器
    /// </summary>
    private ConversationNodeController Controller{
        get => _controller;
        set => _controller = value;
    }

    private ConversationNodeController _controller;
    

    /// <summary>
    /// 储存得文件
    /// </summary>
    private TextAsset _textAsset;

    private TextAsset TextAsset
    {
        get => _textAsset;
        set
        {
            if (value!=_textAsset)
            {
                _textAsset = value;
                Debug.Log(_textAsset.text);
                Controller = JsonConvert.DeserializeObject<ConversationNodeController>(_textAsset.text);
                if (Controller==null)
                {
                    Controller=new ConversationNodeController();
                }
            }
        }
    }

    /// <summary>
    /// 鼠标位置
    /// </summary>
    private Vector2 _mousePotion;

    /// <summary>
    /// 是否点击到窗体
    /// </summary>
    private bool _isClickWindow = false;

    /// <summary>
    /// 是否正在画线
    /// </summary>
    private bool _isDrawLining;

    /// <summary>
    /// 上一个窗体得下标
    /// </summary>
    private int _lastClickWindowIndex = -1;

    /// <summary>
    /// 是否点击到线
    /// </summary>
    private bool _isClickLine;

    /// <summary>
    /// 点击到得线
    /// </summary>
    private ConversationNodeTranslation _clickLine= null;

    /// <summary>
    /// 当前窗口
    /// </summary>
    private NodeEditorWindow _window;

    //宽度
    float width = 8;
    
    //鼠标监听
    private Event _e;


    private Vector3 scrollPos;
    Vector3 scrollLastPos = Vector2.zero;
    private static NodeEditorWindow window;
    
    [MenuItem("Tool/Open Node Window")]
    static void OnOpenWindow()
    {
        window = EditorWindow.GetWindow<NodeEditorWindow>(true);
        
        window.position = new Rect(300, 300, 500, 500);  // 窗口的坐标
    }

    /// <summary>
    /// 储存文件
    /// </summary>
    private void SaveData()
    {
        string path= Application.dataPath + "/";
        File.WriteAllText(path + TextAsset.name + ".txt",JsonConvert.SerializeObject(Controller) );
        AssetDatabase.Refresh();
    }
    private void OnGUI()            
    {
        //绘制总得窗口
        BeginWindows();
        EditorGUILayout.BeginHorizontal();
        //选择文件
        TextAsset =  EditorGUILayout.ObjectField("添加流程文件",TextAsset,typeof(TextAsset),true) as TextAsset;
       

        if (TextAsset == null || Controller == null)
        {
            EditorGUILayout.EndHorizontal();
            EndWindows();
            return;
        }
        
        _e=Event.current;
        //按下右键
        if (_e.button==1)
        {
            _mousePotion = _e.mousePosition;
            CreateMenu(_e);
        }
        else if(_e.button==0&& _e.isMouse)
        {
            RightMouseSelect();
        }
        else
        {
            _isClickLine = false;
        }

        //正在画线
        if (_isDrawLining && _lastClickWindowIndex!=-1)
        {
            _mousePotion = _e.mousePosition;
            DrawBezier(Controller.WindowNodes[_lastClickWindowIndex]._editorRect, new Rect(_mousePotion.x,_mousePotion.y,0,0),out Vector2 startPos, out Vector2 endPos);
            if (_e.button == 0 && IsFocusWindowNode(_e,out int index))
            {
                if (index!=_lastClickWindowIndex)
                {
                    Controller.AddTranslation(Controller.WindowNodes[_lastClickWindowIndex],Controller.WindowNodes[index]);
                    
                    _lastClickWindowIndex = -1;
                    _isDrawLining = false;
                }
            }
        }
     
        
     
        if (GUILayout.Button("save"))
        {
            SaveData();
        }
        EditorGUILayout.EndHorizontal();
        
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, window.position.width, window.position.height),
            scrollPos, new Rect(0, 0, 3000, 3000));
        //画窗口
        for (int i = 0; i < Controller.WindowNodes.Count; i++)
        {
            string winTitle = "对话节点" + i;
            //计算滑动条偏移
            Controller.WindowNodes[i].Title = winTitle;
            Vector3 offset = scrollLastPos - scrollPos;
            Rect nodeRect = Controller.WindowNodes[i]._editorRect;
            nodeRect = new Rect(nodeRect.x+offset.x, nodeRect.y+offset.y, nodeRect.width, nodeRect.height);
            //绘制节点和线段
            Controller.WindowNodes[i]._editorRect = GUI.Window(i, nodeRect, OnNodeCallBack, winTitle);

            foreach (var t in Controller.WindowNodes[i].Translations)
            {
                if (t.ToIndex!=-1)
                {
                    DrawBezier(t);
                }
            }
        }

        scrollLastPos = scrollPos;
        GUI.EndScrollView();  //结束 ScrollView 窗口

        EndWindows();

    }

    
    private void OnNodeCallBack(int id)
    {
        //Debug.Log("显示Node"+id);
        Controller.WindowNodes[id].DrawWindow(Controller);

        GUI.DragWindow();
    }


    /// <summary>
    /// 创建菜单
    /// </summary>
    /// <param name="e"></param>
    private void CreateMenu(Event e)
    {
        FocusWindowNode();
        if (_isClickWindow)
        {
            GenericMenu menu=new GenericMenu();
            menu.AddItem(new GUIContent("删除节点"), false,OnOptionNode,ENodeType.DeleteNode);
            menu.AddItem(new GUIContent("Make Translation"), false,OnOptionNode,ENodeType.MakeTranslation);
            menu.ShowAsContext();
            e.Use();
            _isClickWindow = false;
        }
        else
        {
            GenericMenu menu=new GenericMenu();
            menu.AddItem(new GUIContent("添加节点"), false,OnOptionNode,ENodeType.ConversationNode);
            menu.ShowAsContext();
            e.Use();
            _lastClickWindowIndex = -1;
        }
        _isDrawLining = false;
    }

    private void RightMouseSelect()
    {
        IsFocusLine();
        if (_isClickLine)
        {
            
            //_isClickLine = false;
        }
        else
        {
            _isClickLine = false;
        }
    }


    /// <summary>
    /// 是否在选中得窗口上
    /// </summary>
    private void FocusWindowNode()
    {
        for (int i = Controller.WindowNodes.Count - 1; i >= 0; i--)
        {
            if (Controller.WindowNodes[i]._editorRect.Contains(_mousePotion))
            {
                _isClickWindow = true;
                _lastClickWindowIndex = i;
                break;
            }
            else
            {
                _isClickWindow = false;
            }
        }
    }

    private bool IsFocusWindowNode(Event e, out int nodeIndex)
    {
        for (int i = Controller.WindowNodes.Count - 1; i >= 0; i--)
        {
            if (Controller.WindowNodes[i]._editorRect.Contains(_mousePotion))
            {
                nodeIndex = i;
                return true;
            }
        }

        nodeIndex = -1;
        return false;
    }

    /// <summary>
    /// 添加节点
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void OnOptionNode(object type)
    {
        switch ((ENodeType)type)
        {
            case ENodeType.ConversationNode:
                ConversationNode node = new ConversationNode();
                node._editorRect=new Rect(_mousePotion.x,_mousePotion.y,200,200);
                Controller.AddNode(node);
                break;
            case ENodeType.DeleteNode:
                if (_lastClickWindowIndex!=-1)
                {
                   Controller.RemoveNode(_lastClickWindowIndex);
                    _lastClickWindowIndex = -1;
                }
                break;
            case ENodeType.MakeTranslation:
                _isDrawLining = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    private void DrawBezier(ConversationNodeTranslation nodeTranslation)
    {
        Rect start = Controller[nodeTranslation.FromIndex]._editorRect;
        Rect end = Controller[nodeTranslation.ToIndex]._editorRect;

        DrawBezier(start, end, out Vector2 startPos, out Vector2 endPos, _clickLine == nodeTranslation);
        
        //计算线段得区域
        Vector2 distanceVector = endPos - startPos;
        Vector3 crossVector = Vector3.Cross(distanceVector, Vector3.forward);
        
        Vector2 widthVector = crossVector.normalized * width;
        //沿法线方向与法线反方向各偏移一定距离
        Vector2 heightVector = startPos - endPos;

        nodeTranslation.LineRect =
            new ConversationNodeTranslation.NodeRect(widthVector, heightVector, (startPos + endPos) * 0.5f);
    }

    private void DrawBezier(Rect start, Rect end)
    {
        DrawBezier(start,end,out Vector2 startPos,out Vector2 endPos);
    }

    private void DrawBezier(Rect start, Rect end, out Vector2 startPos,out Vector2 endPos,bool isSelect = false)
    {
        
        
        //计算线得其实和初始点
        bool startXMore = start.x > end.x;
        bool startYMore = start.y > end.y;
        float matchX = (startXMore) ? (start.x - end.x) : (end.x - start.x) - start.width;
        float matchY = (startYMore) ? (start.y - end.y) : (end.y - start.y) - start.height;
        //Debug.Log("start"+start.x);
        if (matchX > matchY)
        {
            startPos = new Vector3(start.x + (startXMore ? 0 : start.width), start.y + start.height / 2, 0);
            endPos = new Vector3(end.x + (startXMore ? start.width : 0), end.y + end.height / 2, 0);
        }
        else
        {
            startPos = new Vector3(start.x + start.width / 2, start.y + (startYMore ? 0 : start.height), 0);
            endPos = new Vector3(end.x + end.width / 2, end.y + (startYMore ? end.height : 0), 0);
        }
        //画线
        Handles.BeginGUI();
        Vector2 offset = scrollPos*Vector2.one;
        Handles.color = isSelect ? Color.cyan : Color.black;
        Handles.DrawAAPolyLine(width, startPos+offset, endPos+offset);
        Vector2 v0 = startPos - endPos;
        v0 *= 10 / v0.magnitude;
        Vector2 v1 = new Vector2(v0.x * 0.866f - v0.y * 0.5f, v0.x * 0.5f + v0.y * 0.866f);
        Vector2 v2 = new Vector2(v0.x * 0.866f + v0.y * 0.5f, v0.x * -0.5f + v0.y * 0.866f); ;
        Handles.DrawAAPolyLine(width, (Vector2)endPos+offset + v1, endPos+offset, (Vector2)endPos+offset + v2);
        Handles.EndGUI();
        
    }

    /// <summary>
    /// 是否点击到线
    /// </summary>
    private void IsFocusLine()
    {
        bool tempClick = false;
        //画窗口
        foreach (var node in Controller.WindowNodes)
        {
            foreach (var translation in node.Translations)
            {
                if (translation.LineRect.Contain(_e.mousePosition))
                {
                    _isClickLine = true;
                    _clickLine = translation;
                    tempClick = true;
                    break;
                }
                else
                {
                    _isClickLine = false;
                    _clickLine = null;
                }
            }
            if (tempClick)
            {
                break;
            }
        }
    }
    
}
