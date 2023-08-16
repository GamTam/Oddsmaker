using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image[] _blueSprites;
    [SerializeField] private Image[] _redSprites;
    [Space] [SerializeField] private bool _show;
    [SerializeField] private float _speed = 3;
    [SerializeField] private Animator _explosion;
    [SerializeField] private Shake _shake;
    private int _hp = 10;
    private int _warnAmount = 0;
    private float _warnFlashTimer;
    private float _moveDelay;

    [HideInInspector] public bool _hitAnimation = false;
    
    private int[] _positions = new[] {770, 1150};

    private void Update()
    {
        for (int i=0; i < _blueSprites.Length; i++)
        {
            _blueSprites[i].color = Color.white;
            if (i + 1 > _hp)
            {
                _blueSprites[i].color = Color.clear;
            }
        }

        if (_warnAmount == 0) _warnFlashTimer = -0.5f;
        else _warnFlashTimer += Time.deltaTime * 4f;
        
        for (int i=0; i < _redSprites.Length && !_hitAnimation; i++)
        {
            _redSprites[i].color = Color.clear;
            if (i + 1 > _hp - _warnAmount && i + 1 <= _hp)
            {
                _redSprites[i].color = new Color(1, 1, 1, -1 * (Mathf.Cos(_warnFlashTimer) / 2f) + 0.5f);
            }
        }

        _moveDelay -= Time.deltaTime;
        Vector3 targetPos =
            new Vector3(_positions[_show ? 0 : 1], transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition != targetPos && _moveDelay <= 0)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, _speed);
            _blueSprites[0].transform.parent.parent.gameObject.SetActive(true);
        }
        else if (!_show && _moveDelay <= 0) _blueSprites[0].transform.parent.parent.gameObject.SetActive(false);
    }

    public IEnumerator Hit(int amount)
    {
        _hitAnimation = true;
        _warnAmount = Mathf.Abs(amount);
        for (int i=0; i < _redSprites.Length; i++)
        {
            _redSprites[i].color = Color.clear;
        }
        yield return new WaitForSeconds(0.05f);
        
        for (int i=0; i < _redSprites.Length; i++)
        {
            _redSprites[i].color = Color.clear;
            if (i + 1 > _hp - _warnAmount && i + 1 <= _hp)
            {
                _redSprites[i].color = new Color(1, 1, 1, 1);
            }
        }

        yield return new WaitForSeconds(0.1f);
        
        for (int i=0; i < _redSprites.Length; i++)
        {
            _redSprites[i].color = Color.clear;
            if (i + 1 > _hp - _warnAmount && i + 1 <= _hp)
            {
                _redSprites[i].color = new Color(0, 0, 0, 1);
            }
        }

        yield return new WaitForSeconds(0.15f);
        
        for (int i=0; i < _redSprites.Length; i++)
        {
            _redSprites[i].color = Color.clear;
            if (i + 1 > _hp - _warnAmount && i + 1 <= _hp)
            {
                _redSprites[i].color = new Color(1, 1, 1, 1);
            }
        }
        
        for (int i=0; i < _blueSprites.Length; i++)
        {
            if (i + 1 <= _hp)
            {
                _explosion.transform.position = _blueSprites[i].gameObject.transform.position;
            }
        }
        Globals.SoundManager.Play("explosion");
        _explosion.Play("HealthBarExplosion");
        _shake.enabled = true;

        yield return new WaitForSeconds(0.1f);
        
        _warnAmount = 0;
        ChangeHP(amount);
        
        for (int i=0; i < _blueSprites.Length; i++)
        {
            _blueSprites[i].color = Color.white;
            if (i + 1 > _hp)
            {
                _blueSprites[i].color = Color.clear;
            }
        }
        for (int i=0; i < _redSprites.Length; i++)
        {
            _redSprites[i].color = Color.clear;
        }
        for (int i=0; i < _blueSprites.Length; i++)
        {
            if (i <= _hp)
            {
                _explosion.transform.position = _blueSprites[i].gameObject.transform.position;
            }
        }

        yield return new WaitForSeconds(0.9f);
        _hitAnimation = false;
        Hide();
    }

    public void Show(bool skipSlide=false)
    {
        _show = true;

        if (skipSlide)
        {
            Vector3 targetPos =
                new Vector3(_positions[0], transform.localPosition.y, transform.localPosition.z);
            transform.localPosition = targetPos;
            _blueSprites[0].transform.parent.parent.gameObject.SetActive(true);
        }
    }

    public void Hide(bool skipSlide=false)
    {
        _show = false;
        _moveDelay = 0.25f;
        
        if (skipSlide)
        {
            Vector3 targetPos =
                new Vector3(_positions[1], transform.localPosition.y, transform.localPosition.z);
            transform.localPosition = targetPos;
            _blueSprites[0].transform.parent.parent.gameObject.SetActive(false);
        }
    }

    public void SetWarnAmount(int amount)
    {
        _warnAmount = amount;
    }

    public void ChangeHP(int amount)
    {
        _hp += amount;
        _hp = Mathf.Min(_hp, 10);
        _hp = Mathf.Max(_hp, 0);
        Globals.HP = _hp;
    }

    public void SetHP(int amount)
    {
        _hp = amount;
        Globals.HP = _hp;
    }
}
