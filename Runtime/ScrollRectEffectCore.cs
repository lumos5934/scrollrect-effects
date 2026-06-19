using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LLib
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEffectCore : MonoBehaviour
    {
        internal List<ScrollRectEffect> BaseEffects = new();
        
        private List<ScrollItem> _items = new();
        private ScrollRect _scrollRect;
        private int _childCountCache = -1;
        
        public IReadOnlyList<ScrollRectEffect> Effects => BaseEffects;
        public IReadOnlyList<ScrollItem> Items => _items;

        private RectTransform ViewPort => _scrollRect.viewport;
        private RectTransform Content => _scrollRect.content;

        
        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        protected virtual void LateUpdate()
        {
            int currentChildCount = transform.childCount;
    
            if (_childCountCache != currentChildCount)
            {
                RefreshItems();
        
                _childCountCache = currentChildCount;
            }
            
            UpdateItems();
        }
  
        
        public void UpdateItems()
        {
            if (ViewPort == null || 
                Content == null)
                return;

            Vector2 center = ViewPort.rect.center;

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i]; 
    
                item.IsActive = item.RectTransform.gameObject.activeInHierarchy;
                if (!item.IsActive)
                {
                    item.Distance = 0;
                    item.NormalizedDistance = 0;
                    item.Direction = Vector2.zero;
                }
                else
                {
                    Vector2 childPos = ViewPort.InverseTransformPoint(item.RectTransform.position);
                    Vector2 offset = childPos - center;

                    float distance = offset.magnitude;
                    var direction = offset.normalized;

                    item.Distance = distance;
                    item.Direction = direction;
                    
                    float halfWidth = ViewPort.rect.width / 2f;
                    float halfHeight = ViewPort.rect.height / 2f;

                    float absX = Mathf.Abs(direction.x);
                    float absY = Mathf.Abs(direction.y);

                    float distanceToEdge = absX * halfHeight > absY * halfWidth ?
                        halfWidth / absX : 
                        halfHeight / absY;

                    item.NormalizedDistance = Mathf.Clamp01(distance / distanceToEdge);
                }
                
                _items[i] = item;
            }

            foreach (var effect in Effects)
            {
                effect.OnUpdated(_items);
            }
        }
        
        private void RefreshItems()
        {
            _items.Clear();

            foreach (RectTransform child in Content)
            {
                _items.Add(new ScrollItem()
                {
                    RectTransform = child
                });
            }
            
            foreach (var effect in Effects)
            {
                effect.OnRefreshed(_items);
            }
        }
    }
}



