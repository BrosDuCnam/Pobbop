
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

    [SerializeField] private float minRandomTimeToSpeech = 10f;
    [SerializeField] private float maxRandomTimeToSpeech = 30f;
    private float randomTimeToSpeech;
    private float randomTimer;
    
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
    [SerializeField] private Collider _spawnCollider;
    [SerializeField] private ColliderTriggerHandler _catchCollider;
    [SerializeField] private Throw _throw;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] public List<TutorialTarget> targets = new List<TutorialTarget>();
    
    [SerializeField] private TextMeshProUGUI _text;

    private void Start()
    {
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
        
        _catchCollider.OnCollisionEnterEvent.AddListener((collision) =>
        {
            Ball ball = Instantiate(_ballPrefab).GetComponent<Ball>();
            NetworkServer.Spawn(ball.gameObject);
            ball.transform.position = _throw.transform.position;
            _throw.ball = ball.transform;

            _throw.ReleaseThrow(false, 1, 1, player.gameObject);
        });
    }

    private void Update()
    {
        randomTimer += Time.deltaTime;
        if (randomTimer > randomTimeToSpeech)
        {
            randomTimer = 0;
            randomTimeToSpeech = Random.Range(minRandomTimeToSpeech, maxRandomTimeToSpeech);
            Dialogue(dialogs[(int) state].GetRandomText());
        }

        if (state == StoryState.Spawn)
        {
            // If the player is not in the spawn collider, go to the next state
            if (!_spawnCollider.bounds.Contains(player.transform.position))
            {
                NextState();
            }
        }
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
        StartCoroutine(DialogueCoroutine(new List<string>() {text}));
    }
    private void Dialogue(List<string> texts)
    {
        StartCoroutine(DialogueCoroutine(texts));
    }
    
    
    public IEnumerator DialogueCoroutine(List<string> text)
    {
        // TODO: better animation
        while (text.Count > 0)
        {
            if (string.IsNullOrEmpty(text[0])) continue;
            _text.text = text[0];
            text.RemoveAt(0);
            yield return new WaitForSeconds(10f);
        }

        _text.text = dialogs[(int) state].defaultText;
    }
    
    private void NextState()
    {
        state += 1;

        Dialogue(dialogs[(int) state].defaultTexts);
    }
}
