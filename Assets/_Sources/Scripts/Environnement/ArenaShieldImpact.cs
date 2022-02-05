using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;
using ComputeShaderUtility;



public class ArenaShieldImpact : MonoBehaviour
{

    [SerializeField] private Material impactMat;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private RenderTexture map;
    [SerializeField] private RenderTexture diffusedMap;

    [SerializeField] private int numAgent;
    [SerializeField] private float moveSpeed;

    [SerializeField] private int textureSize;
    
    [SerializeField] private float decayRate;
    [SerializeField] private float diffuseRate;
    
    
    ComputeBuffer agentBuffer;
    
    private struct Agent
    {
        public Vector2 position;
        public float angle;
    }
    

    
    private void Start()
    {
        map = new RenderTexture(textureSize, textureSize, 24);
        diffusedMap = new RenderTexture(textureSize, textureSize, 24);
        map.enableRandomWrite = true;
        diffusedMap.enableRandomWrite = true;
        map.Create();
        diffusedMap.Create();

        //Init agents
        Agent[] agents = new Agent[numAgent];
        for (int i = 0; i < agents.Length; i++)
        {
            Vector2 center = new Vector2(textureSize / 2, textureSize / 2);
            Vector2 startPos = center;
            float randomAngle = Random.value * Mathf.PI * 2;

            agents[i] = new Agent{position = startPos, angle = randomAngle};
        }
        
        //Send initial values to the compute shader

        ComputeHelper.CreateAndSetBuffer<Agent>(ref agentBuffer, agents, compute, "agents", 0);
        compute.SetBuffer(0, "agents", agentBuffer);
        compute.SetInt("numAgents", numAgent);
        compute.SetInt("textureSize", textureSize);
        compute.SetFloat("moveSpeed", moveSpeed);
        compute.SetFloat("deltaTime", 0);
        compute.SetFloat("time", 0);
        compute.SetFloat("decayRate", decayRate);
        compute.SetFloat("diffuseRate", diffuseRate);
        
        
        compute.SetTexture(0, "Result", map);
        compute.SetTexture(1, "DiffusedMap", diffusedMap); 
        
        
        impactMat.SetTexture("_BaseMap", map);
    }

    private void Update()
    {
        compute.SetFloat("deltaTime", Time.deltaTime);
        compute.SetFloat("time", Time.time);
        ComputeHelper.Dispatch(compute, numAgent, 1, 1, 0);
        //ComputeHelper.Dispatch(compute, textureSize, textureSize, 1, compute.FindKernel("Diffuse"));
        //ComputeHelper.CopyRenderTexture(diffusedMap, map);
    }
}
