using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.State_Machine
{
    public class Opening : State
    {
        public Opening(BattleManager bm) : base(bm)
        {
        }

        public override IEnumerator EnterState()
        {
            Globals.InBattle = true;
            _battleManager._playerInput.SwitchCurrentActionMap("Null");

            Color nameColor = _battleManager._nameTagText.color;
            Color[] buttonColor = new Color[_battleManager._buttonImages.Count];
            PlayerBattle firstPlayer = null;
            
            _battleManager._nameTagText.color = Color.clear;
            _battleManager._nameTagImage.color = Color.clear;
            foreach (Image img in _battleManager._buttonImages)
            {
                img.color = Color.clear;
                img.GetComponentInChildren<TMP_Text>().color = Color.clear;
            }

            GameObject textBox = _battleManager._textBox.gameObject.transform.parent.gameObject;

            Vector3 textBoxPos = textBox.transform.localPosition;

            textBox.transform.localPosition = new Vector3(textBoxPos.x, textBoxPos.y - 300f, textBoxPos.z);

            _battleManager.gameObject.transform.localScale = new Vector3(1, 2, 1);
            
            yield return new WaitForSeconds(0.5f);
            
            if (!_battleManager._overworldMusic) Globals.MusicManager.Play(_battleManager._song);
            //Globals.MusicManager.FadeVariation("Minigame");
            
            float movementDuration = 4;
            float timeElapsed = 0;
            
            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                _battleManager._mat.SetFloat("_alpha", Mathf.Lerp(0, 1, timeElapsed / movementDuration));
                yield return null;
            }
            _battleManager._mat.SetFloat("_alpha", 1);
            yield return new WaitForSeconds(0.1f);

            foreach (Enemy enemy in _battleManager._enemies)
            {
                _battleManager.InitFinalSlide(enemy.gameObject, enemy.StartingLocation, enemy.transform.localScale, 3);
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);
            
            _battleManager.InitFinalSlide(textBox.gameObject, textBoxPos, textBox.transform.localScale, 3);
            _battleManager.InitFinalSlide(_battleManager.gameObject, _battleManager.transform.localPosition, 3);

            yield return new WaitForSeconds(1.5f);
            
            _battleManager._playerInput.SwitchCurrentActionMap("Menu");
            yield return _battleManager.StartCoroutine(_battleManager.BattleText(_battleManager._startingText));
            
            _battleManager._playerInput.SwitchCurrentActionMap("Null");

            movementDuration = 1;
            timeElapsed = 0;

            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                foreach (Battleable obj in _battleManager._fighters)
                {
                    if(obj.GetType() != typeof(PlayerBattle)) continue;
                    if (firstPlayer == null) firstPlayer = (PlayerBattle) obj;
                    obj.gameObject.transform.localPosition = Vector3.Lerp(obj.gameObject.transform.localPosition, ((PlayerBattle) obj).StartingLocation, Time.deltaTime * 5);
                }
                yield return null;
            }
            
            firstPlayer.SetNameText();
            
            foreach (Battleable obj in _battleManager._fighters)
            {
                if(obj.GetType() != typeof(PlayerBattle)) continue;
                _battleManager.InitFinalSlide(obj.gameObject, ((PlayerBattle) obj).StartingLocation);
            }

            foreach (Animator anim in _battleManager._players)
            {
                if (anim.gameObject == firstPlayer.gameObject) anim.Play("Wide From Shrink");
                else anim.Play("Tall From Shrink");
            }

            timeElapsed = 0;
            movementDuration = 1;
            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                _battleManager._nameTagText.color = Color.Lerp(Color.clear, nameColor, timeElapsed/movementDuration);
                _battleManager._nameTagImage.color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                
                foreach (Image img in _battleManager._buttonImages)
                {
                    img.color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                    img.GetComponentInChildren<TMP_Text>().color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                }

                yield return null;
            }

            _battleManager._playerInput.SwitchCurrentActionMap("Menu");
            _battleManager.PickTurn();
        }
    }
}