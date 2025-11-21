using UnityEngine;

namespace IndicatorSystem
{
    /// <summary>
    /// 存储单个指示器计算后的数据，不包含任何UI信息。
    /// </summary>
    public class IndicatorData
    {
        public ulong ID;
        public Transform Target;
        public Vector2 Size; // 目标的世界空间宽高
        public bool IsOffScreen;
        public Vector3 ScreenPosition;
        public float Angle; // 从屏幕中心指向目标的世界角度
    }
}
