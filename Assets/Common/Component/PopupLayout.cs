// 带箭头弹窗布局组件：保持弹窗在屏幕内，同时箭头方向自动调整
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Component
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

	public class PopupLayout : MonoBehaviour
	{
		[SerializeField][Tooltip("弹窗背景板")]
		private RectTransform m_Panel;
		[SerializeField][Tooltip("弹窗箭头（箭头方向默认向下，可以加一个父节点，以使箭头默认向下")]
		private RectTransform m_Arrow;

		[SerializeField][Tooltip("x, y, z, w 分别表示左、右、上、下的边界宽度")]
		private Vector4 m_BorderWidth; // x, y, z, w 分别表示左、右、上、下的边界宽度

		[SerializeField][Tooltip("箭头指向点偏移（相对箭头方向，单位：像素）。x：沿箭头底边方向偏移；y：沿箭头指向方向偏移")]
		private Vector2 m_AnchorOffset;

		[SerializeField][Tooltip("优先对齐方式")]
		private AlignType m_AlignType = AlignType.Vertical;

		[SerializeField][Tooltip("是否检查 Panel 大小，如果背景动态变化则需要勾上这个选项")]
		private bool m_CheckPanelSize;

		private Camera m_MainCamera;
		private Camera m_UICamera;
		private RectTransform m_RectCanvas;

		private Vector2 m_ScreenPos;
		private DirectionType m_Direction;

		/// <summary>
		/// 方向描述：把分散在多处的方向分支收敛为一张数据表。
		/// Angle：箭头旋转角度（箭头默认方向向下）；
		/// Vertical：主轴是否为竖直方向（Top/Bottom 为 true，Left/Right 为 false）；
		/// PointSign：沿主轴的箭头指向符号（面板落在指向的反方向）。
		/// </summary>
		private readonly struct DirDesc
		{
			public readonly float Angle;
			public readonly bool Vertical;
			public readonly int PointSign;

			public DirDesc(float angle, bool vertical, int pointSign)
			{
				Angle = angle;
				Vertical = vertical;
				PointSign = pointSign;
			}
		}

		private DirDesc GetDesc(DirectionType dir)
		{
			switch (dir)
			{
				case DirectionType.Top:    return new DirDesc(180f, true, 1);
				case DirectionType.Bottom: return new DirDesc(0f, true, -1);
				case DirectionType.Left:   return new DirDesc(-90f, false, -1);
				case DirectionType.Right:  return new DirDesc(90f, false, 1);
				default:                   return new DirDesc(0f, true, -1);
			}
		}

		private void Awake()
		{
			m_MainCamera = Camera.main;
			InitCanvas();
			SetPivotToArrowTip(m_Arrow);
			SetPivotToCenter(m_Panel);
		}

		/// <summary>
		/// 设置锚点
		/// </summary>
		/// <param name="uiPosition">UI元素的世界坐标</param>
		/// <param name="dir">默认自动计算箭头朝向</param>
		public void SetAnchor(Vector3 uiPosition, DirectionType dir = DirectionType.None)
		{
			if (!EnsureCanvas())
			{
				return;
			}

			m_Direction = dir;
			m_ScreenPos = RectTransformUtility.WorldToScreenPoint(m_UICamera, uiPosition);
			RefreshPosition();
		}

		/// <summary>
		/// 设置锚点
		/// </summary>
		/// <param name="woldPosition">世界坐标</param>
		public void SetWorldAnchor(Vector3 woldPosition)
		{
			if (m_MainCamera == null)
			{
				m_MainCamera = Camera.main;
			}

			if (m_MainCamera == null)
			{
				return;
			}

			m_ScreenPos = m_MainCamera.WorldToScreenPoint(woldPosition);
			RefreshPosition();
		}

		/// <summary>
		/// 设置箭头指向点偏移。
		/// x：沿箭头底边方向偏移；
		/// y：沿箭头指向方向偏移。
		/// 例如：
		/// Bottom / Top 时，x 表示屏幕水平偏移，y 表示屏幕竖直偏移；
		/// Left / Right 时，x 表示屏幕竖直偏移，y 表示屏幕水平偏移。
		/// </summary>
		public void SetAnchorOffset(Vector2 anchorOffset)
		{
			m_AnchorOffset = anchorOffset;
		}

		/// <summary>
		/// 通过自身所在层级向上查找最近的 Canvas，自动获取 UICamera 与 Canvas 的 RectTransform。
		/// 不再依赖任何外部框架（IUIManager / GameEntry），组件可独立挂载到任意 UI 节点上。
		/// </summary>
		private void InitCanvas()
		{
			Canvas canvas = GetComponentInParent<Canvas>();
			if (canvas == null)
			{
				return;
			}

			m_RectCanvas = canvas.transform as RectTransform;
			// Screen Space - Camera 模式下使用 Canvas 的 worldCamera；
			// Screen Space - Overlay 模式下 worldCamera 为 null，此时坐标转换传 null 即可。
			m_UICamera = canvas.worldCamera;
		}

		/// <summary>
		/// 确保已获取到 Canvas，必要时惰性初始化。返回是否可用。
		/// </summary>
		private bool EnsureCanvas()
		{
			if (m_RectCanvas == null)
			{
				InitCanvas();
			}

			return m_RectCanvas != null;
		}

		private void SetPivotToCenter(RectTransform rectTransform)
		{
			if (rectTransform == null)
			{
				return;
			}

			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}

		private void SetPivotToArrowTip(RectTransform rectTransform)
		{
			if (rectTransform == null)
			{
				return;
			}

			// 箭头默认方向向下，尖端位于图片底边。
			// 将 pivot 设为底边中点（尖端），使定位与旋转都以尖端为基准，
			// 尖端可精确指向传入的坐标。
			rectTransform.pivot = new Vector2(0.5f, 0f);
		}

		private void RefreshPosition()
		{
			if (!EnsureCanvas())
			{
				return;
			}

			if (m_CheckPanelSize)
			{
				RebuildPanelLayoutImmediate();
			}

			UpdatePosition(m_ScreenPos);
		}

		private void RebuildPanelLayoutImmediate()
		{
			if (m_Panel == null)
			{
				return;
			}

			Canvas.ForceUpdateCanvases();

			RectTransform current = m_Panel;
			while (current != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(current);

				if (current == m_RectCanvas)
				{
					break;
				}

				current = current.parent as RectTransform;
			}
		}

		private void UpdatePosition(Vector2 screenPos)
		{
			if (m_RectCanvas == null)
			{
				return;
			}

			float screenWidth = Screen.width;
			float screenHeight = Screen.height;

			DirectionType dir = m_Direction;
			if (dir == DirectionType.None)
			{
				dir = CalcArrowDirection(screenPos, screenWidth, screenHeight);
			}

			DirDesc desc = GetDesc(dir);

			// 一次性取出 Panel/Arrow 尺寸，避免下游重复计算。
			Vector2 panelSize = GetRectSize(m_Panel);
			Vector2 arrowSize = GetRectSize(m_Arrow);

			// 方向确定后再应用锚点偏移，避免 dir=None 时按错误方向解析偏移。
			Vector2 anchorPos = ApplyAnchorOffset(screenPos, desc);
			Vector2 panelScreenPos = UpdatePanelPosition(anchorPos, desc, panelSize, arrowSize, screenWidth, screenHeight);
			UpdateArrowPosition(anchorPos, desc, panelScreenPos, panelSize, arrowSize);
		}

		private Vector2 UpdatePanelPosition(Vector2 screenPos, DirDesc desc, Vector2 panelSize, Vector2 arrowSize, float screenWidth, float screenHeight)
		{
			if (m_Panel == null)
			{
				return screenPos;
			}

			float halfPanelWidth = panelSize.x * 0.5f;
			float halfPanelHeight = panelSize.y * 0.5f;
			float arrowHeight = arrowSize.y;

			// 面板落在箭头指向的反方向，需为箭头预留完整高度使尖端落在锚点上。
			int panelSign = -desc.PointSign;
			Vector2 offset = screenPos;
			if (desc.Vertical)
			{
				offset.y += panelSign * (halfPanelHeight + arrowHeight);
			}
			else
			{
				offset.x += panelSign * (halfPanelWidth + arrowHeight);
			}

			float minX = halfPanelWidth + m_BorderWidth.x;
			float maxX = screenWidth - halfPanelWidth - m_BorderWidth.y;
			float minY = halfPanelHeight + m_BorderWidth.w;
			float maxY = screenHeight - halfPanelHeight - m_BorderWidth.z;

			// 为箭头预留的高度也要计入可用边界。
			if (desc.Vertical)
			{
				if (panelSign > 0) { minY += arrowHeight; } else { maxY -= arrowHeight; }
			}
			else
			{
				if (panelSign > 0) { minX += arrowHeight; } else { maxX -= arrowHeight; }
			}

			offset.x = Mathf.Clamp(offset.x, minX, maxX);
			offset.y = Mathf.Clamp(offset.y, minY, maxY);

			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectCanvas, offset, m_UICamera, out Vector2 panelPos);
			m_Panel.position = m_RectCanvas.TransformPoint(panelPos);

			return offset;
		}

		private void UpdateArrowPosition(Vector2 screenPos, DirDesc desc, Vector2 panelScreenPos, Vector2 panelSize, Vector2 arrowSize)
		{
			if (m_Arrow == null)
			{
				return;
			}

			float halfArrowWidth = arrowSize.x * 0.5f;
			float arrowHeight = arrowSize.y;
			float halfPanelWidth = panelSize.x * 0.5f;
			float halfPanelHeight = panelSize.y * 0.5f;

			m_Arrow.localEulerAngles = new Vector3(0, 0, desc.Angle);

			Vector2 offset = screenPos;
			if (desc.Vertical)
			{
				// 主轴 y：紧贴面板边缘并预留箭头高度；交叉轴 x：夹在面板内。
				offset.y = panelScreenPos.y + desc.PointSign * (halfPanelHeight + arrowHeight);
				offset.x = Mathf.Clamp(
					screenPos.x,
					panelScreenPos.x - halfPanelWidth + halfArrowWidth,
					panelScreenPos.x + halfPanelWidth - halfArrowWidth);
			}
			else
			{
				offset.x = panelScreenPos.x + desc.PointSign * (halfPanelWidth + arrowHeight);
				offset.y = Mathf.Clamp(
					screenPos.y,
					panelScreenPos.y - halfPanelHeight + halfArrowWidth,
					panelScreenPos.y + halfPanelHeight - halfArrowWidth);
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectCanvas, offset, m_UICamera, out Vector2 arrowPos);
			m_Arrow.position = m_RectCanvas.TransformPoint(arrowPos);
		}

		private DirectionType CalcArrowDirection(Vector2 screenPos, float screenWidth, float screenHeight)
		{
			DirectionType dir = DirectionType.None;
			float arrowHeight = GetRectSize(m_Arrow).y;

			if (m_AlignType == AlignType.Horizontal)
			{
				if (screenPos.y < m_BorderWidth.w + arrowHeight)
				{
					// 箭头向下
					dir = DirectionType.Bottom;
				}
				else if (screenPos.y > screenHeight - m_BorderWidth.z - arrowHeight)
				{
					// 箭头向上
					dir = DirectionType.Top;
				}
				else
				{
					// 位于屏幕左半时箭头向左，否则向右
					dir = (screenPos.x >= 0 && screenPos.x < screenWidth * 0.5f)
						? DirectionType.Left
						: DirectionType.Right;
				}
			}
			else
			{
				if (screenPos.x < m_BorderWidth.x + arrowHeight)
				{
					// 箭头向左
					dir = DirectionType.Left;
				}
				else if (screenPos.x > screenWidth - m_BorderWidth.y - arrowHeight)
				{
					// 箭头向右
					dir = DirectionType.Right;
				}
				else
				{
					// 位于屏幕下半时箭头向下，否则向上
					dir = (screenPos.y >= 0 && screenPos.y < screenHeight * 0.5f)
						? DirectionType.Bottom
						: DirectionType.Top;
				}
			}

			return dir;
		}

		/// <summary>
		/// 按方向解析锚点偏移：
		/// 竖直方向 x 为交叉轴（屏幕水平）、y 沿指向；
		/// 水平方向 x 为交叉轴（屏幕竖直）、y 沿指向。
		/// </summary>
		private Vector2 ApplyAnchorOffset(Vector2 screenPos, DirDesc desc)
		{
			Vector2 resolved = desc.Vertical
				? new Vector2(m_AnchorOffset.x, desc.PointSign * m_AnchorOffset.y)
				: new Vector2(desc.PointSign * m_AnchorOffset.y, m_AnchorOffset.x);
			return screenPos + resolved;
		}

		private static Vector2 GetRectSize(RectTransform rectTransform)
		{
			return rectTransform == null ? Vector2.zero : rectTransform.rect.size;
		}
	}
}