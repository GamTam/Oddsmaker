using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace Battle.State_Machine
{
    public class WinState : State
    {
        public WinState(BattleManager bm) : base(bm)
        {}

        public override IEnumerator EnterState()
        {
            if (!_battleManager._overworldMusic)
            {
                _battleManager._musicManager.fadeOut(0.25f);
                yield return new WaitForSeconds(0.5f);
                _battleManager._musicManager.Play(_battleManager._winSong);
            }
            
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
            
            yield return _battleManager.StartCoroutine(_battleManager.BattleText(_battleManager._winText, true));
            yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* Gained {_battleManager._XPTotal} XP!", true));

            foreach (Battleable obj in _battleManager._fighters)
            {
                if(obj.GetType() != typeof(PlayerBattle)) continue;
                PlayerBattle player = (PlayerBattle) obj;

                if (player._HP <= 0)
                {
                    player.WriteStatsToGlobal();
                    continue;
                }
                player._exp += _battleManager._XPTotal;
                
                Debug.Log($"{player._name}: {player._exp}, {player._stats.ExpForNextLevel}");
                if (player._exp >= player._stats.ExpForNextLevel)
                {
                    yield return _battleManager.StartCoroutine(LevelUp(player));
                }
                
                player.WriteStatsToGlobal();
            }

            GameObject textBox = _battleManager._textBox.gameObject.transform.parent.gameObject;
            Vector3 textBoxPos = textBox.transform.localPosition;
            textBoxPos.y -= 300f;
            
            _battleManager.InitFinalSlide(textBox.gameObject, textBoxPos, textBox.transform.localScale, 3);
            _battleManager.InitFinalSlide(_battleManager.gameObject, _battleManager.transform.localPosition, new Vector3(1, 2, 1), 3);

            yield return new WaitForSeconds(0.25f);
            
            foreach (Battleable obj in _battleManager._fighters)
            {
                _battleManager.InitFinalSlide(obj.gameObject, new Vector3(obj.transform.localPosition.x - 500, obj.transform.localPosition.y, obj.transform.localPosition.z), 3);
            }
            
            if (!_battleManager._overworldMusic) Globals.MusicManager.fadeOut(2);
            //Globals.MusicManager.FadeVariation("Minigame_Mono");
            
            _battleManager._inBattle = false;
            
            movementDuration = 2;
            timeElapsed = 0;
            
            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                _battleManager._mat.SetFloat("_alpha", Mathf.Lerp(1, 0, timeElapsed / movementDuration));
                yield return null;
            }
        }

        public IEnumerator LevelUp(PlayerBattle player)
        {
            player._level += 1;
            yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {player._name} has reached level {player._level}!"));

            Random rand = new Random();
            int statIncrease = Globals.LevelUpLut(rand.Next(1, 16));

            player._maxHP += statIncrease;

            player._HP = player._maxHP;
            
            statIncrease = Globals.LevelUpLut(rand.Next(1, 16));

            player._maxMP += statIncrease;

            player._MP = player._maxMP;
            
            statIncrease = Globals.LevelUpLut(rand.Next(1, 11));

            player._pow += statIncrease;
            
            statIncrease = Globals.LevelUpLut(rand.Next(1, 4));

            player._def += statIncrease;
            
            statIncrease = Globals.LevelUpLut(rand.Next(1, 16));

            player._speed += statIncrease;
            
            statIncrease = Globals.LevelUpLut(rand.Next(1, 16));

            player._luck += statIncrease;
            
            player.WriteStatsToGlobal();
            
            if (player._exp >= player._stats.ExpForNextLevel)
            {
                yield return _battleManager.StartCoroutine(LevelUp(player));
            }
        }
    }
}