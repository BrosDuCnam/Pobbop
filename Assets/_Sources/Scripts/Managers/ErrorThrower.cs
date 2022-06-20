using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorThrower : SingletonBehaviour<ErrorThrower>
{
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private Button _easterEggButton;
    [SerializeField] private Image _background;

    [SerializeField] private Color[] _colors;

    private int clickStreak = 0;
    private float _timeAtLastClick = 0;
    
    private Vector3 _originalTextPosition;
    
    private void Start()
    {
        // _background.gameObject.SetActive(false);
        // _errorText.gameObject.SetActive(false);
        // _easterEggButton.gameObject.SetActive(false);
        // _originalTextPosition = _errorText.transform.position;
        // _timeAtLastClick = Time.time;
    }
    
    public void ThrowError(string error, int gravity = 0)
    {
        // _background.gameObject.SetActive(true);
        // _errorText.gameObject.SetActive(true);
        // _easterEggButton.gameObject.SetActive(true);
        //
        // _background.color = _colors[gravity];
        // _errorText.text = error;
        //
        // _easterEggButton.onClick.AddListener(() =>
        // {
        //     if (Time.time - _timeAtLastClick > 1)
        //     {
        //         clickStreak = 0;
        //     }
        //     clickStreak++;
        //
        //     _errorText.GetComponent<RectTransform>().DOShakePosition(0.5f, Mathf.Pow(Mathf.Log(clickStreak), 5f) * 10f, 90, 90, false, true);
        //     _timeAtLastClick = Time.time;
        // });
    }

    private void Update()
    {
        // if (Time.time - _timeAtLastClick > 20)
        // {
        //     _errorText.transform.DOMove(_originalTextPosition, 0.5f);
        // }
    }
}