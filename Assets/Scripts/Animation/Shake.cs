using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public Transform _transform;
	
    // How long the object should shake for.
    public float maxShakeDuration;
    public float multiplier = 1;
    private float shakeDuration;
	
    // Amplitude of the shake. A larger value shakes the camera harder.
    private float shakeAmount = 0.25f;
    private float decreaseFactor = 1.0f;

    [SerializeField] private float _interval = 2 / 60f;
    private float _intervalTimer;
	
    Vector3 originalPos;
	
    void Awake()
    {
        if (_transform == null)
        {
            _transform = GetComponent(typeof(Transform)) as Transform;
        }
    }
	
    void OnEnable()
    {
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
                _transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount * multiplier;
            }
        }
        else
        {
            shakeDuration = 0f;
            _transform.localPosition = originalPos;
            enabled = false;
        }

        _intervalTimer -= Time.deltaTime;
        shakeDuration -= Time.deltaTime * decreaseFactor;
    }
}