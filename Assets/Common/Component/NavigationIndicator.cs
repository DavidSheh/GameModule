using System;
using UnityEngine;

public class NavigationIndicator : MonoBehaviour
{
    public float rotationSpeed = 2f;
    private Camera cam;
    private Transform cacheTrans;
    private Vector3 targetPos;
    private Vector3 center;

    private Transform transCam;
    private Vector3 lastPos = Vector3.zero;

    void Start()
    {
        cacheTrans = transform;
        //cam = Camera.main;
        // 屏幕中心点
        center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    }
    
    public void SetCamera(Camera cam)
    {
        this.cam = cam;
        this.transCam = cam.transform;
    }

    public void SetTarget(Vector3 targetPos)
    {
        this.targetPos = targetPos;
    }

    public bool CheckInScreen()
    {
        if (null == cam)
        {
            return false;
        }
        var screenPoint = cam.WorldToScreenPoint(targetPos);
        if (screenPoint.x >= 0 && screenPoint.x <= Screen.width
            && screenPoint.y >= 0 && screenPoint.y <= Screen.height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void LateUpdate()
    {
        var curPos = this.transCam.position;
        if (null == cam || lastPos == curPos)
        {
            return;
        }
        lastPos = curPos;
        var pos = cam.WorldToScreenPoint(targetPos);
        pos.z = 0;
        var dir = pos - center;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        cacheTrans.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}