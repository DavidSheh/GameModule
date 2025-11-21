// 忽略安全区域，全屏适配组件

using System.Collections;
using ProjectDHLD;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 全屏适配组件：支持 Image 或 RawImage 组件的全屏显示，不受父对象的 anchor 和 pivot 影响
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIFullScreenAdapter : MonoBehaviour
{
    [SerializeField, Tooltip("是否保持图片原始宽高比")]
    private bool m_KeepAspectRatio;

    [SerializeField, Range(0.8f, 1.2f), Tooltip("全屏缩放因子")]
    private float m_ScaleFactor = 1.0f; // 增加额外缩放以确保不会穿帮

    [SerializeField, Tooltip("强制图片基于 SizeDelta 拉伸")]
    private bool m_ForceSize;
    
    private RectTransform m_RectTransform;
    private RectTransform m_RootCanvasRect;
    private Vector2 m_ReferenceResolution;
    private Vector2 m_OriginalSize;
    
    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        
        m_RootCanvasRect = GameEntry.UI.RootCanvas.GetComponent<RectTransform>();
        m_ReferenceResolution = m_RootCanvasRect.sizeDelta;

        m_OriginalSize = CalcOriginalSize();
    }
    
    IEnumerator Start()
    {
        yield return null;
        ApplyFullScreen();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyFullScreen();
        }
        else
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ApplyFullScreen();
                }
            };
        }
    }
#endif

    /// <summary>
    /// 计算原始大小
    /// </summary>
    /// <returns></returns>
    private Vector2 CalcOriginalSize()
    {
        var rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            return new Vector2(rawImage.texture.width, rawImage.texture.height);
        }

        var image = GetComponent<Image>();
        if (image != null && image.sprite != null)
        {
            return new Vector2(image.sprite.rect.width, image.sprite.rect.height);
        }

        return m_RectTransform.sizeDelta;
    }
    
    /// <summary>
    /// 计算等比缩放尺寸
    /// </summary>
    /// <returns>计算后的最终尺寸</returns>
    private Vector2 CalcKeepAspectRatioSize()
    {
        // 计算宽高比
        Vector2 size = m_ForceSize ? m_RectTransform.sizeDelta : m_OriginalSize;
        float imageAspect = size.x / size.y;
        float screenAspect = m_ReferenceResolution.x / m_ReferenceResolution.y;
        
        Vector2 finalSize;
        if (imageAspect > screenAspect)
        {
            // 图片更宽，以高度为基准并放大
            finalSize.y = m_ReferenceResolution.y * m_ScaleFactor;
            finalSize.x = finalSize.y * imageAspect;
        }
        else
        {
            // 图片更高，以宽度为基准并放大
            finalSize.x = m_ReferenceResolution.x * m_ScaleFactor;
            finalSize.y = finalSize.x / imageAspect;
        }
        return finalSize;
    }
    
    /// <summary>
    /// 应用全屏设置
    /// </summary>
    private void ApplyFullScreen()
    {
        if (m_OriginalSize != Vector2.zero)
        {
            Vector2 finalSize;
            if (m_KeepAspectRatio)
            {
                finalSize = CalcKeepAspectRatioSize();
            }
            else
            {
                // 非等比拉伸模式，直接使用屏幕分辨率
                finalSize = m_ReferenceResolution * m_ScaleFactor;
            }
            
            // 设置RectTransform属性
            var center = new Vector2(0.5f, 0.5f); 
            m_RectTransform.anchorMin = center;
            m_RectTransform.anchorMax = center;
            m_RectTransform.pivot = center;
            m_RectTransform.sizeDelta = finalSize;
            m_RectTransform.anchoredPosition = Vector2.zero;

            AdjustOffset();
        }
    }
    
    private void AdjustOffset()
    {
        var parent = m_RectTransform.parent;
        var offset = Vector3.zero;
        while (null != parent)
        {
            if (parent == m_RootCanvasRect)
            {
                break;
            }
            offset += parent.localPosition;
            parent = parent.parent;
        }

        m_RectTransform.localPosition = -offset;
    }
}