//========================================================================
// Copyright (c) 2023 ChangYou, All rights reserved.
// http://www.changyou.com/
// 
// Filename:    UIKeepOutScreenTip.cs
// Time:        2023-01-06 17:28:27
// Author:      sheweiwei
// Email:       sheweiwei@cyou-inc.com
// Version:     v2022.0.1
// Description: UIKeepOutScreenTip
//========================================================================

using System;
using UnityEngine;
using UnityEngine.Events;
using XLua;

namespace ProjectDHLD
{
    public class UIKeepOutScreenTip : MonoBehaviour
    {
        [Header("目标对象")]
        public Transform Target;
        [Header("游戏UI根画布")]
        public Canvas Canvas;
        [Header("箭头（向下）")]
        public RectTransform Arrow;
        [Header("是否检查距离")]
        public bool IsCheckDistance;
        [Header("边界偏移")]
        public Vector4 BorderOffset;
        
        private static GameObject m_DummyTarget;

        private Camera m_MainCamera;
        private RectTransform m_PointerTransform;
        private RectTransform m_CanvasTransform;
        private RectTransform m_GuideRectTransform;

        [SerializeField]
        private UnityEvent m_InScreenAction = new UnityEvent();
        [SerializeField]
        private UnityEvent m_OutScreenAction = new UnityEvent();

        public Action<float, Vector3, Vector3> OnDistanceChanged;

        private Vector3 m_Center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        private Plane m_GroundPlane = new Plane(Vector3.up, Vector3.zero);

        private RectTransform m_RectCanvas;
        
        public UnityEvent InScreenAction
        {
            get
            {
                return m_InScreenAction;
            }
            set
            {
                m_InScreenAction = value;
            }
        }

        public UnityEvent OutScreenAction
        {
            get
            {
                return m_OutScreenAction;
            }
            set
            {
                m_OutScreenAction = value;
            }
        }

        public Camera MainCamera
        {
            get
            {
                if (m_MainCamera == null)
                {
                    m_MainCamera = Camera.main;
                }

                return m_MainCamera;
            }
        }

        public RectTransform GuideRectTransform
        {
            get
            {
                if (m_GuideRectTransform == null)
                {
                    m_GuideRectTransform = m_PointerTransform.GetComponent<RectTransform>();
                }

                return m_GuideRectTransform;
            }
        }

        private void Awake()
        {
            if (!Target)
            {
                transform.localScale = Vector3.zero;
            }
        }

        private void Start()
        {
            if (!m_PointerTransform)
            {
                m_PointerTransform = GetComponent<RectTransform>();
            }

            m_PointerTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (!Target)
            {
                return;
            }

            if (!MainCamera)
            {
                Debug.LogError("MainCamera is invalid.");

                return;
            }

            Vector3 targetScreenPoint = MainCamera.WorldToScreenPoint(Target.position);
            if (targetScreenPoint.z < 0)
            {
                targetScreenPoint.x = -targetScreenPoint.x;
                targetScreenPoint.y = -targetScreenPoint.y;
            }

            if (IsInScreen(targetScreenPoint))
            {
                transform.localScale = Vector3.zero;
                InScreenAction?.Invoke();
            }
            else
            {
                transform.localScale = Vector3.one;
                // 计算导航指引的位置（屏幕外）
                Vector3 guidePosition = targetScreenPoint;
                if (guidePosition.x < 0)
                {
                    guidePosition.x = BorderOffset.x;
                }
                else if (guidePosition.x > m_RectCanvas.sizeDelta.x)
                {
                    guidePosition.x = m_RectCanvas.sizeDelta.x + BorderOffset.y;
                }

                if (guidePosition.y < 0)
                {
                    guidePosition.y = BorderOffset.z;
                }
                else if (guidePosition.y > m_RectCanvas.sizeDelta.y)
                {
                    guidePosition.y = m_RectCanvas.sizeDelta.y + BorderOffset.w;
                }

                // 设置导航指引的位置
                if (GuideRectTransform)
                {
                    GuideRectTransform.anchoredPosition = guidePosition;
                }

                if (Arrow)
                {
                    Vector2 dir = targetScreenPoint - m_Center;
                    Arrow.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, dir) - 180);
                }

                if (IsCheckDistance)
                {
                    CheckDistance();
                }

                OutScreenAction?.Invoke();
            }
        }

        public bool IsInScreen()
        {
            if (m_MainCamera != null)
            {
                Vector2 viewPos = m_MainCamera.WorldToViewportPoint(Target.position);
                Vector3 dir = (Target.position - m_MainCamera.transform.position).normalized;
                float dot = Vector3.Dot(m_MainCamera.transform.forward, dir);

                return (dot > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1);
            }

            return true;
        }

        public bool IsInScreen(Vector2 targetScreenPoint)
        {
            return targetScreenPoint.x > 0 && targetScreenPoint.x < m_RectCanvas.sizeDelta.x && targetScreenPoint.y > 0 && targetScreenPoint.y < m_RectCanvas.sizeDelta.y;
        }

        private void CheckDistance()
        {
            if (m_MainCamera != null)
            {
                Ray ray = m_MainCamera.ScreenPointToRay(m_Center);
                if (m_GroundPlane.Raycast(ray, out float enter))
                {
                    Vector3 pos = ray.GetPoint(enter);
                    float distance = Vector3.Distance(Target.position, pos);
                    OnDistanceChanged?.Invoke(distance, pos, Target.position);
                }
            }
        }

        /// <summary>
        /// 设置目标参数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="canvas"></param>
        public void SetTargetParam(Transform target, Canvas canvas)
        {
            Target = target;
            Canvas = canvas;
            
            m_RectCanvas = Canvas.transform as RectTransform;
            
            m_Center = new Vector3(m_RectCanvas.sizeDelta.x * 0.5f, m_RectCanvas.sizeDelta.y * 0.5f, 0);
        }

        /// <summary>
        /// 设置目标虚拟参数
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="canvas"></param>
        public void SetDummyTargetParam(Vector3 targetPosition, Canvas canvas)
        {
            if (m_DummyTarget == null)
            {
                m_DummyTarget = new GameObject("_DummyTarget");
            }

            m_DummyTarget.hideFlags = HideFlags.HideAndDontSave;
            m_DummyTarget.transform.position = targetPosition;

            SetTargetParam(m_DummyTarget.transform, canvas);
        }

        private void OnDestroy()
        {
            if (m_DummyTarget != null)
            {
                GameObject.Destroy(m_DummyTarget);
            }
        }
    }
}
