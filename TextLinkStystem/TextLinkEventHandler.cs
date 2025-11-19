using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TextLinkEventHandler : Singleton<TextLinkEventHandler>
{
    public enum LinkTag
    {
        None = 0,
        ShowTerms01,
        ShowTerms02,

        HV_ShowTerms01,
        HV_ShowTerms02,
        HV_ShowTerms03
    }
    Dictionary<LinkTag, UnityAction> _linkEvent = new();

    public void AddLinkTag(LinkTag tag, UnityAction action = null)
    {
        if (_linkEvent.ContainsKey(tag)) return;
        if (tag == LinkTag.None)
        {
            _linkEvent.Add(tag, null);
            return;
        }

        _linkEvent.Add(tag, action);
    }

    public void AddLinkAction(LinkTag tag, UnityAction action)
    {
        if(_linkEvent.ContainsKey(tag) == false) return;

        _linkEvent[tag] += action;
    }

    public void InvokeLink(LinkTag tag) 
    {
        if (tag == LinkTag.None) return;

        _linkEvent[tag]?.Invoke();
    }
    
    public void RemoveActionToLinkTag(LinkTag tag )
    {
        if (_linkEvent.ContainsKey(tag) == false) return;
        
        _linkEvent.Remove(tag);
    }

    public void LinkClear()
    {
        _linkEvent.Clear();
    }
}
