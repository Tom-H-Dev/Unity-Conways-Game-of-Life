using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateGrid : MonoBehaviour
{
    public int _width, _height;
    private bool _isGameActive;
    private float _timer;
    public float _maxTime;

    public GameObject[,] _cellGrid;
    public int[,] _activeCells , _activeCells2;
    public int[] _optionArrayX, _optionArrayY;

    public GameObject _cell;
    public Sprite _deadCell, _aliveCell;

    [SerializeField] private Transform _startPosition;


    private void Awake()
    {
        if (_width == 0 || _height == 0)
        {
            Debug.LogError("There is either no height or width set!");
        }

        if (_maxTime == 0)
        {
            Debug.LogError("There is no max error set");
        }

        if (_width < 3 || _height < 3)
        {
            Debug.LogError("The simulation play area is to small");
        }

        //sets the length of the tile array to the amount of squares
        _cellGrid = new GameObject[_width, _height];
        //sets teh same lenght of the array of the amount of gameobjects
        _activeCells = new int[_width, _height];
        _activeCells2 = new int[_width, _height];
    }

    void Start()
    {
        CalculateGrid();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //gets the right position of the mouse in the game
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //sets the live mouse position in a vector2
            Vector2 mousePos2D = new Vector2(mousePosition.x, mousePosition.y);

            //check what tile is hit when clicked
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            //if the hit, hit something this will continue
            if (hit.collider != null)
            {
                //for everytile that is generated
                for (int x = 0; x < _cellGrid.GetLength(0); x++)
                {
                    for (int y = 0; y < _cellGrid.GetLength(1); y++)
                    {
                        //the hit will get the ganmeobject and get the selected position in the active arry
                        if (hit.collider.gameObject == _cellGrid[x, y].gameObject)
                        {
                            //if the cell is not active
                            if (_activeCells[x, y] == 0)
                            {
                                //the cell will be activated
                                _activeCells[x, y] = 1;
                                //the sprite of the cell is set to the active state
                                CalculateGrid();
                            }
                        }
                    }
                }
            }
        }

        //if the game is active the timer will add to the count
        if (_isGameActive)
            _timer += Time.deltaTime;

        //the grid will update every time when the timer has reached the target time
        while (_timer >= _maxTime)
        {
            _timer = 0;
            CheckStates();
            CalculateGrid();
        }

        //A debugging command to let the grid generate and calculate 1 update at a time
        if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl))
        {
            CheckStates();
            CalculateGrid();
        }
    }

    [SerializeField] private Button _playButton;

    public void CalculateGrid()
    {
        //for the width and the height it will loop through the amount of cells the width and height is
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float seperationDistance = 0.9f;
                if (_cellGrid[x, y] != null)
                {
                    //Gets the sprite renderer of the cell of the right cells
                    SpriteRenderer spriteShow = _cellGrid[x, y].GetComponent<SpriteRenderer>();

                    if (_activeCells[x, y] == 1)
                    {
                        //Sets the sprite of the cell to the alive state
                        _cellGrid[x, y].gameObject.GetComponent<SpriteRenderer>().sprite = _aliveCell;
                    }
                    else
                    {
                        //Sets the sprite of the cell to the dead state
                        _cellGrid[x, y].gameObject.GetComponent<SpriteRenderer>().sprite = _deadCell;
                    }
                }
                else
                {
                    //For each item in the array lenght it will generate a gameobject on each slot
                    //The position for each cell will be set to a defferent position 
                    _cellGrid[x, y] = GameObject.Instantiate(_cell, new Vector3(_startPosition.transform.position.x + seperationDistance, _startPosition.transform.position.y * seperationDistance) + new Vector3(x * 0.5f, y * 0.5f), Quaternion.identity);

                    //gives every tile its own coordinate as name
                    _cellGrid[x, y].name = x + "," + y;
                }
            }
        }
    }

    //the play button will call this function
    public void GameState(bool state)
    {
        //sets the gamestate to an active or deactive state
        _isGameActive = state;
    }

    public void CheckStates()
    {
        //for each tile in the entire grid
        for (int x = 0; x < _cellGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _cellGrid.GetLength(1); y++)
            {
                //the count of the neighbours
                int neighbourCount = 0;
                //for all the choises around the selected tile
                for (int i = 0; i < _optionArrayX.Length; i++)
                {
                    //sets all the different x and y offsets aroudn the tile that needs to be checked in a int
                    int offsetX = _optionArrayX[i];
                    int offsetY = _optionArrayY[i];
                    //if both offsets are 0 you are in the middle tile and will skip the entire next check
                    if (offsetX == 0 && offsetY == 0)
                        continue;
                    //the x and y offsets are checked with de width and height to check if the specific tile is in the grid
                    if (x + offsetX >= 0 && x + offsetX < _width && y + offsetY >= 0 && y + offsetY < _height)
                    {
                        //the current cell with be checked if the cell is active or not
                        int v = _activeCells[x + offsetX, y + offsetY];
                        //if the cell is active execute
                        if (v == 1)
                        {
                            //ads 1 neighbour to the count
                            neighbourCount++;
                        }
                    }
                }

                //sets the array of active cells to the buffer array with wich cells are active and wich not
                _activeCells2[x, y] = _activeCells[x, y];
                //if the cell has less that 2 or more than 3 neighbours the cell dies
                if (neighbourCount < 2 || neighbourCount > 3)
                {
                    _activeCells2[x, y] = 0;
                }
                //Sets the rules if the cell has 3 neighbours and is not alive it will be born
                else if (neighbourCount == 3 && _activeCells[x, y] == 0)
                {
                    _activeCells2[x, y] = 1;
                }
            }
        }
        (_activeCells, _activeCells2) = (_activeCells2, _activeCells);
    }

    public void ReloadGrid()
    {
        //The scene will reload when the relaod button is pressed so the level will reload
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }


    public void RandomCells()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                int random = Random.Range(0, 99);

                if (random <= 20)
                {
                    _activeCells[i, j] = 1;
                }
                else
                {
                    _activeCells[i, j] = 0;
                }
            }
        }
        CalculateGrid();
    }
}
