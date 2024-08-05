using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain.Tools
{
    public static class ConstFields
    {
        public const float PI = 3.1415f;
        public const float PI2 = 6.283f;
        public const float PI1_2 = 1.5708f;
    }
    [Serializable]
    public struct RibbonNode
    {
        public float splitInterval;
        public float splitOffset;
        public Vector3 pos;
        public Quaternion rot;
    }

    public class RibbonSimulator : MonoBehaviour
    {
        [Header("RotatePart")] public Transform rotateCenter;
        public float rotateSpeed;
        public float rotateRadius;

        [Header("RibbonPart")] public RibbonNode[] ribbonNodes;
        public AnimationCurve mappingCurve;
        public bool useMapping = true;

        private List<Vector3> _splitPoints;
        private List<Transform> _splitTrans;
        private Transform _selfTrans;
        private Vector3 _hardConfig;
        private Vector3 _vParam;

        private void Start()
        {
            _selfTrans = transform;
            int splitCount = ribbonNodes.Length;
            _splitTrans = new List<Transform>();
            _splitPoints = new List<Vector3>();

            for (int i = 0; i < splitCount; i++)
            {
                GameObject newGobj = new GameObject($"Split_{i}");
                newGobj.transform.SetParent(transform);
                _splitTrans.Add(newGobj.transform);
            }
        }


        private bool _isStart;

        private void OnGUI()
        {
            if (GUILayout.Button("开始测试", GUILayout.MinWidth(200),
                    GUILayout.MinHeight(50)))
            {
                _isStart = true;
            }

            if (GUILayout.Button("停止测试", GUILayout.MinWidth(200),
                    GUILayout.MinHeight(50)))
            {
                _isStart = false;
            }

            if (GUILayout.Button("变化开关", GUILayout.MinWidth(200),
                    GUILayout.MinHeight(50)))
            {
                _startAddFirstConfigureOffset = !_startAddFirstConfigureOffset;
                if (_startAddFirstConfigureOffset)
                {
                    ribbonNodes[0].splitOffset = 0;
                }
            }
        }

        private bool _startAddFirstConfigureOffset;
        private float _rotateParam;

        private void FixedUpdate()
        {
            if (_startAddFirstConfigureOffset)
            {
                ribbonNodes[0].splitOffset = ((ribbonNodes[0].splitOffset + Time.fixedDeltaTime));
            }
            
            if (_isStart)
            {
                if(useMapping)
                {
                    _hardConfig = Vector3.up * (-Mathf.Min(mappingCurve.Evaluate(Mathf.Abs(rotateSpeed)),15)*(rotateSpeed > 0 ? 1 : -1);
                }
                if (rotateCenter != null)
                {
                    _rotateParam = (_rotateParam + rotateSpeed / 100) % ConstFields.PI2;
                    var rotateCenterPos = rotateCenter.position;
                    _selfTrans.position = rotateCenterPos + ComputePos(_rotateParam, rotateRadius);
                    transform.rotation = Quaternion.LookRotation(rotateCenterPos - _selfTrans.position);
                }
                Vector3 lastTransPos = _selfTrans.position;
                Quaternion lastTransRot = _selfTrans.rotation;
                for (int i = 0; i < _splitTrans.Count; i++)
                {
                    var currentTrans = _splitTrans[i];
                    var currentSplitConfig = ribbonNodes[i];
                    var direction = lastTransPos - currentTrans.position;
                    if((direction.sqrMagnitued - 0) > 0.001f)
                    {
                        currentTrans.rotation = Quaternion.LookRotation(direction);
                    }
                    currentTrans.position = lastTransPos + (Quaternion.Euler(_hardConfig) * lastTransRot) *
                        ComputePos(currentSplitConfig.splitOffset - ConstFields.PI1_2, currentSplitConfig.splitInterval);
                    lastTransPos = currentTrans.position;
                    lastTransRot = currentTrans.rotation;
                }
            }
        }

        private Vector3 ComputePos(float radian, float radius)
        {
            var useRadian = radian % ConstFields.PI2;
            float x = -radius * Mathf.Cos(useRadian);
            float z = radius * Mathf.Sin(useRadian);
            _vParam.x = x;
            _vParam.y = 0;
            _vParam.z = z;
            return _vParam;
        }

        private void OnDrawGizmos()
        {
            if (_splitTrans != null)
            {
                for (int i = 0; i < _splitTrans.Count; i++)
                {
                    var item = _splitTrans[i];
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(item.position, 0.1f);
                    if (i == 0)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(item.position, transform.position);
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(item.position, _splitTrans[i - 1].position);
                    }
                }
            }
        }
    }
}
