using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camWTVtest : MonoBehaviour
{
    [SerializeField] private bool stop;
    
    [SerializeField] private GameObject point;
    [SerializeField] private float xRange = 90;
    [SerializeField] private float yRange = 800;
    [SerializeField] private int elements = 100;
    [SerializeField] private float size = 5;

    [SerializeField] private Transform ball;

    private List<Vector2> vPos = new List<Vector2>();

    
    private void Update()
    {
        bool draw = false;
        if (!stop)
        {
            Vector3 vPos3 = Camera.main.WorldToViewportPoint(ball.position, Camera.MonoOrStereoscopicEye.Mono);
            Camera.main.transform.Rotate(0, Time.deltaTime * 0.5f, 0);
            if (Camera.main.transform.rotation.y != 0)
                vPos.Add(new Vector2(Camera.main.transform.rotation.eulerAngles.y, vPos3.y / (1 / Camera.main.transform.rotation.eulerAngles.y + 1)));
        }
        else
        {
            draw = true;
            stop = false;
        }

        if (draw)
        {
            int count = vPos.Count;
            int offset = count / elements;
            Debug.Log("offset : " + offset + " count : " + count);
            for (int i = 0; i <= count; i+= offset)
            {
                GameObject go = Instantiate(point, new Vector3(vPos[i].x / xRange, vPos[i].y / yRange, 1), 
                    Quaternion.Euler(0, 0, 0));
                go.name = vPos[i].x + " ; " + vPos[i].y;
            }


            draw = false;
        }
    }
}
