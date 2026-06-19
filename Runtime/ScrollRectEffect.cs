using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LLib
{
    [RequireComponent(typeof(ScrollRectEffectCore))]
    public abstract class ScrollRectEffect : MonoBehaviour
    {
        protected RectTransform _rectTransform;
        protected ScrollRect _scrollRect;
        protected ScrollRectEffectCore _effectCore;
    
        
        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _effectCore = GetComponent<ScrollRectEffectCore>();
            _effectCore.BaseEffects.Add(this);
        }

        protected virtual void OnDestroy()
        {
            _effectCore.BaseEffects.Remove(this);
        }

        public abstract void OnRefreshed(IReadOnlyList<ScrollItem> items);
        public abstract void OnUpdated(IReadOnlyList<ScrollItem> items);
    }
}

