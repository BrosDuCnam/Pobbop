using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class RectTransformData
    {
        public Vector3 LocalPosition;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector3 Scale;
        public Quaternion Rotation;
 
        public void PullFromTransform(RectTransform transform)
        {
            this.LocalPosition = transform.localPosition;
            this.AnchorMin = transform.anchorMin;
            this.AnchorMax = transform.anchorMax;
            this.Pivot = transform.pivot;
            this.AnchoredPosition = transform.anchoredPosition;
            this.SizeDelta = transform.sizeDelta;
            this.Rotation = transform.localRotation;
            this.Scale = transform.localScale;
        }
 
        public void PushToTransform(RectTransform transform)
        {
            transform.localPosition = this.LocalPosition;
            transform.anchorMin = this.AnchorMin;
            transform.anchorMax = this.AnchorMax;
            transform.pivot = this.Pivot;
            transform.anchoredPosition = this.AnchoredPosition;
            transform.sizeDelta = this.SizeDelta;
            transform.localRotation = this.Rotation;
            transform.localScale = this.Scale;
        }
        
        public void PushToTransform(RectTransform transform, float time, Ease ease = Ease.Linear, Action onUpdate = null, Action onComplete = null)
        {
            if (onUpdate == null) onUpdate = () => { };
            if (onComplete == null) onComplete = () => { };
            
            transform.DOSizeDelta(this.SizeDelta, time).SetEase(ease);
            transform.DOAnchorPos(this.AnchoredPosition, time).SetEase(ease);
            transform.DOAnchorMax(this.AnchorMax, time).SetEase(ease);
            transform.DOAnchorMin(this.AnchorMin, time).SetEase(ease);
            transform.DOPivot(this.Pivot, time).SetEase(ease);
            transform.DOLocalRotate(this.Rotation.eulerAngles, time).SetEase(ease);
            transform.DOScale(this.Scale, time).SetEase(ease).OnComplete(onComplete.Invoke).OnUpdate(onUpdate.Invoke);
        }
    }
}