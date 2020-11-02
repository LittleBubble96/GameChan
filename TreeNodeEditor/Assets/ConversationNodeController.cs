using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationNodeController 
{
    public List<ConversationNode> WindowNodes = new List<ConversationNode>();
    
    public void AddNode(ConversationNode node)
    {
        if (node!=null)
        {
            WindowNodes.Add(node);
        }
    }

    public void RemoveNode(ConversationNode node)
    {
        int index = WindowNodes.FindIndex((n) => n == node);
        
        if (index >= 0)
        {
            RemoveNode(index);
        }
    }

    public void RemoveNode(int index)
    {
        for (int i = WindowNodes.Count - 1; i >= 0; i--) 
        {
            for (int j = WindowNodes[i].Translations.Count - 1; j >= 0; j--)
            {
                ConversationNodeTranslation translation = WindowNodes[i].Translations[j];
                if (translation.FromIndex==index||translation.ToIndex==index)
                {
                    RemoveTranslation(translation);
                }
            }
        }
      
        WindowNodes.RemoveAt(index);
    }

    public void AddTranslation(ConversationNodeTranslation translation)
    {
        this[translation.FromIndex].AddTranslation(translation);
    }
    
    public void AddTranslation(ConversationNode from,ConversationNode to)
    {
        int fromIndex = WindowNodes.FindIndex((node) => node == from);
        int toIndex = WindowNodes.FindIndex((node) => node == to);
        AddTranslation(new ConversationNodeTranslation(fromIndex, toIndex));
    }
    
    public void RemoveTranslation(ConversationNodeTranslation translation)
    {
        this[translation.FromIndex].RemoveTranslation(translation);
    }
    
    public ConversationNode this[int index]
    {
        get { return WindowNodes[index]; }
    }
}
