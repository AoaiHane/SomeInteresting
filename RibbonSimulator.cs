using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain.Tools
{
    [Serializable]
    public struct RibbonNode
    {
        public float splitInterval;
        public float splitOffset;
    }

    public class RibbonSimulator : MonoBehaviour
    {
        [Header("RotatePart")] public Transform rotateCenter;
        public float rotateSpeed;
        public float rotateRadius;

        [Header("RibbonPart")] public RibbonNode[] roibbonNodes;
        public float hard;

        private List<Vector3> _splitPoints;
        private List<Transform> _splitTrans;
        private Transform _selfTrans;

        private void Start()
        {
            _selfTrans = transform;
            int splitCount = roibbonNodes.Length;
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
                    roibbonNodes[0].splitOffset = 0;
                }
            }
        }

        private bool _startAddFirstConfigureOffset;
        private float _rotateParam;

        private void FixedUpdate()
        {
            if (_startAddFirstConfigureOffset)
            {
                roibbonNodes[0].splitOffset = ((roibbonNodes[0].splitOffset + Time.fixedDeltaTime) * hard);
            }
            
            if (_isStart)
            {
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
                    var currentSplitConfig = roibbonNodes[i];

                    currentTrans.rotation = Quaternion.LookRotation(lastTransPos - currentTrans.position);
                    currentTrans.position = lastTransPos + lastTransRot *
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
            return new Vector3(x, 0, z);
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