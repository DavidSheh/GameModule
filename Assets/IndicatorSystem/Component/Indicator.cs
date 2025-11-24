using UnityEngine;

namespace CyStarRoad.IndicatorSystem
{
    /// <summary>
    /// 指示器箭头的默认朝向
    /// </summary>
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 指示器组件，用于每个指示器的属性设置和行为控制。
    /// </summary>
    public class Indicator : MonoBehaviour
    {
        [Tooltip("指示器箭头的 Transform 组件，用于指示方向")]
        [SerializeField] private Transform m_ArrowTransform;
        [Tooltip("指示器箭头的默认朝向")]
        [SerializeField] private ArrowDirection mArrowDirection = ArrowDirection.Down;
        
        private Transform m_CachedTransform;
        
        private void Awake()
        {
            m_CachedTransform = transform;
        }

        /// <summary>
        /// 根据传入的数据更新UI的显示状态、位置和旋转。
        /// </summary>
        /// <param name="data">指示器数据</param>
        public void UpdateUI(IndicatorData data)
        {
            gameObject.SetActive(data.IsOffScreen);

            if (data.IsOffScreen)
            {
                m_CachedTransform.localPosition = data.ScreenPosition;
                
                if (m_ArrowTransform != null)
                {
                    float finalAngle = CalculateFinalRotationAngle(data.Angle);
                    m_ArrowTransform.rotation = Quaternion.Euler(0, 0, finalAngle);
                }
                else
                {
                    Debug.LogWarning($"[Indicator] Indicator for target {data.Target.name} has no ArrowTransform assigned. Cannot rotate arrow.");
                }
            }
        }

        /// <summary>
        /// 根据默认箭头方向和计算出的目标角度，计算最终的旋转角度。
        /// </summary>
        /// <param name="dataAngle">从屏幕中心指向目标的原始角度 (0度为右，逆时针增加)</param>
        /// <returns>最终的Z轴旋转角度</returns>
        private float CalculateFinalRotationAngle(float dataAngle)
        {
            float offsetAngle = 0f;
            switch (mArrowDirection)
            {
                case ArrowDirection.Up:
                    offsetAngle = -90f; // 默认朝上，需要减去90度才能指向右方0度
                    break;
                case ArrowDirection.Down:
                    offsetAngle = 90f; // 默认朝下，需要加上90度才能指向右方0度
                    break;
                case ArrowDirection.Left:
                    offsetAngle = 180f; // 默认朝左，需要加上180度才能指向右方0度
                    break;
                case ArrowDirection.Right:
                    offsetAngle = 0f; // 默认朝右，无需偏移
                    break;
            }
            // dataAngle 是从屏幕中心指向目标的角度，0度为右方。
            // 我们需要将这个角度与箭头的默认朝向进行抵消，才能让箭头正确指向目标。
            return dataAngle + offsetAngle;
        }
    }
}
