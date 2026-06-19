using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLib
{
    public class ScrollRectSnapEffect : ScrollRectEffect, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] private AnimationCurve _snapCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float _snapDuration = 0.2f;
        
        private int _cachedChildCount;
        private Coroutine _snapCoroutine;
        
        
        public AnimationCurve SnapCurve
        {
            get => _snapCurve;
            set => _snapCurve = value;
        }
        
        private RectTransform ViewPort => _scrollRect.viewport;
        private RectTransform Content => _scrollRect.content;

        
        public event Action<RectTransform> OnSnapped;

        

        protected override void Awake()
        {
            base.Awake();
            
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            IEnumerator DelaySnapCoroutine()
            {
                yield return new WaitForEndOfFrame();
                Snap();
            }
            
            StartCoroutine(DelaySnapCoroutine());
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);

                _snapCoroutine = null;
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Snap();
        }


        public override void OnRefreshed(IReadOnlyList<ScrollItem> items)
        {
            Snap();
        }

        public override void OnUpdated(IReadOnlyList<ScrollItem> items)
        {
        }
        
        
        
        public void Snap(bool useAnimation = true)
        {
            if (Content == null || 
                Content.childCount == 0 ||
                ViewPort == null)
                return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            _effectCore.UpdateItems();
            
            ScrollItem centerItem = new();
            centerItem.Distance = float.MaxValue;
            
            foreach (var item in _effectCore.Items)
            {
                if (item.IsActive && item.Distance < centerItem.Distance)
                {
                    centerItem = item;
                }
            }

            if (centerItem.RectTransform != null)
            {
                
                StartSnapCoroutine(centerItem.RectTransform, useAnimation);
            }
        }

        public void Snap(RectTransform target, bool useAnimation = true)
        {
            if (target == null)
                return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            
            _effectCore.UpdateItems();

            var item = _effectCore.Items.FirstOrDefault(item => item.RectTransform == target);
            if (item.RectTransform == null)
                return;
            
            StartSnapCoroutine(target, useAnimation);
        }
        
        private void StartSnapCoroutine(RectTransform target, bool useAnimation = true)
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
            }
                
            _snapCoroutine = StartCoroutine(SnapCoroutine(target, useAnimation));
        }

        private IEnumerator SnapCoroutine(RectTransform target, bool useAnimation = true)
        {
            yield return new WaitForEndOfFrame();
            
            _scrollRect.velocity = Vector2.zero;
            
            Vector2 targetLocalPos = ViewPort.InverseTransformPoint(target.position);
            var snapPos = Content.localPosition - (Vector3)(targetLocalPos - ViewPort.rect.center);


            if (useAnimation && 
                _snapCurve != null && 
                _snapDuration > 0)
            {
                var startPos = Content.localPosition;
                
                float timer = _snapDuration;
                while (timer > 0)
                {
                    timer -= Time.deltaTime;

                    var t = 1 - (timer / _snapDuration);
                    
                    Content.localPosition = Vector2.LerpUnclamped(startPos, snapPos, _snapCurve.Evaluate(t));
                    
                    yield return null;
                }
            }
            
            Content.localPosition = snapPos;
            
            _scrollRect.velocity = Vector2.zero;
            
            OnSnapped?.Invoke(target);
            
            _snapCoroutine = null;
        }
    }
}


