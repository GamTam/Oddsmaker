using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.State_Machine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class BattleManager : MonoBehaviour
{
    public string _song;
    public string _winSong = "Victory";
    [SerializeField] private GameObject _playerList;
    [SerializeField] private GameObject _enemyList;
    [SerializeField] private MeshRenderer _background;
    [HideInInspector] public Material _mat;
    
    public EnemySelectionController _enemySelectionController;
    
    public TMP_Text _textBox;
    private List<string> _textBoxText = new List<string>();
    private int _currentTextBoxLine;
    public bool _canFlee;
    public int _XPTotal;
    
    [Header("Player Box")]
    public TMP_Text _nameTagText;
    public Image _nameTagImage;
    public Button[] _buttons;
    public TextSelection[] _selectionBoxes;
    [HideInInspector] public List<Image> _buttonImages = new List<Image>();
    
    public State _state;
    [HideInInspector] public List<Battleable> _fighters = new List<Battleable>();
    [HideInInspector] public List<Animator> _players = new List<Animator>();
    [HideInInspector] public List<Enemy> _enemies = new List<Enemy>();
    [HideInInspector] public List<Vector3> _profilePoints = new List<Vector3>();
    [HideInInspector] public MusicManager _musicManager;
    [HideInInspector] public SoundManager _soundManager;
    private int _currentFighterIndex;
    [HideInInspector] public List<Animator> _deadPlayers = new List<Animator>();
    [HideInInspector] public List<Enemy> _deadEnemies = new List<Enemy>();
    [HideInInspector] public int _currentPlayerIndex;
    [HideInInspector] public int _turnIndex;
    [HideInInspector] public int _currentButton = 0;
    [HideInInspector] public bool _overworldMusic;
    
    [HideInInspector] public bool _cancelMovement;
    
    [HideInInspector] public PlayerInput _playerInput;
    [HideInInspector] public InputAction _moveVector;
    [HideInInspector] public InputAction _confirm;
    [HideInInspector] public InputAction _back;
    
    [TextArea(4, 4)] public string _startingText;
    [TextArea(4, 4)] public string _winText = "* You defeated the enemies!";

    public List<sItem> _items = new List<sItem>();
    public bool _inBattle = true;
    
    public DialogueVertexAnimator dialogueVertexAnimator;
    public bool _selectingPlayers;

    private GameObject _selectedObj;
    private List<Coroutine> _textCoroutines = new List<Coroutine>();

    private void Start()
    {
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _moveVector = _playerInput.actions["Menu/Move"];
        _confirm = _playerInput.actions["Menu/Confirm"];
        _back = _playerInput.actions["Menu/Cancel"];
        
        // Globals.Items = _items;
        
        _background.material = new Material(_background.material);
        _mat = _background.material;
        _mat.SetFloat("_alpha", 0);

        dialogueVertexAnimator = new DialogueVertexAnimator(_textBox);
        
        foreach (Battleable chara in _playerList.GetComponentsInChildren<Battleable>())
        {
            _players.Add(chara.gameObject.GetComponent<Animator>());
            _profilePoints.Add(chara.gameObject.transform.localPosition);
            _fighters.Add(chara);
        }

        foreach (Battleable chara in _enemyList.GetComponentsInChildren<Battleable>())
        {
            _enemies.Add((Enemy) chara);
            _fighters.Add(chara);
            _XPTotal += chara._exp;
        }
        
        _currentPlayerIndex = 0;
        _currentFighterIndex = 0;
        _turnIndex = 0;
        
        SortFighters();

        _currentFighterIndex = -1;
        _turnIndex = -1;

        foreach (Button button in _buttons)
        {
            _buttonImages.Add(button.GetComponent<Image>());
        }

        _selectedObj = EventSystem.current.currentSelectedGameObject;
        
        _musicManager = GameObject.FindWithTag("Audio").GetComponent<MusicManager>();
        _soundManager = GameObject.FindWithTag("Audio").GetComponent<SoundManager>();

        Dictionary<String, int> names = new Dictionary<string, int>();
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            string text = _enemies[i]._name;
                        
            if (!names.ContainsKey(text)) names.Add(text, 0);
            names[text] += 1;
            if (names[text] > 1)
            {
                text += $" {Globals.NumberToChar(names[text], true)}";
            }

            _enemies[i].gameObject.name = text;
        }
        
        ClearBattleText();
        SwitchStates(new Opening(this));
    }

    private void Update()
    {
        if (!_selectingPlayers) return;

        if (_confirm.triggered)
        {
            Globals.SoundManager.Play("confirm");
            Click();
            return;
        }

        if (_back.triggered)
        {
            Globals.SoundManager.Play("back");
            Back();
            return;
        }
    }

    private void LateUpdate()
    {
        if (!Equals(_selectedObj, EventSystem.current.currentSelectedGameObject) && EventSystem.current.currentSelectedGameObject != null)
        {
            _selectedObj = EventSystem.current.currentSelectedGameObject;

            if (!_confirm.triggered && !_back.triggered && _playerInput.currentActionMap.name != "Null")
            {
                _soundManager.Play("select");
            }
        }
    }

    public void PickTurn()
    {
        _currentFighterIndex = (_currentFighterIndex + 1) % _fighters.Count;
        if (_currentFighterIndex == 0) _turnIndex += 1;
        
        bool playersDead = true;
        foreach (Animator player in _players)
        {
            if (!_deadPlayers.Contains(player))
            {
                playersDead = false;
                break;
            }
        }

        if (playersDead)
        {
            SwitchStates(new LoseState(this));
            return;
        }
        
        bool enemysDead = true;
        foreach (Enemy enemy in _enemies)
        {
            if (!_deadEnemies.Contains(enemy))
            {
                enemysDead = false;
                break;
            }
        }

        if (enemysDead)
        {
            SwitchStates(new WinState(this));
            return;
        }
        
        SortFighters(false);
        
        if (_fighters[_currentFighterIndex]._HP > 0)
        {
            if (_fighters[_currentFighterIndex].GetType() == typeof(PlayerBattle))
            {
                SwitchStates(new PlayerTurn(this, (PlayerBattle) _fighters[_currentFighterIndex]));
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            }
            else
            {
                SwitchStates(new EnemyTurn(this, (Enemy) _fighters[_currentFighterIndex]));
            }

            return;
        }

        if (_fighters[_currentFighterIndex].GetType() == typeof(PlayerBattle))
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }

        PickTurn();
    }

    public void Click()
    {
        StartCoroutine(_state.OnClick());
    }

    public void Back()
    {
        StartCoroutine(_state.OnBack());
    }
    
    public void SwitchStates(State state)
    {
        if (_state != null) StartCoroutine(_state.ExitState());
        _state = state;
        StartCoroutine(state.EnterState());
    }

    public void SortFighters(bool position = true)
    {
        _profilePoints.Sort((x, y) => x.y.CompareTo(y.y));
        _fighters.Sort((x, y) => y._speed.CompareTo(x._speed));

        if (!position) return;
        int i = 0;
        foreach (Battleable bat in _fighters)
        {
            if (bat.GetType() != typeof(PlayerBattle)) continue;
            bat.transform.localPosition = _profilePoints[(i + _currentPlayerIndex) % _players.Count];
            Vector2 vec = ((PlayerBattle) bat).StartingLocation;
            ((PlayerBattle) bat).StartingLocation = new Vector2(vec.x, bat.transform.localPosition.y);
            i++;
        }
    }

    public void InitFinalSlide(GameObject obj, Vector3 position, float duration=2)
    {
        StartCoroutine(FinalSlide(obj, position, new Vector3(1, 1, 1), duration));
    }
    
    public void InitFinalSlide(GameObject obj, Vector3 position, Vector3 scale, float duration=2)
    {
        StartCoroutine(FinalSlide(obj, position, scale, duration));
    }
    
    private IEnumerator FinalSlide(GameObject obj, Vector3 position, Vector3 scale, float movementDuration)
    {
        _cancelMovement = false;
        float timeElapsed = 0;
            
        while (timeElapsed < movementDuration && !_cancelMovement)
        {
            timeElapsed += Time.deltaTime;
            obj.gameObject.transform.localPosition = Vector3.Lerp(obj.gameObject.transform.localPosition, position, Time.deltaTime * 5);
            obj.gameObject.transform.localScale = Vector3.Lerp(obj.transform.localScale, scale, Time.deltaTime * 5);
                    
            yield return null;
        }

        if (!_cancelMovement)
        {
            obj.gameObject.transform.localPosition = position;
            obj.gameObject.transform.localScale = scale;
        }
    }
    
    public void DisableButtons()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i].gameObject == EventSystem.current.currentSelectedGameObject)
                _currentButton = i;
            _buttons[i].interactable = false;
        }
    }
    
    public void EnableButtons()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].interactable = true;
        }

        SelectButton();
    }

    public void SelectButton()
    {
        EventSystem.current.SetSelectedGameObject(_buttons[_currentButton].gameObject);
        _selectedObj = _buttons[_currentButton].gameObject;
    }

    public void SetBattleText(string text, bool reset = false)
    {
        foreach (Coroutine coroutine in _textCoroutines)
        {
            StopCoroutine(coroutine);
        }
        
        _textCoroutines = new List<Coroutine>();
        
        if (reset) _textBoxText = new List<string>();
        _textBoxText.Add(text);
        
        Queue<string> textQueue = new Queue<string>();
        
        if (_textBoxText.Count > 3)
        {
            _textBoxText.RemoveAt(0);
        }
        
        string str = "";
        for (int i = 0; i < _textBoxText.Count; i++)
        {
                str += $"{_textBoxText[i]}\n";
        }

        string[] tempList = str.Split("\n");

        for (int i = 0; i < tempList.Length - 1; i++)
        {
            textQueue.Enqueue(tempList[i]);
        }

        while (textQueue.Count > 3)
        {
            textQueue.Dequeue();
        }

        str = "";
        while (textQueue.Count != 0)
        {
            var tempStr = textQueue.Dequeue();
            str += tempStr + "\n";
        }
        
        List<DialogueCommand> commands = DialogueUtility.ProcessInputString(str, out string processedStr);
        DialogueUtility.ProcessInputString(text, out string processedText);
        
        int startIndex = Globals.RemoveRichText(processedStr).Length - Globals.RemoveRichText(processedText).Length;
        
        _textCoroutines.Add(StartCoroutine(dialogueVertexAnimator.AnimateTextIn(commands, processedStr, "typewriter", null, startIndex)));
    }
    
    

    public IEnumerator BattleText(string message, bool reset = false, bool autoEnd = false)
    {
        SetBattleText(message, reset);
            
        while (dialogueVertexAnimator.textAnimating)
        {
            if (_confirm.triggered)
            {
                dialogueVertexAnimator.QuickEnd();
            }
            yield return null;
        }

        while (!autoEnd)
        {
            if (_confirm.triggered)
            {
                yield return null;
                _soundManager.Play("confirm");
                break;
            }
                            
            yield return null;
        }
    }

    public IEnumerator FadeOutPlayerText()
    {
        float timeElapsed = 0;
        float movementDuration = 1;
        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            _nameTagText.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
            _nameTagImage.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                            
            foreach (Image img in _buttonImages)
            {
                img.color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
                img.GetComponentInChildren<TMP_Text>().color = Color.Lerp(Color.white, Color.clear, timeElapsed/movementDuration);
            }

            yield return null;
        }
    }

    public void ClearBattleText()
    {
        foreach (Coroutine coroutine in _textCoroutines)
        {
            StopCoroutine(coroutine);
        }
        
        _textCoroutines = new List<Coroutine>();
        _textBoxText = new List<string>();
        
        _textBox.SetText("");
    }
    
    public void EnableEnemySelection()
    {
        Dictionary<String, int> names = new Dictionary<string, int>();

        Enemy enemy = null;
            
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            string text = _enemies[i]._name;
                        
            if (!names.ContainsKey(text)) names.Add(text, 0);
            names[text] += 1;
            if (names[text] > 1)
            {
                text += $" {Globals.NumberToChar(names[text], true)}";
            }

            _enemies[i].gameObject.name = text;

            if (_enemies[i]._HP > 0)
            {
                _enemies[i].GetComponent<Button>().interactable = true;
                if (!enemy) enemy = _enemies[i];
            }
        }

        DisableButtons();

        EventSystem.current.SetSelectedGameObject(enemy!.gameObject);
        _enemySelectionController.SwitchEnemy(enemy);
    }
}

public enum DamageTypes
{
    HP,
    MP,
    Money
}