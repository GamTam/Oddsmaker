using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public float _moveSpeed = 2.5f;
    [SerializeField] private Rigidbody2D _char;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private string _song;
    
    [HideInInspector] public PlayerInput _playerInput;
    [HideInInspector] public InputAction _moveVector;
    [HideInInspector] public InputAction _interact;
    [HideInInspector] public InputAction _save;
    [HideInInspector] public InputAction _load;
    [HideInInspector] public bool _interacting;

    private Vector2 _prevMoveVector;
    private PlayerDir _facing = PlayerDir._d;
    
    public bool IsMoving => _moveVector.ReadValue<Vector2>() != Vector2.zero;
    
    IEnumerator Start()
    {
        if (Globals.Player != null) Destroy(Globals.Player);

        Globals.Player = this;

        _facing = Globals.PlayerDir;
        
        if (Globals.PlayerStatsList[0] != null) _stats = Globals.PlayerStatsList[0];
        else Globals.PlayerStatsList[0] = _stats;

        if (Globals.PlayerPos != Vector3.zero)
        {
            transform.position = Globals.PlayerPos;
            Globals.PlayerPos = Vector3.zero;
        }
        
        Globals.GameState = GameState.Play;
        
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
        _moveVector = _playerInput.actions["Overworld/Move"];
        _interact = _playerInput.actions["Interact"];
        _save = _playerInput.actions["Save"];
        _load = _playerInput.actions["Load"];

        Globals.MusicManager.Play(_song);
        
        AsyncOperationHandle<Sprite> pfpHandler = _stats.PFPRef.LoadAssetAsync();
        AsyncOperationHandle<Sprite> deadPfpHandler = _stats.DeadPFPRef.LoadAssetAsync();
        while (!pfpHandler.IsDone && !deadPfpHandler.IsDone) yield return null;

        _stats.PFP = pfpHandler.Result;
        _stats.DeadPFP = deadPfpHandler.Result;
    }
    
    void Update()
    {
        Globals.PlayerDir = _facing;
        if (Globals.GameState != GameState.Play)
        {
            _char.velocity = Vector2.zero;
            return;
        }
        
        _interacting = _interact.triggered;
        
        Vector2 moveVector = _moveVector.ReadValue<Vector2>();

        if (_save.triggered)
        {
            FindObjectOfType<SaveData>().SaveIntoJson();
        }
        
        if (_load.triggered)
        {
            FindObjectOfType<SaveData>().LoadFromJson();
        }

        #region Set Animation

        if (moveVector == Vector2.zero)
        {
            _animator.Play($"idle{_facing.ToString()}");
        }
        else
        {
            if (moveVector == Vector2.up)
            {
                _facing = PlayerDir._u;
            } 
            else if (moveVector == Vector2.down)
            {
                _facing = PlayerDir._d;
            } 
            else if (moveVector == Vector2.left)
            {
                _facing = PlayerDir._l;
            } 
            else if (moveVector == Vector2.right)
            {
                _facing = PlayerDir._r;
            }
            
            _animator.Play($"walk{_facing.ToString()}");
        }

        #endregion

        _char.velocity = moveVector * _moveSpeed;
    }

    public void SetFacing(PlayerDir facing)
    {
        _facing = facing;
    }

    public PlayerDir GetFacing()
    {
        return _facing;
    }
}

public enum PlayerDir
{
    _u,
    _d,
    _l,
    _r
}
