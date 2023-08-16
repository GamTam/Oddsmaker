using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Battle.State_Machine
{
    public class LoseState : State
    {
        public LoseState(BattleManager bm) : base(bm)
        {}

        public override IEnumerator EnterState()
        {
            _battleManager._musicManager.fadeOut(0.25f);
            yield return new WaitForSeconds(0.5f);
            _battleManager._musicManager.Play("GameOver");
            
            foreach (Animator anim in _battleManager._players)
            {
                if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Wide")) anim.Play("Shrink From Wide");
                else anim.Play("Shrink From Tall");
            }
            
            float timeElapsed = 0;
            float movementDuration = 1;
            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                _battleManager._nameTagText.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                _battleManager._nameTagImage.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                
                foreach (Image img in _battleManager._buttonImages)
                {
                    img.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                    img.GetComponentInChildren<TMP_Text>().color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                }

                yield return null;
            }

            yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* You were defeated...", true));
            
            Application.Quit();
        }
    }
}