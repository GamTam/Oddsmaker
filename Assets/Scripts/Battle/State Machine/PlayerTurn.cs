using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace Battle.State_Machine
{
    public class PlayerTurn : State
    {
        private PlayerBattle _player;
        private Random _rand = new Random();
        
        public PlayerTurn(BattleManager bm, PlayerBattle player) : base(bm)
        {
            _player = player;
        }

        public override IEnumerator EnterState()
        {
            _battleManager._playerInput.SwitchCurrentActionMap("Null");
            _battleManager._cancelMovement = true;
            
            _battleManager._nameTagText.color = Color.white;
            _battleManager._nameTagImage.color = Color.white;
                
            foreach (Image img in _battleManager._buttonImages)
            {
                img.color = Color.white;
                img.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
            
            if ((_battleManager._turnIndex == 0 && _battleManager._currentPlayerIndex == 0) || _battleManager._players.Count == 1)
            {
                _battleManager.SetBattleText($"* {_player._name.ToUpper()}'s turn!", true);
                goto movedIcons;
            }

            PlayerBattle currentPlayer = null;
            PlayerBattle startingPlayer = null;
            int i = 0;
            foreach (Battleable obj in _battleManager._fighters)
            {
                if(obj.GetType() != typeof(PlayerBattle)) continue;
                int point = Globals.Mod(i + _battleManager._currentPlayerIndex, _battleManager._players.Count);
                
                if (point == 0) currentPlayer = (PlayerBattle) obj;
                if (point == 1) startingPlayer = (PlayerBattle) obj;

                ((PlayerBattle) obj).StartingLocation = new Vector2(obj.transform.localPosition.x, _battleManager._profilePoints[(_battleManager._players.Count - point) % _battleManager._players.Count].y);
                i--;
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

            _battleManager.SetBattleText($"* {_player._name.ToUpper()}'s turn!", true);
            yield return new WaitForSeconds(0.5f);
            
            movementDuration = 1;
            timeElapsed = 0;

            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                foreach (Battleable obj in _battleManager._fighters)
                {
                    if(obj.GetType() != typeof(PlayerBattle)) continue;
                    obj.gameObject.transform.localPosition = Vector3.Lerp(obj.gameObject.transform.localPosition, ((PlayerBattle) obj).StartingLocation, Time.deltaTime * 5);
                }
                yield return null;
            }
            
            currentPlayer.SetNameText();

            foreach (Battleable obj in _battleManager._fighters)
            {
                if(obj.GetType() != typeof(PlayerBattle)) continue;
                _battleManager.InitFinalSlide(obj.gameObject, ((PlayerBattle) obj).StartingLocation);
            }
            
            foreach (Animator anim in _battleManager._players)
            {
                if (anim.gameObject == currentPlayer.gameObject) anim.Play("Wide From Shrink");
                else anim.Play("Tall From Shrink");
            }
            
            timeElapsed = 0;
            movementDuration = 1;
            while (timeElapsed < movementDuration)
            {
                timeElapsed += Time.deltaTime;
                _battleManager._nameTagText.color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                _battleManager._nameTagImage.color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                
                foreach (Image img in _battleManager._buttonImages)
                {
                    img.color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                    img.GetComponentInChildren<TMP_Text>().color = Color.Lerp(Color.clear, Color.white, timeElapsed/movementDuration);
                }

                yield return null;
            }
            
            movedIcons:
            if (_player._HP <= 0)
            {
                yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} is unable to participate!"));
                _battleManager.PickTurn();
                yield break;
            }
            
            _battleManager._playerInput.SwitchCurrentActionMap("Menu");
            _battleManager.SelectButton();

            foreach (Button button in _battleManager._buttons)
            {
                button.interactable = true;
            }
        }

        public override IEnumerator OnClick()
        {
            switch (EventSystem.current.currentSelectedGameObject.name)
            {
                case "Fight":
                case "Check":
                    _battleManager._soundManager.Play("confirm");
                    
                    _battleManager.EnableEnemySelection();
                    yield break;
                case "Special":
                    if (_player._attacks.Count == 0)
                    {
                        Globals.SoundManager.Play("error");
                        yield break;
                    }
                    
                    _battleManager._soundManager.Play("confirm");
                    _battleManager.DisableButtons();
                    _battleManager._selectionBoxes[0].gameObject.SetActive(true);
                    _battleManager._selectionBoxes[0].ResetButtons();
                    
                    foreach (AttackSO attack in _player._attacks)
                    {
                        string text = $"{attack.Name}\n<size=75%><color=blue>{attack.Cost} {attack.CostType}";

                        bool enableButton = false;
                        
                        switch (attack.CostType)
                        {
                            case DamageTypes.HP:
                                enableButton = _player._HP >= attack.Cost;
                                break;
                            case DamageTypes.MP:
                                enableButton = _player._MP >= attack.Cost;
                                break;
                        }
                        
                        _battleManager._selectionBoxes[0].AddButton(text, enableButton);
                    }
                    
                    EventSystem.current.SetSelectedGameObject(_battleManager._selectionBoxes[0]._buttons[0].Key.gameObject);
                    yield break;
                case "Item":
                    if (Globals.Items.Count == 0)
                    {
                        Globals.SoundManager.Play("error");
                        yield break;
                    }
                    
                    _battleManager._soundManager.Play("confirm");
                    _battleManager.DisableButtons();
                    _battleManager._selectionBoxes[0].gameObject.SetActive(true);
                    _battleManager._selectionBoxes[0].ResetButtons();
                    
                    foreach (sItem item in Globals.Items)
                    {
                        string text = $"{item.Item.Name}\n<size=75%>x{item.Count}";

                        bool enableButton = item.Count > 0;
                        
                        _battleManager._selectionBoxes[0].AddButton(text, enableButton);
                    }
                    EventSystem.current.SetSelectedGameObject(_battleManager._selectionBoxes[0]._buttons[0].Key.gameObject);
                    
                    yield break;
                case "Flee":
                    yield return null;
                    _battleManager._soundManager.Play("confirm");
                    _battleManager.DisableButtons();

                    #region Cannot Flee
                    if (!_battleManager._canFlee)
                    {
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText("* Cannot flee!", true, true));
                        yield return new WaitForSeconds(1);

                        _battleManager.SetBattleText($"* {_player._name.ToUpper()}'s turn!", true);
                        _battleManager.EnableButtons();
                        yield break;
                    }
                    #endregion

                    #region Can Flee

                    float partySpeed = 0;
                    float enemySpeed = 0;
                    foreach (Battleable bat in _battleManager._fighters)
                    {
                        if (bat.GetType() == typeof(PlayerBattle)) partySpeed += bat._speed;
                        else enemySpeed += bat._speed;
                    }

                    partySpeed /= _battleManager._players.Count;
                    enemySpeed /= _battleManager._enemies.Count;

                    int threshold = (int) (((enemySpeed / partySpeed) * 100) / 2);

                    int chance = _rand.Next(101);
                    
                    yield return _battleManager.StartCoroutine(_battleManager.BattleText("* You tried to escape...", autoEnd:true));
                    yield return new WaitForSeconds(1f);
                    
                    if (chance > threshold)
                    {
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText("* Escaped!", autoEnd:true));
                        yield return new WaitForSeconds(1);
                        
                        foreach (Animator anim in _battleManager._players)
                        {
                            if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Wide")) anim.Play("Shrink From Wide");
                            else anim.Play("Shrink From Tall");
                        }

                        foreach (Enemy enem in _battleManager._enemies)
                        {
                            Vector3 pos = enem.transform.localPosition;
                            pos.y -= 1080f;
                            _battleManager.InitFinalSlide(enem.gameObject, pos, enem.transform.localScale, 3);
                            yield return new WaitForSeconds(0.1f);
                        }

                        _battleManager.StartCoroutine(_battleManager.FadeOutPlayerText());
                        yield return new WaitForSeconds(0.75f);

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
                        
                        _battleManager._inBattle = false;
                        
                        float movementDuration = 2;
                        float timeElapsed = 0;
                        while (timeElapsed < movementDuration)
                        {
                            timeElapsed += Time.deltaTime;
                            _battleManager._mat.SetFloat("_alpha", Mathf.Lerp(1, 0, timeElapsed / movementDuration));
                            yield return null;
                        }
                        yield break;
                    }
                    else
                    {
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText("* ...but failed.", autoEnd:true));
                        yield return new WaitForSeconds(1);
                    }
                    
                    _battleManager.PickTurn();

                    #endregion
                    
                yield break;
            }

            Enemy enemy = EventSystem.current.currentSelectedGameObject.GetComponent<Enemy>();
            string textBoxSelection = _battleManager._selectionBoxes[0].GetSelectedButtonText().Split("\n")[0];

            int damage;
            Shake shake;
            bool crit;
            
            switch (_battleManager._buttons[_battleManager._currentButton].gameObject.name)
            {
                case "Fight":
                    yield return null;
                    _battleManager._enemySelectionController.Disable();            
            
                    _battleManager.ClearBattleText();
                    
                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} attacked {enemy.gameObject.name.ToUpper()}!", true, true));

                    damage = Globals.DamageFormula(_player._pow, enemy._def, out crit, _player._luck);
                    if (crit) damage *= 2;
                    
                    enemy._slider.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                    
                    shake = enemy.gameObject.GetComponent<Shake>();
                    enemy._HP -= damage;
                    
                    _battleManager._soundManager.Play("hit");
                    shake.maxShakeDuration = 0.25f;
                    shake.enabled = true;
                    
                    yield return new WaitForSeconds(0.5f);
                    enemy.InitSetRedSlider(enemy._HP);
                    yield return new WaitForSeconds(0.5f);

                    if (crit)
                    {
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText("<anim:shake>* A critical hit!</anim>", autoEnd:true));
                        yield return new WaitForSeconds(0.5f);
                    }

                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {enemy.gameObject.name.ToUpper()} took <color=red>{damage}</color> damage!", autoEnd:true));
                    yield return new WaitForSeconds(0.5f);
                    
                    enemy._slider.gameObject.SetActive(false);

                    if (enemy._HP <= 0)
                    {
                        enemy._killable = true;
                        _battleManager._soundManager.Play("enemyDie");
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} <color=red>defeated {enemy.gameObject.name.ToUpper()}</color>!", autoEnd:true));
                        yield return new WaitForSeconds(0.5f);
                        _battleManager._deadEnemies.Add(enemy);
                    }
                    
                    break;
                case "Check":
                    yield return null;
                    _battleManager._enemySelectionController.Disable();            
            
                    _battleManager.ClearBattleText();
                    
                    for(int i=0; i < enemy._description.Length; i++)
                    {
                        yield return null;
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText(enemy._description[i], true));
                    }
                    
                    break;
                case "Special":
                    if (!_battleManager._enemySelectionController.gameObject.activeSelf)
                    {
                        _battleManager.EnableEnemySelection();
                        _battleManager._selectionBoxes[0].Freeze();
                        yield break;
                    }

                    yield return null;
                    _battleManager._enemySelectionController.Disable();
                    _battleManager._selectionBoxes[0].ResetButtons();
                    _battleManager._selectionBoxes[0].gameObject.SetActive(false);
                    
                    _battleManager._selectionBoxes[0].UnFreeze();

                    AttackSO attack = ScriptableObject.CreateInstance<AttackSO>();
                    
                    foreach (AttackSO atk in _player._attacks)
                    {
                        if (atk.Name == textBoxSelection)
                        {
                            attack = atk;
                            break;
                        }
                    }

                    switch (attack.CostType)
                    {
                        case DamageTypes.HP:
                            _battleManager.StartCoroutine(_player.UpdateHpSlider(Math.Max(_player._HP - attack.Cost, 0)));
                            break;
                        case DamageTypes.MP:
                            _battleManager.StartCoroutine(_player.UpdateMpSlider(Math.Max(_player._MP - attack.Cost, 0)));
                            break;
                        case DamageTypes.Money:
                            break;
                    }
                    
                    _battleManager.ClearBattleText();
                    
                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} used {attack.Name.ToUpper()} on {enemy.gameObject.name.ToUpper()}!", true, autoEnd:true));

                    damage = Globals.DamageFormula(_player._pow + attack.Strength, enemy._def, out crit, _player._luck);
                    if (crit) damage *= 2;
                    
                    enemy._slider.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                    
                    shake = enemy.gameObject.GetComponent<Shake>();
                    enemy._HP -= damage;
                    
                    _battleManager._soundManager.Play("hit");
                    shake.maxShakeDuration = 0.25f;
                    shake.enabled = true;
                    
                    yield return new WaitForSeconds(0.5f);
                    enemy.InitSetRedSlider(enemy._HP);
                    yield return new WaitForSeconds(0.5f);

                    if (crit)
                    {
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText("<anim:shake>* A critical hit!</anim>", autoEnd:true));
                        yield return new WaitForSeconds(0.5f);
                    }

                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {enemy.gameObject.name.ToUpper()} took <color=red>{damage}</color> damage!", autoEnd:true));
                    yield return new WaitForSeconds(0.5f);
                    
                    enemy._slider.gameObject.SetActive(false);

                    if (enemy._HP <= 0)
                    {
                        enemy._killable = true;
                        _battleManager._soundManager.Play("enemyDie");
                        yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} <color=red>defeated {enemy.gameObject.name.ToUpper()}</color>!", autoEnd:true));
                        yield return new WaitForSeconds(0.5f);
                        _battleManager._deadEnemies.Add(enemy);
                    }
                    
                    yield return new WaitForSeconds(0.5f);

                    break;
                case "Item":
                    if (!_battleManager._selectingPlayers)
                    {
                        _battleManager._selectionBoxes[0].Freeze();
                        
                        foreach (Animator player in _battleManager._players)
                        {
                            player.gameObject.GetComponent<Button>().interactable = true;
                            if (player.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Wide"))
                            {
                                EventSystem.current.SetSelectedGameObject(player.gameObject);
                            }
                        }

                        yield return null;
                        
                        _battleManager._selectingPlayers = true;

                        yield break;
                    }

                    yield return null;
                    
                    PlayerBattle target = EventSystem.current.currentSelectedGameObject.GetComponent<PlayerBattle>();

                    foreach (Animator player in _battleManager._players)
                    {
                        player.gameObject.GetComponent<Button>().interactable = false;
                    }
                    
                    _battleManager._selectingPlayers = false;
                    _battleManager._selectionBoxes[0].ResetButtons();
                    _battleManager._selectionBoxes[0].gameObject.SetActive(false);
                    
                    _battleManager._selectionBoxes[0].UnFreeze();

                    AttackSO item = ScriptableObject.CreateInstance<AttackSO>();
                    
                    for (int i = 0; i < Globals.Items.Count; i++)
                    {
                        sItem itm = Globals.Items[i];
                        if (Globals.Items[i].Item.Name == textBoxSelection)
                        {
                            item = Globals.Items[i].Item;
                            itm.Count -= 1;
                            Globals.Items[i] = itm;
                            break;
                        }
                    }

                    _battleManager.ClearBattleText();
                    
                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* {_player._name.ToUpper()} gave a {item.Name.ToUpper()} to {target._name.ToUpper()}!", true, true));
                    
                    switch (item.CostType)
                    {
                        case DamageTypes.HP:
                            _battleManager.StartCoroutine(target.UpdateHpSlider(Math.Min(target._HP + item.Strength, target._maxHP)));
                            break;
                        case DamageTypes.MP:
                            _battleManager.StartCoroutine(target.UpdateMpSlider(Math.Min(target._MP + item.Strength, target._maxMP)));
                            break;
                        case DamageTypes.Money:
                            break;
                    }
                    
                    yield return new WaitForSeconds(0.5f);
                    
                    yield return _battleManager.StartCoroutine(_battleManager.BattleText($"* Healed {item.Strength} {item.CostType}!", autoEnd:true));
                    
                    Animator anim = target.gameObject.GetComponent<Animator>();
                    if (target._HP > 0 && _battleManager._deadPlayers.Contains(anim)) _battleManager._deadPlayers.Remove(anim);
                    yield return new WaitForSeconds(0.5f);

                    break;
            }
            
            _battleManager.PickTurn();
        }

        public override IEnumerator OnBack()
        {
            switch (_battleManager._buttons[_battleManager._currentButton].gameObject.name)
            {
                case "Special":
                    if (_battleManager._enemySelectionController.gameObject.activeSelf)
                    {
                        _battleManager._enemySelectionController.Disable();
                        _battleManager._selectionBoxes[0].UnFreeze();
                        
                        yield break;
                    }
                    goto default;
                case "Item":
                    if (_battleManager._selectingPlayers)
                    {
                        _battleManager._selectingPlayers = false;
                        
                        _battleManager._selectionBoxes[0].UnFreeze();
                        
                        foreach (Animator player in _battleManager._players)
                        {
                            player.gameObject.GetComponent<Button>().interactable = false;
                        }

                        yield break;
                    }
                    goto default;
                default:
                    _battleManager._selectionBoxes[0].ResetButtons();
                    _battleManager._selectionBoxes[0].gameObject.SetActive(false);
                    
                    _battleManager._selectionBoxes[0].UnFreeze();
                    
                    _battleManager._enemySelectionController.Disable();
                    
                    _battleManager.EnableButtons();
                    break;
            }
        }
    }
}