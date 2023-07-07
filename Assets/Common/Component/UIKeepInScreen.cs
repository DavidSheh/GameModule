// 保持 uGUI 制作的 Tips 显示在屏幕内
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FrameCore;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTW
{
	public enum DirectionType
	{
		None   = 0,
		Left   = 1,
		Right  = 2,
		Top    = 3,
		Bottom = 4
	}

	public enum AlignType
	{
		Horizontal = 1,
		Vertical   = 2,
	}
	
    public class UIKeepInScreen : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_Panel;
		[SerializeField][Tooltip("注意：箭头默认方向向下（可以加一个父节点，以使箭头默认向下）")]
		private RectTransform m_Arrow;
		
		[SerializeField][Tooltip("箭头位置离 Panel 边界的距离")]
		private Vector2 m_ArrowOffPanelEdge = Vector2.zero;
		
		[SerializeField][Tooltip("x, y, z, w 分别表示左、右、上、下的边界宽度")]
		private Vector4 m_BorderWidth; // x, y, z, w 分别表示左、右、上、下的边界宽度

		[SerializeField][Tooltip("优先对齐方式")]
		private AlignType m_AlignType = AlignType.Vertical;
		
		[SerializeField][Tooltip("是否检查 Panel 大小，如果背景动态变化则需要勾上这个选项")]
		private bool m_CheckPanelSize;
		private CanvasGroup m_CanvasGroup;
		
		private Camera m_MainCamera;
		private Camera m_UICamera;
		private RectTransform m_RectCanvas;

		private Vector2 m_ScreenPos;
		private DirectionType m_Direction;
		
		private void Awake()
		{
			m_MainCamera = Camera.main;
			IUIManager uiManger = GameEntry.UI;
			if (uiManger != null)
			{
				m_UICamera = uiManger.UICamera;
				m_RectCanvas = uiManger.RootCanvas.transform as RectTransform;
			}

			if (m_CheckPanelSize)
			{
				GameObject objParent = m_Panel.parent.gameObject;
				m_CanvasGroup = objParent.GetOrAddComponent<CanvasGroup>();
			}
		}

		private void Start()
	    {
		    Vector2 center = new Vector2(0.5f, 0.5f);
		    if (m_Arrow)
		    {
			    m_Arrow.pivot = center;
		    }

		    if (m_Panel)
		    {
			    m_Panel.pivot = center;
		    }
	    }

		/// <summary>
		/// 设置锚点，不传对象表示跟随手指
		/// </summary>
		/// <param name="rectTransform"></param>
		public void SetAnchor(RectTransform rectTransform = null, int dir = 0)
	    {
		    if (m_UICamera == null)
		    {
			    return;
		    }

		    m_Direction = (DirectionType) dir;

		    if (rectTransform)
		    {
			    m_ScreenPos = RectTransformUtility.WorldToScreenPoint(m_UICamera, rectTransform.position);
		    }
		    else
		    {
			    m_ScreenPos = Input.mousePosition;
		    }
		    
		    if (m_CheckPanelSize)
		    {
			    StartCoroutine(CheckPanelSize());
		    }
		    else
		    {
			    UpdatePosition(m_ScreenPos);
		    }
	    }
	    
	    public void SetAnchor(Vector3 woldPosition)
	    {
		    if(m_MainCamera != null)
		    {
			    m_ScreenPos = m_MainCamera.WorldToScreenPoint(woldPosition);
			    
			    if (m_CheckPanelSize)
			    {
				    StartCoroutine(CheckPanelSize());
			    }
			    else
			    {
				    UpdatePosition(m_ScreenPos);
			    }
		    }
	    }

	    private IEnumerator CheckPanelSize()
	    {
		    if (m_Panel == null)
		    {
			    yield break;
		    }

		    m_CanvasGroup.alpha = 0;
		    
		    yield return null;
		    
		    UpdatePosition(m_ScreenPos);
		    m_CanvasGroup.alpha = 1;
	    }
	    
	    private void UpdatePosition(Vector2 screenPos)
	    {
		    if (m_RectCanvas == null || m_UICamera == null)
		    {
			    return;
		    }

		    DirectionType dir = m_Direction;
		    if (dir == DirectionType.None)
		    {
			    dir = CalcArrowDirection(screenPos);
		    }
		    UpdateArrowPosition(screenPos, dir);
			UpdatePanelPosition(screenPos, dir);
	    }
	    
	    private void UpdateArrowPosition(Vector2 screenPos, DirectionType dir)
	    {
		    if (m_Arrow == null)
		    {
			    return;
		    }

		    float halfArrowWidth = m_Arrow.sizeDelta.x * 0.5f;
		    float halfArrowHeight = m_Arrow.sizeDelta.y * 0.5f;

		    float angle = CalcArrowAngle(dir);
		    m_Arrow.localEulerAngles = new Vector3(0, 0, angle);
		    
		    float minX = 0, maxX = 0, minY = 0, maxY = 0;
		    if (dir == DirectionType.Bottom || dir == DirectionType.Top) // 竖直方向
		    {
			    minX = m_BorderWidth.x + m_ArrowOffPanelEdge.x + halfArrowWidth;
			    maxX = Screen.width - m_BorderWidth.y - m_ArrowOffPanelEdge.x - halfArrowWidth;
				    
			    minY = m_BorderWidth.w + halfArrowHeight;
			    maxY = Screen.height - m_BorderWidth.z - halfArrowHeight;
		    }
		    else // 水平方向
		    {
			    minX = m_BorderWidth.x + halfArrowHeight;
			    maxX = Screen.width - m_BorderWidth.y - halfArrowHeight;
				    
			    minY = m_BorderWidth.w + m_ArrowOffPanelEdge.y + halfArrowWidth;
			    maxY = Screen.height - m_BorderWidth.z - m_ArrowOffPanelEdge.y - halfArrowWidth;
		    }
			    
		    Vector2 offset = screenPos;
		    offset.x = Mathf.Clamp(offset.x, minX, maxX);
		    offset.y = Mathf.Clamp(offset.y, minY, maxY);
			    
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectCanvas, offset, m_UICamera, out Vector2 arrowPos);
		    m_Arrow.position = m_RectCanvas.TransformPoint(arrowPos);
	    }

	    private float CalcArrowAngle(DirectionType dir)
	    {
		    float angle = 0;
		    switch (dir)
		    {
			    case DirectionType.Bottom:
				    // 箭头向下
				    angle = 0;
				    break;
			    case DirectionType.Top:
				    // 箭头向上
				    angle = 180;
				    break;
			    case DirectionType.Left:
				    // 箭头向左
				    angle = -90;
				    break;
			    case DirectionType.Right:
				    // 箭头向右
				    angle = 90;
				    break;
		    }

		    return angle;
	    }
	    
	    private void UpdatePanelPosition(Vector2 screenPos, DirectionType dir)
	    {
		    if (m_Panel == null)
		    {
			    return;
		    }
		    
		    float halfPanelWidth = m_Panel.sizeDelta.x * 0.5f;
		    float halfPanelHeight = m_Panel.sizeDelta.y * 0.5f;
		    
		    Vector2 offsetPanel = Vector2.zero;
		    switch (dir)
		    {
			    case DirectionType.Bottom:
				    // 箭头向下
				    offsetPanel.y += halfPanelHeight;
				    break;
			    case DirectionType.Top:
				    // 箭头向上
				    offsetPanel.y -= halfPanelHeight;
				    break;
			    case DirectionType.Left:
				    // 箭头向左
				    offsetPanel.x += halfPanelWidth;
				    break;
			    case DirectionType.Right:
				    // 箭头向右
				    offsetPanel.x -= halfPanelWidth;
				    break;
		    }
		    
		    // Log.Debug($"m_Panel.sizeDelta: ({m_Panel.sizeDelta.x}, {m_Panel.sizeDelta.y}), offsetPanel: ({offsetPanel.x}, {offsetPanel.y})");
		    
		    if(m_Arrow)
		    {
			    if (dir == DirectionType.Bottom || dir == DirectionType.Top) // 竖直方向
			    {
				    if (screenPos.x < Screen.width * 0.5f)
				    {
					    offsetPanel.x -= m_Arrow.sizeDelta.x * 0.5f - m_ArrowOffPanelEdge.x;
				    }
				    else
				    {
					    offsetPanel.x += m_Arrow.sizeDelta.x * 0.5f + m_ArrowOffPanelEdge.x;
				    }
			    }
			    else // 水平方向
			    {
				    if (screenPos.y < Screen.height * 0.5f)
				    {
					    offsetPanel.y -= m_Arrow.sizeDelta.x * 0.5f - m_ArrowOffPanelEdge.y;
				    }
				    else
				    {
					    offsetPanel.y += m_Arrow.sizeDelta.x * 0.5f + m_ArrowOffPanelEdge.y;
				    }
			    }
		    }

		    Vector2 offset = screenPos + offsetPanel;

		    float minX = halfPanelWidth + m_BorderWidth.x;
		    float maxX = Screen.width - halfPanelWidth - m_BorderWidth.y;
		    float minY = halfPanelHeight + m_BorderWidth.w;
		    float maxY = Screen.height - halfPanelHeight - m_BorderWidth.z;

		    offset.x = Mathf.Clamp(offset.x, minX, maxX);
		    offset.y = Mathf.Clamp(offset.y, minY, maxY);
		    
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectCanvas, offset, m_UICamera, out Vector2 panelPos);
		    m_Panel.position = m_RectCanvas.TransformPoint(panelPos);
		    
		    // Log.Debug($"screenPos: {screenPos}, offset: {offset}, panelPos: {panelPos}");
	    }

	    private DirectionType CalcArrowDirection(Vector2 screenPos)
	    {
		    DirectionType dir = DirectionType.None;
		    float arrowHegith = m_Arrow ? m_Arrow.sizeDelta.y : 0;
		    
		    if(m_AlignType == AlignType.Horizontal)
		    {
			    if (screenPos.y < m_BorderWidth.w + arrowHegith)
			    {
				    // 箭头向下
				    dir = DirectionType.Bottom;
			    }
			    else if (screenPos.y > Screen.height - m_BorderWidth.z - arrowHegith)
			    {
				    // 箭头向上
				    dir = DirectionType.Top;
			    }
			    else
			    {
				    if (screenPos.x >= 0 && screenPos.x < Screen.width * 0.5f)
				    {
					    // 箭头向左
					    dir = DirectionType.Left;
				    }
				    else
				    {
					    // 箭头向右
					    dir = DirectionType.Right;
				    }
			    }
		    }
		    else
		    {
			    if (screenPos.x < m_BorderWidth.x + arrowHegith)
			    {
				    // 箭头向左
				    dir = DirectionType.Left;
			    }
			    else if (screenPos.x > Screen.width - m_BorderWidth.y - arrowHegith)
			    {
				    // 箭头向右
				    dir = DirectionType.Right;
			    }
			    else
			    {
				    if (screenPos.y >= 0 && screenPos.y < Screen.height * 0.5f)
				    {
					    // 箭头向下
					    dir = DirectionType.Bottom;
				    }
				    else
				    {
					    // 箭头向上
					    dir = DirectionType.Top;
				    }
			    }
		    }

		    return dir;
	    }
	}
}
