using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace CyStarRoad.IndicatorSystem
{
    /// <summary>
    /// 视图层：负责将IndicatorSystem计算出的数据渲染到UI上。
    /// </summary>
    public class IndicatorManager : MonoBehaviour
    {
        [SerializeField] private Indicator indicatorPrefab; // 指示器UI预制体
        [SerializeField] private RectTransform canvasRect;   // 画布的RectTransform
        [SerializeField] private float indicatorSize = 52f;  // UI贴边的边距
        [SerializeField] private int maxIndicatorCount = 20;  // 最大指示器显示个数
        
        private Camera mainCamera;
        private Dictionary<ulong, Indicator> activeIndicators = new Dictionary<ulong, Indicator>();
        private Queue<Indicator> indicatorPool = new Queue<Indicator>();
        private List<ulong> keysToRemove = new List<ulong>();

        private void Awake()
        {
            if (indicatorPrefab != null)
            {
                indicatorPrefab.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            var uiCamera = GameUISystem.Inst.getUICamera();
            // 初始化核心逻辑系统
            IndicatorSystem.Instance.Initialize(mainCamera, uiCamera, canvasRect, indicatorSize);
        }

        private void LateUpdate()
        {
            IndicatorSystem.Instance.Tick();

            var indicatorsData = IndicatorSystem.Instance.Indicators;
            
            // 清空待删除列表
            keysToRemove.Clear();

            // 首先处理需要回收的指示器
            foreach (var activeIndicator in activeIndicators)
            {
                if (!indicatorsData.ContainsKey(activeIndicator.Key))
                {
                    keysToRemove.Add(activeIndicator.Key);
                    ReturnIndicatorToPool(activeIndicator.Value);
                }
            }

            // 批量删除回收的指示器
            foreach (var key in keysToRemove)
            {
                activeIndicators.Remove(key);
            }

            // 处理所有数据，同时创建新实例和更新UI
            foreach (var data in indicatorsData.Values)
            {
                if (!activeIndicators.TryGetValue(data.ID, out Indicator indicatorUI))
                {
                    // 创建新实例
                    indicatorUI = GetIndicatorFromPool();
                    if (indicatorUI != null)
                    {
                        activeIndicators.Add(data.ID, indicatorUI);
                    }
                    else
                    {
                        continue; // 如果创建失败，跳过更新
                    }
                }

                // 更新UI
                indicatorUI.UpdateUI(data);
            }
        }

        private Indicator GetIndicatorFromPool()
        {
            // 优先从对象池获取
            while (indicatorPool.Count > 0)
            {
                var indicator = indicatorPool.Dequeue();
                if (indicator != null && indicator.gameObject != null)
                {
                    indicator.gameObject.SetActive(true);
                    return indicator;
                }
            }

            // 对象池为空时创建新实例
            if (indicatorPrefab == null)
            {
                Debug.LogError("[IndicatorManager] Indicator Prefab is null!");
                return null;
            }

            var newIndicator = Instantiate<Indicator>(indicatorPrefab, canvasRect, false);
            if (newIndicator == null)
            {
                Debug.LogError($"[IndicatorManager] Failed to instantiate Indicator Prefab '{indicatorPrefab.name}'!");
                return null;
            }

            newIndicator.gameObject.SetActive(true);
            return newIndicator;
        }

        private void ReturnIndicatorToPool(Indicator indicator)
        {
            if (indicator == null || indicator.gameObject == null)
                return;

            indicator.gameObject.SetActive(false);
            
            // 限制对象池大小，避免内存占用过大
            if (indicatorPool.Count <= maxIndicatorCount)
            {
                indicatorPool.Enqueue(indicator);
            }
            else
            {
                Destroy(indicator.gameObject);
            }
        }
    }
}
