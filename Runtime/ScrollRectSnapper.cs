using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLib
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectSnapper : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] private AnimationCurve _snapCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float _snapDuration;
        
        private int _cachedChildCount;
        private ScrollRect _scrollRect;
        private Coroutine _snapCoroutine;
        private RectTransform _rectTransform;
        
        private RectTransform ViewPort => _scrollRect.viewport;
        private RectTransform Content => _scrollRect.content;


        public event Action<RectTransform> OnSnapped;


        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _rectTransform = GetComponent<RectTransform>();
        }


        private void OnEnable()
        {
            Snap();
        }

        
        private void LateUpdate()
        {
            var childCount = Content.childCount;
            if (childCount != _cachedChildCount)
            {
                _cachedChildCount = childCount;
            
                
                Snap();
            }
        }
        
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Snap();
        }

        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);

                _snapCoroutine = null;
            }
        }
        
        
        
        

        public void Snap(bool useAnimation = true)
        {
            if (Content == null || 
                Content.childCount == 0 ||
                ViewPort == null)
                return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            
            Vector2 center = ViewPort.rect.center;
            float maxDist = ViewPort.rect.size.magnitude * 0.5f;
            if (maxDist <= 0f)
                return;

            int centerIndex = -1;
            
            float closetDist = float.MaxValue;

            for (int i = 0; i < Content.childCount; i++)
            {
                var child =  Content.GetChild(i);
                if(!child.gameObject.activeSelf)
                    continue;

                Vector2 childPos = ViewPort.InverseTransformPoint(child.position);

                float dist = (childPos - center).magnitude;
                if (dist < closetDist)
                {
                    closetDist = dist;
                    centerIndex = i;
                }
            }

            if (centerIndex > -1)
            {
                var target = (RectTransform)Content.GetChild(centerIndex);
                OnSnap(target, useAnimation);
            }
        }


        public void Snap(RectTransform target, bool useAnimation = true)
        {
            if (target == null)
                return;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            
            for (int i = 0; i < Content.childCount; i++)
            {
                var child = Content.GetChild(i);
                if (child == target)
                {
                    OnSnap(target, useAnimation);
                    return;
                }
            }
        }
        
        
        private void OnSnap(RectTransform target, bool useAnimation = true)
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
            
            _scrollRect.StopMovement();
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
            
            OnSnapped?.Invoke(target);
            
            _snapCoroutine = null;
        }
    }
}


