using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private Matching _matching;

    [SerializeField] private int _rows = 5;
    [SerializeField] private int _columns = 4;
    [SerializeField] public Color _unselectedColor = Color.gray;
    [SerializeField] public Color _selectedColor = Color.green;

    int selectedRow = 0;
    int selectedCol = 0;
    int _counter = 0;
    int[] _done;


    public Image[] _images;
    public Button[] _buttons;

    bool _soTrue;

    // Start is called before the first frame update
    void Start()
    {
        _done = new int[20];
        _images = new Image[20];
        _buttons = new Button[20];
        for(int i = 0; i < _matching._cards.Length; i++)
        {
            _done[i] = 100;
            _images[i] = _matching._cards[i].GetComponent<Image>();
            _buttons[i] = _matching._cards[i].GetComponent<Button>();
        }
        //EventSystem.current.SetSelectedGameObject(_matching._listGameObjects[0].gameObject);
        //_images[15].color = _selectedColor;
    }

    private void Check()
    {
        if(selectedCol < 0)
        {
            selectedCol = 3;
            _images[selectedRow + selectedCol].color = _selectedColor;
        }
        else if(selectedCol > 3)
        {
            selectedCol = 0;
            _images[selectedRow + selectedCol].color = _selectedColor;
        }

        if(selectedRow < 0)
        {
            selectedRow = 16;
            _images[selectedRow + selectedCol].color = _selectedColor;
        }
        else if(selectedRow > 16)
        {
            selectedRow = 0;
            _images[selectedRow + selectedCol].color = _selectedColor;
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.15f);
        _soTrue = false;
    }

    public void Record(int number)
    {
        _done[_counter] = number;
        _counter++;
    }
}
