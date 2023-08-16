using System.Collections;
using TMPro;
using UnityEngine;

public class TextBoxSettings : MonoBehaviour
{
    [SerializeField] private RectTransform _backgroundRectTransform;
    [SerializeField] public TMP_Text _textMeshPro;
    [SerializeField] private int _minWidth = 500;
    [SerializeField] private int _minHeight = 500;

    [Header("Tails")] 
    [SerializeField] private GameObject _talkTail;

    private RectTransform _tailRect;
    private RectTransform _rectTransform;
    private Vector2 _screenSize;
    private float _screenFactor;
    
    private string _text;
    private float _time;
    [HideInInspector] public float _scale = 0.1f;

    private bool _killing;
    
    private Transform _parentPos;
    private SpriteRenderer _spriteRenderer;
    private Camera _cam;

    public bool _forceBottom;
    
    public Transform ParentPos { get { return _parentPos; } set { _parentPos = value; } }
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } set { _spriteRenderer = value; } }

    public void Open()
    {
        // transform.position = Vector3.zero;
        _backgroundRectTransform.sizeDelta = new Vector2(_minWidth, _minHeight);
        _tailRect = _talkTail.GetComponent<RectTransform>();
        _rectTransform = GetComponent<RectTransform>();
        
        RectTransform screen = GameObject.FindWithTag("UI").GetComponent<RectTransform>();
        _screenSize = new Vector2(screen.sizeDelta.x, screen.sizeDelta.y);
        _screenFactor = _screenSize.x / Screen.width;
        
        _cam = Camera.main;
        _parentPos = transform;
        
        _tailRect.localScale = Vector3.zero;

        _rectTransform.anchoredPosition = _cam.WorldToScreenPoint(_spriteRenderer.bounds.center);

        LateUpdate();
    }
    
    void LateUpdate()
    {
        if (_killing) return;

        Vector3 min = _spriteRenderer.bounds.min;
        Vector3 max = _spriteRenderer.bounds.max;
 
        Vector3 screenMin = _cam.WorldToScreenPoint(min);
        Vector3 screenMax = _cam.WorldToScreenPoint(max);
        
        float parentHeight = screenMax.y - screenMin.y;

        if (_scale < 1)
        {
            _scale += Time.deltaTime;
            

            _tailRect.localScale = Vector3.Lerp(_tailRect.localScale, Vector3.one, _scale);
        }
        
        if (_text != _textMeshPro.text)
        {
            if (string.IsNullOrEmpty(_text)) _textMeshPro.ForceMeshUpdate();
            _text = _textMeshPro.text;
            _time = 0;
        }

        _time += Time.deltaTime;
        
        _backgroundRectTransform.sizeDelta = Vector2.Lerp(_backgroundRectTransform.sizeDelta, new Vector2(_textMeshPro.textBounds.size.x + _minWidth, _textMeshPro.textBounds.size.y + _minHeight), _time);

        Vector3 pos = _cam.WorldToScreenPoint(_spriteRenderer.bounds.center);
        Vector3 targetPos;
        int offsetAmount = 10;
        
        // Bubble Above Head
        targetPos = new Vector3(pos.x, pos.y + (_backgroundRectTransform.sizeDelta.y / 2 + _tailRect.sizeDelta.y + parentHeight) , pos.z);
            
        _tailRect.anchorMax = new Vector2(0.5f, 0);
        _tailRect.anchorMin = new Vector2(0.5f, 0);
        _tailRect.rotation = Quaternion.Euler(0f, 0f, 180);

        if (targetPos.y + _backgroundRectTransform.sizeDelta.y >= _screenSize.y || _forceBottom) {
            // Bubble Below Head
            targetPos = new Vector3(pos.x, pos.y - (_backgroundRectTransform.sizeDelta.y / 2 + _tailRect.sizeDelta.y + parentHeight), pos.z);
            
            _tailRect.anchorMax = new Vector2(0.5f, 1);
            _tailRect.anchorMin = new Vector2(0.5f, 1);
            _tailRect.rotation = Quaternion.Euler(0f, 0f, 0);

            offsetAmount *= -1;
        }

        _rectTransform.anchoredPosition = Vector2.Lerp(pos, targetPos, _scale * 5);
        
        float xpos = _rectTransform.anchoredPosition.x;
        xpos = Mathf.Clamp(xpos, _backgroundRectTransform.sizeDelta.x + 15, _screenSize.x - _backgroundRectTransform.sizeDelta.x - 15);
        _tailRect.anchoredPosition = new Vector2((pos.x - xpos) / 2, 0);
        _rectTransform.anchoredPosition = new Vector2(xpos, _rectTransform.anchoredPosition.y + offsetAmount);
    }

    public IEnumerator Kill()
    {
        _killing = true;
        _time = 0;

        _textMeshPro.text = "";

        Vector3 startPos = _rectTransform.anchoredPosition;
        Vector3 endPos = _cam.WorldToScreenPoint(_spriteRenderer.bounds.center);

        while (_time < 0.1)
        {
            _rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, _time * 5);
            _tailRect.localScale = Vector3.Lerp(_tailRect.localScale, Vector3.zero, _time);
            _backgroundRectTransform.sizeDelta = Vector2.Lerp(_backgroundRectTransform.sizeDelta, new Vector2(_minWidth, _minHeight), _time);


            _time += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
