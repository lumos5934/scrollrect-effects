using System.Collections.Generic;
using UnityEngine;

namespace LLib
{
    public class ScrollRectRotationEffect : ScrollRectLayoutEffect
    {
        public enum Axis
        {
            Horizontal,
            Vertical
        }
        
        [Space(10f)]
        [Header("Rotation")] 
        [SerializeField] private bool _useMirror;
        [SerializeField] private Axis _axis;
        [SerializeField] private Vector3 _center = Vector3.zero;
        [SerializeField] private Vector3 _edge = Vector3.zero;


        public override void OnRefreshed(IReadOnlyList<ScrollItem> items)
        {
        }

        public override void OnUpdated(IReadOnlyList<ScrollItem> items)
        {
            foreach (var item in items)
            {
                float normalized = GetNormalizedDistance(item); 
                float weight = _curve.Evaluate(normalized);

                float sign = 1f;
                if (_useMirror)
                {
                    float dir = (_axis == Axis.Horizontal) ? item.Direction.x : item.Direction.y;
                    sign = Mathf.Sign(dir);
                }

                Vector3 target = Vector3.LerpUnclamped(_center, _edge * sign, weight);
                item.RectTransform.localRotation = Quaternion.Euler(target);
            }
        }
    }
}