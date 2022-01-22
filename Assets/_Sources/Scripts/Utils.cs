using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Function ro know if a gameobject is in the camera view
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="cam">The camera</param>
    /// <returns>True if the object is in camera view</returns>
    public static bool IsVisibleByCamera(GameObject obj, Camera cam)
    {
        return IsVisibleByCamera(obj.transform.position, cam);
    }
    
    /// <summary>
    /// Function ro know if a position is in the camera view
    /// </summary>
    /// <param name="position">The position</param>
    /// <param name="cam">The camera</param>
    /// <returns>True if the position is in camera view</returns>
    public static bool IsVisibleByCamera(Vector3 position, Camera cam)
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        return onScreen;
    }
    
    /// <summary>
    /// Function to get the distance between object and camera center
    /// </summary>
    /// <param name="obj">The object</param>
    /// <param name="cam">The camera</param>
    /// <returns>The disctance to the center of camera</returns>
    public static float GetDistanceFromCenterOfScreen(GameObject obj, Camera cam)
    {
        return GetDistanceFromCenterOfScreen(obj.transform.position, cam);
    }
    
    /// <summary>
    /// Function to get the distance between object and camera center
    /// </summary>
    /// <param name="position">The position</param>
    /// <param name="cam">The camera</param>
    /// <returns>The disctance to the center of camera</returns>
    public static float GetDistanceFromCenterOfScreen(Vector3 position, Camera cam)
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(position);
        float distance = Mathf.Abs(screenPoint.x - 0.5f) + Mathf.Abs(screenPoint.y - 0.5f);
        return distance;
    }
    
    /// <summary>
    /// Function to get the position of point in bezier curve
    /// </summary>
    /// <param name="origin">Origin</param>
    /// <param name="step">Step</param>
    /// <param name="end">End</param>
    /// <param name="t">Time between 0 and 1</param>
    /// <returns>The position of point in bezier curve</returns>
    public static Vector3 BezierCurve(Vector3 origin, Vector3 step, Vector3 end, float t)
    {
        float u = (1 - t);
        float uu = u * u;
        float tt = t * t;
        Vector3 Q0 = uu * origin + 2 * (1 - t) * t * step + tt * end;
        return Q0;
    }
    
    public static Vector3[] GetBezierCurvePositions(Vector3 origin, Vector3 step, Vector3 end, int interation)
    {
        Vector3[] points = new Vector3[interation];
        for (int i=0; i<interation; i++) {
            points[i] = BezierCurve(origin, step, end, i / (float)interation);
        }
        return points;
    }
    
    /// <summary>
    /// Function to get travel distance of a bezier curve
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="step"></param>
    /// <param name="end"></param>
    /// <param name="interation"></param>
    /// <returns>The length of the curve in meter</returns>
    public static float BezierCurveDistance(Vector3 origin, Vector3 step, Vector3 end, float interation)
    {
        Vector3[] points = GetBezierCurvePositions(origin, step, end, (int)interation);
        float distance = 0;
        for (int i=0; i<points.Length-1; i++) {
            distance += Vector3.Distance(points[i], points[i+1]);
        }
        return distance;
    }
    
    /// <summary>
    /// Function to draw a bezier curve with debug lines
    /// </summary>
    /// <param name="origin">Origin</param>
    /// <param name="step">Step</param>
    /// <param name="end">End</param>
    /// <param name="interation">The number of iteration, the more there are, the more precise it will be</param>
    /// <param name="color">The color of the line</param>
    /// <param name="time">The duration of the line</param>
    public static void DebugBezierCurve(Vector3 origin, Vector3 step, Vector3 end, float interation, Color color, float time = 0f)
    {
        
        Vector3[] points = GetBezierCurvePositions(origin, step, end, (int)interation);
        for (int i=0; i<points.Length-1; i++) {
            Debug.DrawLine(points[i], points[i+1], color, time);
        }
    }
}
