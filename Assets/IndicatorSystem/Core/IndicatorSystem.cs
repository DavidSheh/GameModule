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
        private Dictionary<ulong, Vector3> lastPositions = new Dictionary<ulong, Vector3>();
        private List<ulong> keysToRemove = new List<ulong>(); // 预分配列表，减少GC
        
        private Camera mainCamera;
        private Camera uiCamera;
        private RectTransform canvasRect;
        private float indicatorSize;
        private Rect screenBounds;
        private Vector2Int lastScreenSize;
        private bool screenSizeChanged;

        // 私有构造函数，确保单例
        private IndicatorSystem() { }

        /// <summary>
        /// 初始化系统，需要传入摄像机和屏幕边距
        /// </summary>
        public void Initialize(Camera worldCamera, Camera uiCam, RectTransform canvas, float size)
        {
            mainCamera = worldCamera;
            uiCamera = uiCam;
            canvasRect = canvas;
            indicatorSize = size;
            UpdateScreenBounds();
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        public void AddTarget(ulong id, Transform target, Vector2 size = default)
        {
            if (target == null || Indicators.ContainsKey(id))
                return;

            var newData = new IndicatorData
            {
                ID = id, 
                Target = target,
                Size = size
            };
            Indicators.Add(id, newData);
            lastPositions[id] = target.position; // 初始化位置缓存
        }

        public void RemoveTarget(ulong id)
        {
            Indicators.Remove(id);
            lastPositions.Remove(id);
        }
        
        public void Clear()
        {
            Indicators.Clear();
            lastPositions.Clear();
        }

        /// <summary>
        /// 每帧调用此方法来更新所有指示器的数据
        /// </summary>
        public void Tick()
        {
            if (mainCamera == null || Indicators.Count == 0)
                return;
            
            // 只在屏幕尺寸变化时更新边界
            CheckScreenSizeChange();
            if (screenSizeChanged)
            {
                UpdateScreenBounds();
                screenSizeChanged = false;
            }

            // 清理无效的目标 - 使用预分配列表减少GC
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

            // 批量删除无效目标
            for (int i = 0; i < keysToRemove.Count; i++)
            {
                var key = keysToRemove[i];
                Indicators.Remove(key);
                lastPositions.Remove(key);
            }
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
                screenSizeChanged = true;
                lastScreenSize = currentScreenSize;
            }
        }

        private void UpdateIndicatorData(IndicatorData data)
        {
            Vector3 currentPosition = data.Target.position;
            bool positionChanged = true;
            
            // 检查位置是否变化，避免不必要的WorldToScreenPoint调用
            if (lastPositions.TryGetValue(data.ID, out Vector3 lastPosition))
            {
                positionChanged = Vector3.Distance(currentPosition, lastPosition) > 0.01f;
            }
            
            if (positionChanged)
            {
                // 更新位置缓存
                lastPositions[data.ID] = currentPosition;
                
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentPosition);

                if (data.Size != Vector2.zero) // 当需要考虑目标 Size 时
                {
                    // 与投影无关的边界检查 - 优化数学计算
                    Vector3 worldRight = mainCamera.transform.right * (data.Size.x * 0.5f);
                    Vector3 worldUp = mainCamera.transform.up * (data.Size.y * 0.5f);

                    Vector3 screenRight = mainCamera.WorldToScreenPoint(data.Target.position + worldRight);
                    Vector3 screenUp = mainCamera.WorldToScreenPoint(data.Target.position + worldUp);

                    // 使用平方距离比较，避免开方运算
                    float pixelWidthSq = (screenPoint - screenRight).sqrMagnitude * 4f;
                    float pixelHeightSq = (screenPoint - screenUp).sqrMagnitude * 4f;

                    // 使用近似矩形重叠检测
                    float halfWidth = Mathf.Sqrt(pixelWidthSq) * 0.5f;
                    float halfHeight = Mathf.Sqrt(pixelHeightSq) * 0.5f;
                    
                    Rect targetRect = new Rect(screenPoint.x - halfWidth, screenPoint.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
                    data.IsOffScreen = screenPoint.z < 0 || !screenBounds.Overlaps(targetRect);
                }
                else
                {
                    // 仅检查中心点
                    data.IsOffScreen = screenPoint.z < 0 || !screenBounds.Contains(screenPoint);
                }

                Vector2 localPoint;
                if (data.IsOffScreen)
                {
                    Vector3 cappedScreenPoint = GetCappedScreenPosition(screenPoint);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, cappedScreenPoint, uiCamera, out localPoint);
                    data.ScreenPosition = localPoint;

                    Vector3 direction = (data.Target.position - mainCamera.transform.position);
                    data.Angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                }
                else
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out localPoint);
                    data.ScreenPosition = localPoint;
                }
            }
        }
        
        private Vector3 GetCappedScreenPosition(Vector3 screenPoint)
        {
            if (screenPoint.z < 0)
            {
                screenPoint *= -1;
            }
            
            var cameraRect = mainCamera.pixelRect;
            Vector3 screenCenter = cameraRect.center;
            screenPoint -= screenCenter;

            // 预计算常用值
            float halfScreenWidth = (cameraRect.width * 0.5f) - indicatorSize;
            float halfScreenHeight = (cameraRect.height * 0.5f) - indicatorSize;

            // 避免除零错误和优化角度计算
            if (Mathf.Abs(screenPoint.x) < 0.001f)
            {
                return new Vector3(0, Mathf.Sign(screenPoint.y) * halfScreenHeight, 0) + screenCenter;
            }

            float slope = screenPoint.y / screenPoint.x; // 直接使用斜率，避免tan计算

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
