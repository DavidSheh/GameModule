using System;
using UnityEngine;

/// <summary>
/// Rect Transform Utility methods.
/// </summary>
public static class RectTransformUtility
{
    /// <summary>
    /// Method to get Rect related to ScreenSpace, from given RectTransform.
    /// This will give the real position of this Rect on screen.
    /// </summary>
    /// <param name="transform">Original RectTransform of some object</param>
    /// <returns>New Rect instance.</returns>
    public static Rect RectTransformToScreenSpace(RectTransform transform) 
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }

    public static Rect GetScreenCoordinates(RectTransform uiElement)
    {
        var worldCorners = new Vector3[4];
        uiElement.GetWorldCorners(worldCorners);
        var result = new Rect(
                        worldCorners[0].x,
                        worldCorners[0].y,
                        worldCorners[2].x - worldCorners[0].x,
                        worldCorners[2].y - worldCorners[0].y);
        return result;
    }

    /// <summary>
    /// Method to get anchored position from world position
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="camera"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Vector2 WorldToAnchoredPosition(RectTransform canvas, Camera camera, Vector3 position)
    {
        Vector2 pos = camera.WorldToViewportPoint(position);

        //Calculate position considering our percentage, using our canvas size
        //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
        pos.x *= canvas.sizeDelta.x;
        pos.y *= canvas.sizeDelta.y;

        //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
        //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
        //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
        //returned value will still be correct.

        pos.x -= canvas.sizeDelta.x * canvas.pivot.x;
        pos.y -= canvas.sizeDelta.y * canvas.pivot.y;

        return pos;
    }
}