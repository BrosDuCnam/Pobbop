    using System;
    using System.Collections;
    using JetBrains.Annotations;
    using UnityEngine;

    public class ThrowableObject : MonoBehaviour
    {
        [SerializeField] [NotNull] private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Throw(Vector3 step, Transform target, float speed, AnimationCurve curve)
        {
            StartCoroutine(ThrowEnumerator(step, target, speed, curve));
        }
        
        public void Throw(Vector3 step, Transform target, float speed)
        {
            StartCoroutine(ThrowEnumerator(step, target, speed, AnimationCurve.Linear(0, 1, 1, 1)));
        }
        
        private IEnumerator ThrowEnumerator(Vector3 step, Transform target, float speed, AnimationCurve curve)
        {
            Vector3 origin = transform.position;
            
            if (_rigidbody != null)
            {
                _rigidbody.useGravity = false;
            }
            float time = 0;
            float distance = Utils.BezierCurveDistance(origin, step, target.position, 10);
            float i = 1 / (distance / speed);
            print(i);
            
            while (time < 1)
            {
                transform.position = Utils.BezierCurve(origin, step, target.position, time);
                
                new WaitForEndOfFrame();
                print(i * curve.Evaluate(time));
                time += (i * curve.Evaluate(time)) * Time.deltaTime;
                yield return null;
            }
        }
        
    }