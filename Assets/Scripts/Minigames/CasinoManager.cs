using System.Collections;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class CasinoManager : MonoBehaviour
{
    public bool _done = false;
    public Image _fade;
    public Animator _anim;

    public IEnumerator FadeOut()
    {
        _fade.color = new Color(1, 1, 1, 0);
        _anim.enabled = false;
        
        float duration = 0f;
        float maxDuration = 1f;
        
        while (duration < maxDuration)
        {
            duration += Time.deltaTime * 2;
            _fade.color = new Color(1, 1, 1, duration / maxDuration);
            Debug.Log(_fade.color.a);
            
            yield return null;
        }
        _fade.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(0.5f);

        _done = true;
    }
}