using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public static class Utils
{
    
    public static List<GameObject> GetAllVisibleObject(List<GameObject> targets, Camera cam, Func<RaycastHit, bool> condition)
    {
        List<GameObject> visibleTargets = new List<GameObject>();
        foreach (GameObject target in targets)
        {
            if (target == null) continue;
            // Check if target is in field of view of player camera
            if (Utils.IsVisibleByCamera(target, cam))
            {
                // If object obstructs the view of the player, it is not visible
                RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, target.transform.position - cam.transform.position);

                hits = hits.Where(x => condition.Invoke(x)).ToArray();
                hits = hits.OrderBy(x => x.distance).ToArray();

                if (hits.Length > 0 && 
                    (hits[0].collider.gameObject == target || hits[0].transform.root.gameObject == target || hits[0].transform.root.GetComponentsInChildren<Transform>().Contains(target.transform)))
                {
                    //Debug.DrawLine(cam.transform.position, target.transform.position, Color.green);
                    visibleTargets.Add(target);
                    continue;
                }
            }
            //Debug.DrawLine(cam.transform.position, target.transform.position, Color.red);
        }
        return visibleTargets;
    }
    
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
    
    /// <summary>
    /// Get random position on navmesh
    /// </summary>
    /// <param name="radius">Radius around origin</param>
    /// <param name="origin">The center</param>
    /// <returns>The final position</returns>
    public static Vector3 RandomNavmeshLocation(float radius, Vector3 origin)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += origin;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
            finalPosition = hit.position;            
        }
        return finalPosition;
    }
    
    
    /// <summary>
    /// Function do draw a navmesh path
    /// </summary>
    /// <param name="path">The path that we want to debug</param>
    /// <param name="time">The time that the debug stay</param>
    public static void DebugNavMeshPath(NavMeshPath path, float time = 0f)
    {
        DebugNavMeshPath(path, Color.red, time);
    }
    
    /// <summary>
    /// Function do draw a navmesh path
    /// </summary>
    /// <param name="path">The path that we want to debug</param>
    /// <param name="color">The color of the debug</param>
    /// <param name="time">The time that the debug stay</param>
    public static void DebugNavMeshPath(NavMeshPath path, Color color, float time = 0f)
    {
        if (path.corners.Length > 0) {
            for (int i=0; i<path.corners.Length-1; i++) {
                Debug.DrawLine(path.corners[i], path.corners[i+1], color, time);
            }
        }
    }
    
    /// <summary>
    /// Radian vector to degree
    /// </summary>
    /// <param name="radian">The vector of cos and sin value</param>
    /// <returns>The degree value</returns>
    public static float RadianToDegree(Vector2 radian)
    {
        //Debug.Log(radian);
        return Mathf.Atan2(radian.y, radian.x) * Mathf.Rad2Deg;
    }

    
    /// <summary>
    /// Radian to vector 2
    /// </summary>
    /// <param name="radian">Radian</param>
    /// <returns>Cos and sin of radian</returns>
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    
    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    /// <summary>
    /// Function to remap a number by minimun and maximum
    /// </summary>
    /// <param name="oldMax"></param>
    /// <param name="oldMin"></param>
    /// <param name="newMax"></param>
    /// <param name="newMin"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public static float Map(float oldMax, float oldMin, float newMax, float newMin, float number)
    {
        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        return (((number - oldMin) * newRange) / oldRange) + newMin;
    }

    /// <summary>
    /// Function reformat degree to -180/180
    /// </summary>
    /// <param name="degree"></param>
    /// <returns></returns>
    public static float DegreeFormat360To180(float degree)
    {
        if (degree > 180)
        {
            return Map(360, 180, 0, -180, degree);
        }
        return degree;
    }
    
    /// <summary>
    /// Function reformat degree to 0/360
    /// </summary>
    /// <param name="degree"></param>
    /// <returns></returns>
    public static float DegreeFormat180To360(float degree)
    {
        if (degree < 0)
        {
            return Map(0, -180, 360, 180, degree);
        }
        return degree;
    }
    
    
    /// <summary>
    /// Extension of Array to remove element at index
    /// </summary>
    /// <param name="source"></param>
    /// <param name="index">Index of element to remove</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] RemoveAt<T>(T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if( index > 0 )
            Array.Copy(source, 0, dest, 0, index);

        if( index < source.Length - 1 )
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static float NavMeshDistance(Vector3 origin, Vector3 destination, int navMesh = NavMesh.AllAreas)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(origin, destination, navMesh, path);

        float result = 0;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
            result += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return result;
    }
    
    /// <summary>
    /// Coroutine to make the bot a timed action
    /// </summary>
    /// <param name="time">The time to callback true</param>
    /// <param name="callback">The callback to call</param>
    public static IEnumerator TimedAction(float time, Action<bool> callback)
    {
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            callback(true);
            yield return new WaitForEndOfFrame();
        }
        callback(false);
    }
}
