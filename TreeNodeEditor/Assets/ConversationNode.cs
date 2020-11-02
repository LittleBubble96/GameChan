using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConversationNode
{

    public List<ConversationNodeTranslation> Translations;
    
    public string ConversationContent;

    public ConversationNode(string content = "输入内容")
    {
        ConversationContent = content;
        Translations = new List<ConversationNodeTranslation>();
    }

    public void AddTranslation(ConversationNodeTranslation translation)
    {
        if (translation!=null)
        {
            Translations.Add(translation);
        }
    }

    public void RemoveTranslation(ConversationNodeTranslation translation)
    {
        if (translation!=null)
        {
            Translations.Remove(translation);
        }
    }

#if UNITY_EDITOR

    public Rect _editorRect;

    private Vector2 _scroll;

    public string Title;

    //private List<TranslationCondition> _translationConditions = new List<TranslationCondition>();
    public void DrawWindow(ConversationNodeController controller)
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        ConversationContent = EditorGUILayout.TextArea(ConversationContent, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add"))
        {
            controller.AddTranslation(this,null);
        }

        _editorRect.height = 200 + Translations.Count * 20;
        
        for (int i = 0; i < Translations.Count; i++)
        {
            Translations[i].DrawArea(this,controller);
        }
    }
    

#endif
}
