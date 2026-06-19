using System.Collections.Generic;
using UnityEngine;

namespace LLib
{
    public class ScrollRectScaleEffect : ScrollRectLayoutEffect
    {
        
        [Space(10f)]
        [Header("Scale")]
        [SerializeField] private Vector3 _center = Vector3.one;
        [SerializeField] private Vector3 _edge = Vector3.zero;


        public override void OnRefreshed(IReadOnlyList<ScrollItem> items)
        {
        }

        public override void OnUpdated(IReadOnlyList<ScrollItem> items)
        {
            foreach (var item in items)
            {
                float weight = _curve.Evaluate(GetNormalizedDistance(item));

                item.RectTransform.localScale = Vector3.Lerp(_center, _edge, weight);
            }
        }
    }
}