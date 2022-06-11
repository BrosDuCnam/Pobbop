
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoryManager : SingletonBehaviour<StoryManager>
{
    public Player player;

    [SerializeField] private float _thowerRotationSpeed;
    [SerializeField] private float _thowerCooldown;
    [SerializeField] private float _minRandomTimeToSpeech = 10f;
    [SerializeField] private float _maxRandomTimeToSpeech = 30f;
    private float _thowerCooldownTimer; 
    private float _randomTimeToSpeech;
    private float _randomTimer;

    private float _startDelay = 5f;
    private float _startDelayTimer;
    
    public enum StoryState
    {
        None = 0,
        Spawn = 1,
        Sprint = 2,
        Shoot = 3,
        Slide = 4,
        Catch = 5,
        Pass = 6,
        End = 7,
    }
    public StoryState state;
    
    [Serializable]
    public class DialogData
    {
        public string defaultText;
        [SerializeField] public List<string> defaultTexts = new List<string>();
        [SerializeField] public List<string> randomTexts = new List<string>();
        private List<string> saidTexts = new List<string>();
        
        public string GetRandomText()
        {
            if (randomTexts.Count == 0)
            {
                return "";
            }
            else
            {
                int index = Random.Range(0, randomTexts.Count);
                saidTexts.Add(randomTexts[index]);
                randomTexts.RemoveAt(index);
                return randomTexts[index];
            }
        }
    }
    public DialogData[] dialogs;
    [SerializeField] private ColliderTriggerHandler _spawnCollider;
    [SerializeField] private ColliderTriggerHandler _catchCollider;
    [SerializeField] private Throw _throw;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] public List<TutorialTarget> targets = new List<TutorialTarget>();
    
    [SerializeField] private TextMeshProUGUI _text;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindObjectsOfType<Player>().FirstOrDefault();
            if (player == null) return;
        }
        
        NextState();
        NetworkClient.RegisterPrefab(_ballPrefab);
        
        player._pickup.OnCatch.AddListener(() =>
        {
            if (state == StoryState.Catch)
            {
                NextState();
            }
        });
        
        player._throw.OnPass.AddListener(() =>
        {
            if (state == StoryState.Pass)
            {
                NextState();
            }
        });
        
        _catchCollider.OnTriggerEnterEvent.AddListener((collision) =>
        {
            if (_thowerCooldownTimer > 0) return;
            
            _thowerCooldownTimer = _thowerCooldown;
            
            Ball ball = Instantiate(_ballPrefab).GetComponent<Ball>();
            NetworkServer.Spawn(ball.gameObject);
            ball.transform.position = _throw.transform.position;
            _throw.ball = ball.transform;

            _throw.ReleaseThrow(false, .1f, 1, player.gameObject);
        });
        
        _spawnCollider.OnTriggerExitEvent.AddListener((collision) =>
        {
            if (state == StoryState.Spawn)
            {
                NextState();
            }
        });
    }

    private void Update()
    {
        _throw.IsCharging = true;
        
        //Rotate the thrower
        _throw.transform.Rotate(0, _thowerRotationSpeed * Time.deltaTime, 0);
        
        if (player == null)
        {
            player = GameObject.FindObjectsOfType<Player>().FirstOrDefault();
            if (player == null) return;
            Start();
        }
        
        if (_thowerCooldownTimer > 0)
        {
            _thowerCooldownTimer -= Time.deltaTime;
        }

        // randomTimer += Time.deltaTime;
        // if (randomTimer > randomTimeToSpeech)
        // {
        //     randomTimer = 0;
        //     randomTimeToSpeech = Random.Range(minRandomTimeToSpeech, maxRandomTimeToSpeech);
        //     Dialogue(dialogs[(int) state].GetRandomText());
        // }
        
        else if (state == StoryState.Sprint)
        {
            // If the player is sprinting, go to the next state
            if (player._controller.run)
            {
                NextState();
            }
        }
        else if (state == StoryState.Shoot)
        {
            // If the player shoot all the targets, go to the next state
            if (!targets.Any(t => t.gameObject.activeSelf))
            {
                NextState();
            }
        }
        else if (state == StoryState.Slide)
        {
            // If the player is sliding, go to the next state
            if (player._controller.crouch)
            {
                NextState();
            }
        }
        // Catch state is done by event
        
        // Pass state is done by event
    }

    private void Dialogue(string text)
    {
        StopAllCoroutines();
        StartCoroutine(DialogueCoroutine(new List<string>() {text}));
    }
    private void Dialogue(List<string> texts)
    {
        StopAllCoroutines();
        StartCoroutine(DialogueCoroutine(texts));
    }
    
    
    public IEnumerator DialogueCoroutine(List<string> text)
    {
        while (text.Count > 0)
        {
            if (string.IsNullOrEmpty(text[0])) continue;
            StartCoroutine(TextRevealer.RevealText(_text, text[0], .2f, true));
            text.RemoveAt(0);
            yield return new WaitForSeconds(10f);
        }

        StartCoroutine(TextRevealer.RevealText(_text, dialogs[(int) state - 1].defaultText, .2f, true));
    }
    
    private void NextState()
    {
        state += 1;

        Dialogue(dialogs[(int) state -1].defaultTexts);
        
        print("Next state: " + state);
    }
}
