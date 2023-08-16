using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour
{
    public Transform _transform;
	
    public float maxShakeDuration;
    public float multiplier = 1;
    private float shakeDuration;
	
    private float shakeAmount = 0.25f;
    private float decreaseFactor = 1.0f;
    private float _defaultMultiplier;

    [SerializeField] private float _interval = 2f / 60f;
    private float _intervalTimer;

    [SerializeField] private bool _folowGlobalSetting = false;
	
    Vector3 originalPos;
	
    void Awake()
    {
        if (_transform == null)
        {
            _transform = GetComponent(typeof(Transform)) as Transform;
        }

        _defaultMultiplier = multiplier;
    }
	
    void OnEnable()
    {
        if (_folowGlobalSetting)
        {
            if (!Globals.ScreenShake) return;
        }
        originalPos = _transform.localPosition;
        shakeDuration = maxShakeDuration;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            if (_intervalTimer <= 0)
            {
                _intervalTimer = _interval;
                Vector2 newPos = originalPos + Random.insideUnitSphere * (shakeAmount * multiplier);
                _transform.localPosition = new Vector3(newPos.x, newPos.y, _transform.localPosition.z);
            }
        }
        else
        {
            shakeDuration = 0f;
            multiplier = _defaultMultiplier;
            _transform.localPosition = originalPos;
            enabled = false;
        }

        _intervalTimer -= Time.deltaTime;
        shakeDuration -= Time.deltaTime * decreaseFactor;
    }

    public void SetShakeInterval(float interval)
    {
        _interval = interval;
    }

    public void ResetShakeInterval()
    {
        _interval = 2f / 60f;
    }
}