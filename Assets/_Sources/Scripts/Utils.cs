using UnityEngine;

public static class Utils
{
    public static bool IsVisibleByCamera(GameObject obj, Camera cam)
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(obj.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        return onScreen;
    }
    
    public static float GetDistanceFromCenterOfScreen(GameObject obj, Camera cam)
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(obj.transform.position);
        float distance = Mathf.Abs(screenPoint.x - 0.5f) + Mathf.Abs(screenPoint.y - 0.5f);
        return distance;
    }
    
    /// <summary>
    /// Function to get travel distance of a bezier curve
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="step"></param>
    /// <param name="end"></param>
    /// <param name="interation"></param>
    /// <returns></returns>
    public static float BezierCurveDistance(Vector3 origin, Vector3 step, Vector3 end, float interation)
    {
        Vector3 previousSample = origin;
        float distance = 0;
        for (int i=1; i<interation; i++) {
            Vector3 sample = BezierCurve(origin, step, end, i / interation);
            distance += Vector3.Distance(sample, previousSample);
            previousSample = sample;
        }

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
    
    public static void DebugBezierCurve(Vector3 origin, Vector3 step, Vector3 end, float interation, Color color, float time = 0f)
    {
        Vector3 previousSample = origin;
        for (int i=1; i<interation; i++) {
            Vector3 sample = BezierCurve(origin, step, end, i / interation);
            Debug.DrawLine(previousSample, sample, color, time);
            previousSample = sample;
        }
    }
}
