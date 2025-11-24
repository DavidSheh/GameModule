using System.Collections.Generic;
using UnityEngine;

namespace CyStarRoad.IndicatorSystem
{
    /// <summary>
    /// 核心逻辑系统，负责跟踪目标并计算指示器数据。
    /// </summary>
    public class IndicatorSystem
    {
        public static IndicatorSystem Instance { get; } = new IndicatorSystem();

        public readonly Dictionary<ulong, IndicatorData> Indicators = new Dictionary<ulong, IndicatorData>();
        // 缓存每个目标最后一次的屏幕坐标，而不是世界坐标
        private readonly Dictionary<ulong, Vector3> lastScreenPoints = new Dictionary<ulong, Vector3>();
        private readonly List<ulong> keysToRemove = new List<ulong>();
        
        private Camera mainCamera;
        private Camera uiCamera;
        private RectTransform canvasRect;
        private float indicatorSize;
        private Rect screenBounds;
        
        private Vector2Int lastScreenSize;
        private Transform playerTransform; // 玩家对象，用于作为指示器角度的参考点

        private IndicatorSystem() { }

        public void Initialize(Camera worldCamera, Camera uiCam, RectTransform canvas, float size)
        {
            mainCamera = worldCamera;
            uiCamera = uiCam;
            canvasRect = canvas;
            indicatorSize = size;
            
            if (mainCamera != null)
            {
                lastScreenSize = new Vector2Int(Screen.width, Screen.height);
                UpdateScreenBounds();
            }
        }

        /// <summary>
        /// 设置玩家对象，其屏幕位置将作为指示器角度的计算参考点。
        /// 如果为null，则默认为屏幕中心。
        /// </summary>
        public void SetPlayer(Transform player)
        {
            playerTransform = player;
            // 当玩家对象改变时，强制所有指示器更新角度
            lastScreenPoints.Clear(); 
        }

        public void AddTarget(ulong id, Transform target, Vector2 size = default)
        {
            if (target == null || Indicators.ContainsKey(id)) return;

            var newData = new IndicatorData { ID = id, Target = target, Size = size };
            Indicators.Add(id, newData);

            // 初始化屏幕坐标缓存
            if (mainCamera != null)
            {
                lastScreenPoints[id] = mainCamera.WorldToScreenPoint(target.position);
            }
        }

        public void RemoveTarget(ulong id)
        {
            Indicators.Remove(id);
            lastScreenPoints.Remove(id);
        }
        
        public void Clear()
        {
            Indicators.Clear();
            lastScreenPoints.Clear();
        }

        public void Tick()
        {
            if (mainCamera == null || Indicators.Count == 0) return;
            
            CheckScreenSizeChange();
            ProcessIndicators();
        }
        
        private void UpdateScreenBounds()
        {
            if (mainCamera == null) return;
            var cameraRect = mainCamera.pixelRect;
            screenBounds = new Rect(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height);
        }

        private void CheckScreenSizeChange()
        {
            var currentScreenSize = new Vector2Int(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                lastScreenSize = currentScreenSize;
                UpdateScreenBounds();
                // 当屏幕尺寸变化，强制所有指示器更新
                lastScreenPoints.Clear();
            }
        }

        private void ProcessIndicators()
        {
            keysToRemove.Clear();
            foreach (var indicator in Indicators.Values)
            {
                if (indicator.Target == null)
                {
                    keysToRemove.Add(indicator.ID);
                    continue;
                }
                UpdateIndicatorData(indicator);
            }

            foreach (var id in keysToRemove)
            {
                RemoveTarget(id);
            }
        }

        private void UpdateIndicatorData(IndicatorData data)
        {
            // 总是重新计算屏幕坐标，这样可以同时检测到目标移动和相机移动
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(data.Target.position);

            // 检查屏幕坐标是否发生显著变化 (阈值设为1个像素的平方，避免浮点数抖动)
            bool screenPointChanged = !lastScreenPoints.TryGetValue(data.ID, out var lastSp) ||
                                      (screenPoint - lastSp).sqrMagnitude > 1.0f;

            if (!screenPointChanged) return;

            // 更新缓存并继续处理
            lastScreenPoints[data.ID] = screenPoint;
            
            data.IsOffScreen = IsTargetOffScreen(data, screenPoint);

            if (data.IsOffScreen)
            {
                Vector3 cappedScreenPoint = GetCappedScreenPosition(screenPoint);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, cappedScreenPoint, uiCamera, out var localPoint);
                data.ScreenPosition = localPoint;
                
                // 计算指示器角度的参考点
                Vector2 referencePointInCanvas;
                if (playerTransform != null && mainCamera != null)
                {
                    // 获取玩家的世界坐标并投影到屏幕
                    Vector3 playerScreenPos = mainCamera.WorldToScreenPoint(playerTransform.position);
                    // 转换到画布本地坐标
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, playerScreenPos, uiCamera, out referencePointInCanvas);
                }
                else
                {
                    // 默认使用画布中心作为参考点
                    referencePointInCanvas = Vector2.zero;
                }

                // 获取目标（未截断的）屏幕坐标并转换到画布本地坐标
                Vector3 actualTargetScreenPos = mainCamera.WorldToScreenPoint(data.Target.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, actualTargetScreenPos, uiCamera, out var actualTargetCanvasPos);

                // 计算从参考点指向实际目标的向量
                Vector2 directionVector = actualTargetCanvasPos - referencePointInCanvas;

                // 设置指示器角度
                data.Angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out var localPoint);
                data.ScreenPosition = localPoint;
                data.Angle = 0f;
            }
        }

        private bool IsTargetOffScreen(IndicatorData data, Vector3 screenPoint)
        {
            if (screenPoint.z < 0) return true;

            if (data.Size == Vector2.zero)
            {
                return !screenBounds.Contains(screenPoint);
            }
            
            Vector3 worldRight = mainCamera.transform.right * (data.Size.x * 0.5f);
            Vector3 worldUp = mainCamera.transform.up * (data.Size.y * 0.5f);
            Vector3 screenRight = mainCamera.WorldToScreenPoint(data.Target.position + worldRight);
            Vector3 screenUp = mainCamera.WorldToScreenPoint(data.Target.position + worldUp);

            float halfWidth = (screenPoint - screenRight).magnitude;
            float halfHeight = (screenPoint - screenUp).magnitude;
            
            var targetRect = new Rect(screenPoint.x - halfWidth, screenPoint.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
            return !screenBounds.Overlaps(targetRect);
        }

        private Vector3 GetCappedScreenPosition(Vector3 screenPoint)
        {
            if (screenPoint.z < 0) screenPoint *= -1;
            
            var cameraRect = mainCamera.pixelRect;
            Vector3 screenCenter = cameraRect.center;
            screenPoint -= screenCenter;

            float halfScreenWidth = (cameraRect.width * 0.5f) - indicatorSize;
            float halfScreenHeight = (cameraRect.height * 0.5f) - indicatorSize;

            if (Mathf.Abs(screenPoint.x) < 0.001f)
            {
                return new Vector3(0, Mathf.Sign(screenPoint.y) * halfScreenHeight, 0) + screenCenter;
            }

            float slope = screenPoint.y / screenPoint.x;
            float x, y;
            if (Mathf.Abs(slope) * halfScreenWidth > halfScreenHeight)
            {
                y = Mathf.Sign(screenPoint.y) * halfScreenHeight;
                x = y / slope;
            }
            else
            {
                x = Mathf.Sign(screenPoint.x) * halfScreenWidth;
                y = x * slope;
            }
            return new Vector3(x, y, 0) + screenCenter;
        }
    }
}
