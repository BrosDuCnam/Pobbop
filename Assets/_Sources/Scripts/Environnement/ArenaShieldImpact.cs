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

    [SerializeField] private bool testAgents;
    
    private ComputeBuffer agentBuffer;

    [Header("Collision Detection")] 
    [SerializeField] private int maxSimultaneousImpacts = 10;
    [SerializeField] private float initialSize = 5;
    
    //Values to calculate impact on the shield
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

        //Init agents at the center of the texture with a random angle
        //It creates the max amount of particle at a time because the compute buffer needs to be initialized with an initial size
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
    }

    private void Update()
    {
        compute.SetFloat("deltaTime", Time.deltaTime);
        compute.SetFloat("time", Time.time);
        ComputeHelper.Dispatch(compute, numAgent * maxSimultaneousImpacts, 1, 1, 0);
        ComputeHelper.Dispatch(compute, textureSize, textureSize, 1, 1);
        //The blur and diffuse effect on the trails is processed on a different maps then copied on the initial map 
        ComputeHelper.CopyRenderTexture(diffusedMap, map);
        
    }
    
    void OnDestroy()
    {
        ComputeHelper.Release(agentBuffer);
    }

    private void OnCollisionEnter(Collision col)
    {
        //TODO : find a function to make the length of the return value linear  
        Vector3 impactPos = col.contacts[0].point;
        impactPos -= shieldPos; 
        Vector2 uvPos = new Vector2(-impactPos.x, -impactPos.z) / shieldSize * 2;
        
        Debug.Log(uvPos);
        newImpact();
        //Generate a new spray of particles at the impact point
        agentBuffer.SetData(CreateAgentsAtPos(uvPos), 0,
            impactHistoryIndex * numAgent, numAgent);
        compute.SetBuffer(0, "agents", agentBuffer);
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

    /// <summary>
    /// Loop in the agent buffer to have multiple impact trails at the same time
    /// and override the oldest when it reaches the max value of particles
    /// </summary>
    private void newImpact()
    {
        if (impactHistoryIndex > maxSimultaneousImpacts - 2)
        {
            impactHistoryIndex = 1;
        }
        impactHistoryIndex += 1;
    }
}
