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
    [Header("Impact Effect")]
    [SerializeField] private Material impactMat;
    [SerializeField] private ComputeShader compute;
    [SerializeField] private RenderTexture map;
    [SerializeField] private RenderTexture diffusedMap;

    [SerializeField] private int numAgent;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lifeDecay;

    [SerializeField] private int textureSize;
    
    [SerializeField] private float decayRate;
    [SerializeField] private float diffuseRate;
    
    private ComputeBuffer agentBuffer;

    [Header("Collision Detection")] 
    [SerializeField] private int maxSimultaneousImpacts = 10;
    [SerializeField] private float initialSize = 5;

    private float shieldSize;
    private Vector3 shieldPos;
    
    private int impactHistoryIndex = 1;

    private void Awake()
    {
        shieldSize = initialSize * transform.localScale.x;
        shieldPos = transform.position;
    }


    private struct Agent
    {
        public Vector2 position;
        public float angle;
        public float health;
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
        Agent[] agents = new Agent[numAgent * maxSimultaneousImpacts];
        for (int i = 0; i < agents.Length; i++)
        {
            Vector2 center = new Vector2(textureSize / 2, textureSize / 2);
            Vector2 startPos = center;
            float randomAngle = Random.value * Mathf.PI * 2;

            agents[i] = new Agent{position = startPos, angle = randomAngle, health = 0};
        }
        
        //Send initial values to the compute shader

        ComputeHelper.CreateAndSetBuffer<Agent>(ref agentBuffer, agents, compute, "agents", 0);
        compute.SetBuffer(0, "agents", agentBuffer);
        compute.SetInt("numAgents", numAgent * maxSimultaneousImpacts);
        compute.SetInt("textureSize", textureSize);
        compute.SetFloat("moveSpeed", moveSpeed);
        compute.SetFloat("lifeDecay", lifeDecay);
        compute.SetFloat("deltaTime", 0);
        compute.SetFloat("time", 0);
        
        compute.SetFloat("decayRate", decayRate);
        compute.SetFloat("diffuseRate", diffuseRate);

        compute.SetTexture(0, "Result", map);
        
        compute.SetTexture(1, "Result", map);
        compute.SetTexture(1, "DiffusedMap", diffusedMap); 

        impactMat.SetTexture("ImpactParticles", map);
        
        newImpact();
        agentBuffer.SetData(CreateAgentsAtPos(new Vector2(0f,0f)), 0,
            impactHistoryIndex * numAgent, numAgent);
        compute.SetBuffer(0, "agents", agentBuffer);
    }

    private void Update()
    {
        compute.SetFloat("deltaTime", Time.deltaTime);
        compute.SetFloat("time", Time.time);
        ComputeHelper.Dispatch(compute, numAgent * maxSimultaneousImpacts, 1, 1, 0);
        ComputeHelper.Dispatch(compute, textureSize, textureSize, 1, 1);
        ComputeHelper.CopyRenderTexture(diffusedMap, map);
        
    }
    
    void OnDestroy()
    {
        ComputeHelper.Release(agentBuffer);
    }

    private Agent[] CreateAgentsAtPos(Vector2 pos)
    {
        Agent[] agents = new Agent[numAgent];
        for (int i = 0; i < agents.Length; i++)
        {
            Vector2 center = new Vector2(textureSize / 2, textureSize / 2);
            Vector2 startPos = center + pos * textureSize / 2;
            float randomAngle = Random.value * Mathf.PI * 2;

            agents[i] = new Agent{position = startPos, angle = randomAngle, health = 1f};
        }

        return agents;
    }

    private void newImpact()
    {
        if (impactHistoryIndex > maxSimultaneousImpacts - 2)
        {
            impactHistoryIndex = 1;
        }
        impactHistoryIndex += 1;
    }
    
    private void OnCollisionEnter(Collision col)
    {
        Vector3 impactPos = col.contacts[0].point;
        impactPos -= shieldPos; 
        Vector2 uvPos = new Vector2(-impactPos.x, -impactPos.z) / shieldSize * 2;
        
        Debug.Log(uvPos);
        newImpact();
        agentBuffer.SetData(CreateAgentsAtPos(uvPos), 0,
            impactHistoryIndex * numAgent, numAgent);
        compute.SetBuffer(0, "agents", agentBuffer);
    }    
}
