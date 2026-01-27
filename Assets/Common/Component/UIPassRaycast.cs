
using UnityEngine;
using XLua;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectDHLD
{
    public class UIPassRaycast : UIEmptyRaycast, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler,
                               IDragHandler, IEndDragHandler, IScrollHandler
    {
        private List<RaycastResult> results;

        // Pinch
        private int m_EffectiveFingerCount = 0; // 起作用的手指数量  -1：操作手指数减少的情况，0/1/2：手指数量
        private int m_DownAndUpCount = 0;
        private int m_DraggingCount = 0; // 必须是拖拽之前手指数量先达到2才算pinch
        private int[] m_PointerId = new int[2];
        private Vector2[] m_PointetPos = new Vector2[2];
        private float m_LastTwoFingerDistance = 0;

        // LuaFunction CallBack
        public LuaFunction ClickCallback;
        public LuaFunction BeginDragCallback;
        public LuaFunction DragCallback;
        public LuaFunction EndDragCallback;
        public LuaFunction PinchCallback;

        private ScrollRect m_ScrollRect;
        private Selectable m_Selectable;
        private Graphic m_Graphic;

        public bool IgnoreDrag = false;
        public bool UseDefaultClick = false;

        public bool PassEventPriority;
        
        // 用于判断pinch和click情况
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_DownAndUpCount < 2)
            {
                m_PointerId[m_DownAndUpCount] = eventData.pointerId;
                m_PointetPos[m_DownAndUpCount] = eventData.position;
            }

            m_DownAndUpCount++;

            // 先把事件渗透下去
            m_Selectable = (Selectable)GetFirstComponent<Selectable>(eventData);

            if (m_Selectable != null)
            {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            }

            // 没有发生多指操作手指变少的情况，而且还未发生拖拽情况
            if (m_EffectiveFingerCount >= 0 && m_DraggingCount == 0)
            {
                m_EffectiveFingerCount = m_DownAndUpCount;
            }
        }

        // 用于判断pinch和click情况
        public void OnPointerUp(PointerEventData eventData)
        {
            m_DownAndUpCount--;

            // 先把事件渗透下去
            if (PassEventPriority && m_Selectable != null)
            {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                m_Selectable = null;
            }

            if (m_DownAndUpCount == 0)
            {
                // 手指全部放开了
                m_EffectiveFingerCount = 0;

                // 没有发生过拖拽，触发点击事件，这里就不区分点击双击和按住了，统一按点击处理，有需求再分开
                if (m_DraggingCount == 0 && !UseDefaultClick)
                {
                    ExecuteClick(eventData);
                }
            }
            else if (m_DownAndUpCount > 0)
            {
                // 多指操作下手指减少了，取消pinch，并且在所有手指放开前，再增加手指都不能算pinch
                m_EffectiveFingerCount = -1;
            }
            else
            {
                // 异常情况，正常这个值不会小于0
                m_DownAndUpCount = 0;
            }

            // 然后把事件渗透下去
            if (!PassEventPriority && m_Selectable != null)
            {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerUpHandler);
                m_Selectable = null;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 发生拖拽点击事件仍然触发，所以不能用这个事件，需要自己实现
            // 引导那里用自动逸的老是出错，手指按下数量总是不对，先用默认的
            if (UseDefaultClick)
            {
                ExecuteClick(eventData);
            }
        }

        // 正常来说应该是先把事件传回Lua，再执行下边seletable的事件
        // 因为顶层界面在Mask界面下边，正常应该先执行顶层界面的事件，但是先传回Lua，再执行顶层界面selectable事件顺序就错了
        // 为了解决上边的问题，统一改为先执行下边seletable的事件，如果有问题，需要添加一套事件，既：seletable前后各一套单独事件
        private void ExecuteClick(PointerEventData eventData)
        {
            Selectable select = (Selectable)GetFirstComponent<Selectable>(eventData);
            
            // 再先把点击事件传回Lua
            if (!PassEventPriority && ClickCallback != null)
            {
                ClickCallback.Call(eventData, select);
            }

            // 先把点击事件渗透下去
            if (select != null)
            {
                ExecuteEvents.Execute(select.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }

            // 再先把点击事件传回Lua
            if (PassEventPriority && ClickCallback != null)
            {
                ClickCallback.Call(eventData, select);
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            m_ScrollRect = (ScrollRect)GetFirstComponent<ScrollRect>(eventData);

            if (m_ScrollRect != null)
            {
                ExecuteEvents.Execute(m_ScrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_DraggingCount++;

            if (IgnoreDrag)
            {
                return;
            }

            if (!PassEventPriority && BeginDragCallback != null)
            {
                BeginDragCallback.Call(eventData, m_Graphic, m_ScrollRect);
            }
            
            m_Graphic = (Graphic)GetFirstComponent<Graphic>(eventData);

            if (m_Graphic != null)
            {
                ExecuteEvents.Execute(m_Graphic.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }

            m_ScrollRect = (ScrollRect)GetFirstComponent<ScrollRect>(eventData);

            if (m_ScrollRect != null)
            {
                ExecuteEvents.Execute(m_ScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }

            if (PassEventPriority && BeginDragCallback != null)
            {
                BeginDragCallback.Call(eventData, m_Graphic, m_ScrollRect);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IgnoreDrag)
            {
                return;
            }
            
            if (m_EffectiveFingerCount == 1)
            {
                // 先把拖拽事件传回Lua
                if (!PassEventPriority && DragCallback != null)
                {
                    DragCallback.Call(eventData, m_Graphic, m_ScrollRect);
                }
                
                // 再把拖拽事件渗透下去
                if (m_Graphic != null)
                {
                    ExecuteEvents.Execute(m_Graphic.gameObject, eventData, ExecuteEvents.dragHandler);
                }

                if (m_ScrollRect != null)
                {
                    ExecuteEvents.Execute(m_ScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
                }
                
                // 先把拖拽事件传回Lua
                if (PassEventPriority && DragCallback != null)
                {
                    DragCallback.Call(eventData, m_Graphic, m_ScrollRect);
                }
            }
            else if (m_EffectiveFingerCount == 2)
            {
                // 没有相应pinch的UI组件，所以pinch不需要渗透下去，直接传回Lua
                if (PinchCallback == null)
                {
                    return;
                }

                if (m_PointerId[0] == eventData.pointerId)
                {
                    m_PointetPos[0] = eventData.position;
                }
                else if (m_PointerId[1] == eventData.pointerId)
                {
                    m_PointetPos[1] = eventData.position;
                }
                else
                {
                    return;
                }

                float distance = Vector2.Distance(m_PointetPos[0], m_PointetPos[1]);
                Vector2 center = new Vector2((m_PointetPos[0].x + m_PointetPos[1].x) / 2f, (m_PointetPos[0].y + m_PointetPos[1].y) / 2);

                PinchCallback.Call(eventData, m_LastTwoFingerDistance - distance, center);
                m_LastTwoFingerDistance = distance;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_DraggingCount--;

            if (IgnoreDrag)
            {
                return;
            }

            if (!PassEventPriority && EndDragCallback != null)
            {
                EndDragCallback.Call(eventData, m_Graphic, m_ScrollRect);
            }
            
            // 再把拖拽事件渗透下去
            if (m_Graphic != null)
            {
                ExecuteEvents.Execute(m_Graphic.gameObject, eventData, ExecuteEvents.endDragHandler);
            }

            if (m_ScrollRect != null)
            {
                ExecuteEvents.Execute(m_ScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
            }

            if (PassEventPriority && EndDragCallback != null)
            {
                EndDragCallback.Call(eventData, m_Graphic, m_ScrollRect);
            }

            m_ScrollRect = null;
            m_Graphic = null;
        }

        public void OnScroll(PointerEventData eventData)
        {
#if UNITY_EDITOR
            if (!PassEventPriority && PinchCallback != null)
            {
                // 抛事件给Lua
                PinchCallback.Call(eventData, eventData.scrollDelta.y * 100, Input.mousePosition);
            }
            
            ScrollRect sr = (ScrollRect)GetFirstComponent<ScrollRect>(eventData);

            if (sr != null)
            {
                ExecuteEvents.Execute(sr.gameObject, eventData, ExecuteEvents.scrollHandler);
            }

            if (PassEventPriority && PinchCallback != null)
            {
                // 抛事件给Lua
                PinchCallback.Call(eventData, eventData.scrollDelta.y * 100, Input.mousePosition);
            }
#endif
        }

        private object GetFirstComponent<T>(PointerEventData eventData) where T : class
        {
            results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 1)
            {
                //执行下层第一个事件
                return results[1].gameObject.GetComponentInParent<T>();
            }

            return null;
        }

        public void Reset()
        {
            m_EffectiveFingerCount = 0;
            m_DownAndUpCount = 0;
            m_DraggingCount = 0;
            m_PointerId = new int[2];
            m_PointetPos = new Vector2[2];
            m_LastTwoFingerDistance = 0;

            m_ScrollRect = null;
            m_Selectable = null;
        }
    }
}
