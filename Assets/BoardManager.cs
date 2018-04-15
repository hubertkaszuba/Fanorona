using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }
    private bool[,] forcedMoves { set; get; }
    public bool[,] previousPositions { set; get; }
    private int[] nextBeat = new int[16];
    public bool next = false;
    public bool move = false;
    public Pawn[,] Pawns { set; get; }
    private Pawn selectedPawn;
    public int countMove = 0;
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public int hasX = -1;
    public int hasY = -1;

    public List<GameObject> draughtsmanPrefabs;
    private List<GameObject> activeDraughtsman;
    private Quaternion orientation = Quaternion.Euler(0, 180, 0);

    public bool isWhiteTurn = true;
    public bool isFirstRound = true;
    public bool didBeat = false;
    public bool hasToBeat = false;
    public bool checkedForceToMove = false;
    public List<Pawn> forcedToMove;
    public bool hasMoved = false;
    public bool didMove = false;
    int moveX = -1;
    int moveY = -1;
    private void Start()
    {
        Instance = this;
        SpawnAll();
        CleanPreviousPositions();
    }

    private void Update()
    {
        
        DrawBoard();

        if (isWaitForMouseClickIsRunning == false)
            UpdateSelection();

        if (countMove < 3)
            isFirstRound = false;



        if (isFirstRound)
        {
            CleanPreviousPositions();           
        }
        else
        {
            if(didBeat && !hasMoved)
            {
                isWhiteTurn = !isWhiteTurn;
                if(!NextBeating(hasX, hasY))
                {
                    isWhiteTurn = !isWhiteTurn;
                    BoardHighlights.Instance.HideHighlights();
                    selectedPawn = Pawns[hasX, hasY];
                    if (selectedPawn.isWhite)
                        selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture", typeof(Material)) as Material;
                    else
                        selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture", typeof(Material)) as Material;
                    selectedPawn = null;
                    didBeat = false;
                    hasToBeat = false;
                    forcedToMove.Clear();
                    CleanPreviousPositions();
                    checkedForceToMove = false;
                    return;
                }
                else if(NextBeating(hasX, hasY))
                {
                    previousPositions[hasX, hasY] = true;
                    addToForcedMoves(hasX, hasY);
                    AddPreviousPositions();
                    allowedMoves = forcedMoves;
                    BoardHighlights.Instance.HighlightPastMoves(previousPositions);
                    if(CanMove())
                    {
                        forcedToMove.Clear();
                        forcedToMove.Add(selectedPawn);
                        selectedPawn = Pawns[hasX, hasY];
                        if (selectedPawn.isWhite)
                            selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture-transparent-green", typeof(Material)) as Material; 
                        else
                            selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture-transparent-red", typeof(Material)) as Material;
                        BoardHighlights.Instance.HighlightAllowedMoves(forcedMoves);
                        didBeat = false;
                        hasToBeat = true;
                    }
                    else
                    {
                        isWhiteTurn = !isWhiteTurn;
                        BoardHighlights.Instance.HideHighlights();
                        selectedPawn = Pawns[hasX, hasY];
                        if (selectedPawn.isWhite)
                            selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture", typeof(Material)) as Material;
                        else
                            selectedPawn.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture", typeof(Material)) as Material;
                        selectedPawn = null;
                        didBeat = false;
                        hasToBeat = false;
                        forcedToMove.Clear();
                        CleanPreviousPositions();
                        checkedForceToMove = false;
                        return;
                    }
                }
            }
        }

        if (!checkedForceToMove)
            checkForceToMove();



        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && (!hasMoved || isWaitForMouseClickIsRunning))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedPawn == null && forcedToMove.Exists(p => p.CurrentX == selectionX && p.CurrentY == selectionY) && hasToBeat)
                {
                    previousPositions[hasX, hasY] = true;
                    SelectPawn(hasX, hasY);
                    didMove = false;
                }
                else if (selectedPawn != null && forcedMoves[selectionX, selectionY] == true)
                {
                    StartCoroutine(MovePawn(selectionX, selectionY));

                }
                else if (selectedPawn != null && forcedToMove.Exists(p => p.CurrentX == selectionX && p.CurrentY == selectionY) && !hasToBeat)
                {
                    CleanPreviousPositions();
                    BoardHighlights.Instance.HideHighlights();
                    previousPositions[selectionX, selectionY] = true;                  
                    SelectPawn(selectionX, selectionY);
                    didMove = false;
                }
                else if (selectedPawn == null && forcedToMove.Exists(p => p.CurrentX == selectionX && p.CurrentY == selectionY) && !hasToBeat)
                {
                    CleanPreviousPositions();
                    previousPositions[selectionX, selectionY] = true;
                    SelectPawn(selectionX, selectionY);
                    didMove = false;
                }
                else if (selectedPawn != null && !forcedToMove.Exists(p => p))
                {
                    if (!Pawns[selectionX, selectionY])
                    {
                        StartCoroutine(MovePawn(selectionX, selectionY));
                    }
                    else
                    {
                        forcedToMove.Clear();
                        CleanPreviousPositions();
                        BoardHighlights.Instance.HideHighlights();
                        SelectPawn(selectionX, selectionY);
                        didMove = false;
                    }
                }
                else if (selectedPawn == null && !forcedToMove.Exists(p => p))
                {
                    BoardHighlights.Instance.HideHighlights();
                    SelectPawn(selectionX, selectionY);
                    didMove = false;
                }

            }
        }
        if (hasMoved && !isWaitForMouseClickIsRunning)
        {
            StartCoroutine(transformPosition(moveX, moveY));
            if(endMove)
            {
                
                Pawns[selectedPawn.CurrentX, selectedPawn.CurrentY] = null;
                selectedPawn.setPosition(moveX, moveY);
                Pawns[moveX, moveY] = selectedPawn;
                selectedPawn = null;
                checkedForceToMove = false;
                hasMoved = false;
                endMove = false;
                isWhiteTurn = !isWhiteTurn;
            }
        }
        //BoardHighlights.Instance.HideHighlights();
        //selectedPawn = null;
    }

    public void AddPreviousPositions()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 5; j++)
            {
                if (forcedMoves[i, j] == true && previousPositions[i, j] == true)
                    forcedMoves[i, j] = false;
            }
    }
    public void CleanPreviousPositions()
    {
        bool[,] r = new bool[9, 5];
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 5; j++)
                r[i, j] = false;
        previousPositions = r;
    }

    public bool CanMove()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 5; j++)
                if (allowedMoves[i, j] == true)
                    return true;
        return false;
    }

    private void SpawnBlack(int index, int x, int y)
    {
        GameObject go = Instantiate(draughtsmanPrefabs[index], GetTileCenter(x, y), Quaternion.identity) as GameObject;
        go.transform.SetParent(transform);
        go.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture", typeof(Material)) as Material;
        Pawns[x, y] = go.GetComponent<Pawn>();
        Pawns[x, y].setPosition(x, y);
        activeDraughtsman.Add(go);
    }

    private void SpawnWhite(int index, int x, int y)
    {
        GameObject go = Instantiate(draughtsmanPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
        go.transform.SetParent(transform);
        go.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture", typeof(Material)) as Material;
        Pawns[x, y] = go.GetComponent<Pawn>();
        Pawns[x, y].setPosition(x, y);
        activeDraughtsman.Add(go);
    }

    private void SpawnAll()
    {
        activeDraughtsman = new List<GameObject>();
        Pawns = new Pawn[9, 5];
        SpawnWhite(1, 0, 0);
        SpawnWhite(1, 1, 0);
        SpawnWhite(1, 2, 0);
        SpawnWhite(1, 3, 0);
        SpawnWhite(1, 4, 0);
        SpawnWhite(1, 5, 0);
        SpawnWhite(1, 6, 0);
        SpawnWhite(1, 7, 0);
        SpawnWhite(1, 8, 0);
        SpawnWhite(1, 0, 1);
        SpawnWhite(1, 1, 1);
        SpawnWhite(1, 2, 1);
        SpawnWhite(1, 3, 1);
        SpawnWhite(1, 4, 1);
        SpawnWhite(1, 5, 1);
        SpawnWhite(1, 6, 1);
        SpawnWhite(1, 7, 1);
        SpawnWhite(1, 8, 1);
        SpawnWhite(1, 1, 2);
        SpawnWhite(1, 3, 2);
        SpawnWhite(1, 6, 2);
        SpawnWhite(1, 8, 2);

        SpawnBlack(0, 0, 2);
        SpawnBlack(0, 2, 2);
        SpawnBlack(0, 5, 2);
        SpawnBlack(0, 7, 2);
        SpawnBlack(0, 0, 3);
        SpawnBlack(0, 1, 3);
        SpawnBlack(0, 2, 3);
        SpawnBlack(0, 3, 3);
        SpawnBlack(0, 4, 3);
        SpawnBlack(0, 5, 3);
        SpawnBlack(0, 6, 3);
        SpawnBlack(0, 7, 3);
        SpawnBlack(0, 8, 3);
        SpawnBlack(0, 0, 4);
        SpawnBlack(0, 1, 4);
        SpawnBlack(0, 2, 4);
        SpawnBlack(0, 3, 4);
        SpawnBlack(0, 4, 4);
        SpawnBlack(0, 5, 4);
        SpawnBlack(0, 6, 4);
        SpawnBlack(0, 7, 4);
        SpawnBlack(0, 8, 4);



    }

    public void checkForceToMove()
    {
        if (hasToBeat)
        {
            forcedToMove.RemoveAll(p => p);
            forcedToMove.Add(Pawns[hasX, hasY]);
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (Pawns[i, j] != null && Pawns[i, j].isWhite == isWhiteTurn)
                    {
                        if (NextBeating(i, j))
                        {
                            if (Pawns[i, j].isWhite)
                                Pawns[i, j].gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture-transparent-green", typeof(Material)) as Material;
                            else
                                Pawns[i, j].gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture-transparent-red", typeof(Material)) as Material;
                            forcedToMove.Add(Pawns[i, j]);
                        }
                    }
                }
            }
        }
        checkedForceToMove = true;
    }

    private void SelectPawn(int x, int y)
    {
        if (Pawns[x, y] == null)
            return;
        if (Pawns[x, y].isWhite != isWhiteTurn)
            return;
        if (NextBeating(x, y))
        {
            addToForcedMoves(x, y);
            allowedMoves = forcedMoves;
            selectedPawn = Pawns[x, y];
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
        else
        {
            allowedMoves = Pawns[x, y].PossibleMove();
            selectedPawn = Pawns[x, y];
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
    }

    public bool isWaitForMouseClickIsRunning = false;
    public IEnumerator WaitForMouseClick(int x, int y)
    {
        while (true)
        {
            isWaitForMouseClickIsRunning = true;
            if (Input.GetMouseButtonDown(1))
            {

                isWaitForMouseClickIsRunning = false;
                yield break;
            }
            yield return null;
        }
    }

    public bool NextBeating(int x, int y)
    {
        allowedMoves = Pawns[x, y].PossibleMove();
        bool[,] f = new bool[9, 5];
        int flag = 0;
        if (y < 3)
        {
            if (Pawns[x, y + 2] != null && Pawns[x, y + 2].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    nextBeat[0]++;
                }
                flag = 1;
                f[x, y + 1] = true;
            }
        }
        if (y < 3 && x < 7 && allowedMoves[x + 1, y + 1])
        {
            if (Pawns[x + 2, y + 2] != null && Pawns[x + 2, y + 2].isWhite != isWhiteTurn && Pawns[x + 1, y + 1] == null)
            {
                int pomx = x + 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    pomy++;
                    nextBeat[1]++;
                }
                flag = 1;
                f[x + 1, y + 1] = true;
            }
        }
        if (y < 3 && x > 1 && allowedMoves[x - 1, y + 1])
        {
            if (Pawns[x - 2, y + 2] != null && Pawns[x - 2, y + 2].isWhite != isWhiteTurn && Pawns[x - 1, y + 1] == null)
            {
                int pomx = x - 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    pomy++;
                    nextBeat[2]++;
                }
                flag = 1;
                f[x - 1, y + 1] = true;
            }
        }
        if (y > 1 && x > 1 && allowedMoves[x - 1, y - 1])
        {
            if (Pawns[x - 2, y - 2] != null && Pawns[x - 2, y - 2].isWhite != isWhiteTurn && Pawns[x - 1, y - 1] == null)
            {
                int pomx = x - 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    pomy--;
                    nextBeat[3]++;
                }
                flag = 1;
                f[x - 1, y - 1] = true;
            }
        }
        if (y > 1 && x < 7 && allowedMoves[x + 1, y - 1])
        {
            if (Pawns[x + 2, y - 2] != null && Pawns[x + 2, y - 2].isWhite != isWhiteTurn && Pawns[x + 1, y - 1] == null)
            {
                int pomx = x + 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    pomy--;
                    nextBeat[4]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 1)
        {
            if (Pawns[x, y - 2] != null && Pawns[x, y - 2].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[5]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 1)
        {
            if (Pawns[x - 2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x < 7)
        {
            if (Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[7]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        if (y > 0 && y < 4)
        {
            if (Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[8]++;
                }
                flag = 1;
                f[x, y + 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y + 1])
        {
            if (Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[x + 1, y + 1] == null)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    pomx--;
                    nextBeat[9]++;
                }
                flag = 1;
                f[x + 1, y + 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x - 1, y + 1])
        {
            if (Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn && Pawns[x - 1, y + 1] == null)
            {
                int pomx = x + 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    pomx++;
                    nextBeat[10]++;
                }
                flag = 1;
                f[x - 1, y + 1] = true;
            }
        }

        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x - 1, y - 1])
        {
            if (Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn && Pawns[x - 1, y - 1] == null)
            {
                int pomx = x + 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx++;
                    nextBeat[11]++;
                }
                flag = 1;
                f[x - 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y - 1])
        {
            if (Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn && Pawns[x + 1, y - 1] == null)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx--;
                    nextBeat[12]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4)
        {
            if (Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    nextBeat[13]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 0 && x < 8)
        {
            if (Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[14]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x > 0 && x < 8)
        {
            if (Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[15]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        
        if (flag == 0)
            return false;
        else
            return true;
    }

    public bool addToForcedMoves(int x, int y)
    {
        allowedMoves = Pawns[x, y].PossibleMove();
        bool[,] f = new bool[9, 5];
        int flag = 0;
        if (y < 3)
        {
            if (Pawns[x, y + 2] != null && Pawns[x, y + 2].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    nextBeat[0]++;
                }
                flag = 1;
                f[x, y + 1] = true;
            }
        }
        if (y < 3 && x < 7 && allowedMoves[x + 1, y + 1])
        {
            if (Pawns[x + 2, y + 2] != null && Pawns[x + 2, y + 2].isWhite != isWhiteTurn && Pawns[x + 1, y + 1] == null)
            {
                int pomx = x + 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    pomy++;
                    nextBeat[1]++;
                }
                flag = 1;
                f[x + 1, y + 1] = true;
            }
        }
        if (y < 3 && x > 1 && allowedMoves[x - 1, y + 1])
        {
            if (Pawns[x - 2, y + 2] != null && Pawns[x - 2, y + 2].isWhite != isWhiteTurn && Pawns[x - 1, y + 1] == null)
            {
                int pomx = x - 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    pomy++;
                    nextBeat[2]++;
                }
                flag = 1;
                f[x - 1, y + 1] = true;
            }
        }
        if (y > 1 && x > 1 && allowedMoves[x - 1, y - 1])
        {
            if (Pawns[x - 2, y - 2] != null && Pawns[x - 2, y - 2].isWhite != isWhiteTurn && Pawns[x - 1, y - 1] == null)
            {
                int pomx = x - 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    pomy--;
                    nextBeat[3]++;
                }
                flag = 1;
                f[x - 1, y - 1] = true;
            }
        }
        if (y > 1 && x < 7 && allowedMoves[x + 1, y - 1])
        {
            if (Pawns[x + 2, y - 2] != null && Pawns[x + 2, y - 2].isWhite != isWhiteTurn && Pawns[x + 1, y - 1] == null)
            {
                int pomx = x + 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    pomy--;
                    nextBeat[4]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 1)
        {
            if (Pawns[x, y - 2] != null && Pawns[x, y - 2].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[5]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 1)
        {
            if (Pawns[x - 2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x < 7)
        {
            if (Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[7]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        if (y > 0 && y < 4)
        {
            if (Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[8]++;
                }
                flag = 1;
                f[x, y + 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y + 1])
        {
            if (Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[x + 1, y + 1] == null)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    pomx--;
                    nextBeat[9]++;
                }
                flag = 1;
                f[x + 1, y + 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x - 1, y + 1])
        {
            if (Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn && Pawns[x - 1, y + 1] == null)
            {
                int pomx = x + 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    pomx++;
                    nextBeat[10]++;
                }
                flag = 1;
                f[x - 1, y + 1] = true;
            }
        }

        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x - 1, y - 1])
        {
            if (Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn && Pawns[x - 1, y - 1] == null)
            {
                int pomx = x + 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx++;
                    nextBeat[11]++;
                }
                flag = 1;
                f[x - 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y - 1])
        {
            if (Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn && Pawns[x + 1, y - 1] == null)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx--;
                    nextBeat[12]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4)
        {
            if (Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    nextBeat[13]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 0 && x < 8)
        {
            if (Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[14]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x > 0 && x < 8)
        {
            if (Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[15]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        forcedMoves = f;
        if (flag == 0)
            return false;
        else
            return true;
    }



    private IEnumerator MovePawn(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            didMove = true;
            if (selectedPawn.CurrentX == x && selectedPawn.CurrentY < y) //Forward White
            {
                if (y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentY != 4 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn && Pawns[x, selectedPawn.CurrentY - 1] != null && Pawns[x, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x == selectedPawn.CurrentX && (int)hit.point.z != selectedPawn.CurrentY)
                    {
                        if ((int)hit.point.z > selectedPawn.CurrentY)
                        {
                            int pomx = x;
                            int pomy = y + 1;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x;
                            int pomy = y - 2;
                            while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy--;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (y + 1 != 5 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn)
                {
                    int pomx = x;
                    int pomy = y + 1;
                    while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy++;
                    }
                    didBeat = true;
                }
                else if (y - 2 > -1 && Pawns[x, y - 2] != null && Pawns[x, y - 2].isWhite != isWhiteTurn)
                {
                    int pomx = x;
                    int pomy = y - 2;
                    while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy--;
                    }
                    didBeat = true;
                }

            }

            if (selectedPawn.CurrentX < x && selectedPawn.CurrentY < y) //Diagonal Forward Right
            {
                if (x + 1 != 9 && y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY - 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x > selectedPawn.CurrentX)
                        {
                            int pomx = x + 1;
                            int pomy = y + 1;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy++;
                                pomx++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x - 2;
                            int pomy = y - 2;
                            while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy--;
                                pomx--;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x + 1 != 9 && y + 1 != 5 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn)
                {
                    int pomx = x + 1;
                    int pomy = y + 1;
                    while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx++;
                        pomy++;
                    }
                    didBeat = true;
                }
                else if (x - 2 > -1 && y - 2 > -1 && Pawns[x - 2, y - 2] != null && Pawns[x - 2, y - 2].isWhite != isWhiteTurn)
                {
                    int pomx = x - 2;
                    int pomy = y - 2;
                    while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy--;
                        pomx--;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX > x && selectedPawn.CurrentY < y) //Diagonal Forward Left
            {
                if (x - 1 != -1 && y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 4 && Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY - 1] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x < selectedPawn.CurrentX)
                        {
                            int pomx = x - 1;
                            int pomy = y + 1;
                            while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy++;
                                pomx--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x + 2;
                            int pomy = y - 2;
                            while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy--;
                                pomx++;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x - 1 != -1 && y + 1 != 5 && Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn)
                {
                    int pomx = x - 1;
                    int pomy = y + 1;
                    while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx--;
                        pomy++;
                    }
                    didBeat = true;

                }
                else if (x + 2 < 9 && y - 2 > -1 && Pawns[x + 2, y - 2] != null && Pawns[x + 2, y - 2].isWhite != isWhiteTurn)
                {
                    int pomx = x + 2;
                    int pomy = y - 2;
                    while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy--;
                        pomx++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX < x && selectedPawn.CurrentY == y) //Right
            {
                if (x + 1 != 9 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentX != 8 && Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x > selectedPawn.CurrentX)
                        {
                            int pomx = x + 1;
                            int pomy = y;
                            while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x - 2;
                            int pomy = y;
                            while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx--;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x + 1 != 9 && Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn)
                {
                    int pomx = x + 1;
                    int pomy = y;
                    while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx++;
                    }
                    didBeat = true;
                }
                else if (x - 2 > -1 && Pawns[x - 2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn)
                {
                    int pomx = x - 2;
                    int pomy = y;
                    while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx--;
                    }
                    didBeat = true;
                }

            }

            if (selectedPawn.CurrentX > x && selectedPawn.CurrentY == y)
            {
                if (x - 1 != -1 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentX != 0 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x < selectedPawn.CurrentX)
                        {
                            int pomx = x - 1;
                            int pomy = y;
                            while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x + 2;
                            int pomy = y;
                            while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx++;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x - 1 != -1 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn)
                {
                    int pomx = x - 1;
                    int pomy = y;
                    while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx--;
                    }
                    didBeat = true;
                }
                else if (x + 2 < 9 && Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn)
                {
                    int pomx = x + 2;
                    int pomy = y;
                    while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX < x && selectedPawn.CurrentY > y)
            {
                if (x + 1 != 9 && y - 1 != -1 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 4 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x > selectedPawn.CurrentX)
                        {
                            int pomx = x + 1;
                            int pomy = y - 1;
                            while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx++;
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x - 2;
                            int pomy = y + 2;
                            while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx--;
                                pomy++;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x + 1 != 9 && y - 1 != -1 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn)
                {
                    int pomx = x + 1;
                    int pomy = y - 1;
                    while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx++;
                        pomy--;
                    }
                    didBeat = true;
                }
                else if (x - 2 > -1 && y + 2 < 5 && Pawns[x - 2, y + 2] != null && Pawns[x - 2, y + 2].isWhite != isWhiteTurn)
                {
                    int pomx = x - 2;
                    int pomy = y + 2;
                    while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx--;
                        pomy++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX > x && selectedPawn.CurrentY > y)
            {
                if (x - 1 != 9 && y - 1 != -1 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.x != selectedPawn.CurrentX)
                    {
                        if ((int)hit.point.x < selectedPawn.CurrentX)
                        {
                            int pomx = x - 1;
                            int pomy = y - 1;
                            while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx--;
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x + 2;
                            int pomy = y + 2;
                            while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomx++;
                                pomy++;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (x - 1 != -1 && y - 1 != -1 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn)
                {
                    int pomx = x - 1;
                    int pomy = y - 1;
                    while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx--;
                        pomy--;
                    }
                    didBeat = true;
                }
                else if (x + 2 < 9 && y + 2 < 5 && Pawns[x + 2, y + 2] != null && Pawns[x + 2, y + 2].isWhite != isWhiteTurn)
                {
                    int pomx = x + 2;
                    int pomy = y + 2;
                    while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomx++;
                        pomy++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX == x && selectedPawn.CurrentY > y)
            {
                if (y - 1 != -1 && selectedPawn.CurrentY != 4 && selectedPawn.CurrentY != 0 && Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, selectedPawn.CurrentY + 1] != null && Pawns[x, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
                {

                    yield return StartCoroutine(WaitForMouseClick(x, y));
                    RaycastHit hit;
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"));
                    if ((int)hit.point.z != selectedPawn.CurrentY)
                    {
                        if ((int)hit.point.z < selectedPawn.CurrentY)
                        {
                            int pomx = x;
                            int pomy = y - 1;
                            while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            int pomx = x;
                            int pomy = y + 2;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                pomy++;
                            }
                            didBeat = true;
                        }
                    }

                }
                else if (y - 1 != -1 && Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn)
                {
                    int pomx = x;
                    int pomy = y - 1;
                    while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy--;
                    }
                    didBeat = true;
                }
                else if (y + 2 < 5 && Pawns[x, y + 2] != null && Pawns[x, y + 2].isWhite != isWhiteTurn)
                {
                    int pomx = x;
                    int pomy = y + 2;
                    while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                    {

                        activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                        Destroy(Pawns[pomx, pomy].gameObject);
                        pomy++;
                    }
                    didBeat = true;
                }
            }



            
            

            BoardHighlights.Instance.HideHighlights();            
            if (countMove < 4)
                countMove++;


            

            if (didBeat)
            {
                hasX = x;
                hasY = y;
            }

            hasMoved = true;
            moveX = x;
            moveY = y;
            if (!isWhiteTurn)
            {
                forcedToMove.ForEach(p => p.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/dark-wood-texture", typeof(Material)) as Material);
                forcedToMove.RemoveAll(p => p);
            }
            else
            {
                forcedToMove.ForEach(p => p.gameObject.GetComponent<Renderer>().material = Resources.Load("Materials/light-wood-texture", typeof(Material)) as Material);
                forcedToMove.RemoveAll(p => p);
            }

        }
    }
    public bool endMove = false;
    public IEnumerator transformPosition(int x, int y)
    {
        Vector3 velocity = Vector3.zero;
        var targetposition = GetTileCenter(x, y);
        Vector3 newPosition;
        while (true)
        {
            newPosition = Vector3.Lerp(selectedPawn.transform.position, GetTileCenter(x, y), 0.05f);
            if (newPosition == selectedPawn.transform.position)
            {

                selectedPawn.transform.position = newPosition;
                endMove = true;
                yield break;
            }

            selectedPawn.transform.position = newPosition;
            endMove = false;
            yield return null;
        }
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }


    private void DrawBoard()
    {
        Vector3 widthLine = Vector3.right * 9;
        Vector3 heightLine = Vector3.forward * 5;

        for (int i = 0; i <= 5; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 9; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        Debug.DrawLine(
            Vector3.forward * 2 + Vector3.right * 0,
            Vector3.forward * 5 + Vector3.right * 3);

        Debug.DrawLine(
            Vector3.forward * 0 + Vector3.right * 0,
            Vector3.forward * 5 + Vector3.right * 5);

        Debug.DrawLine(
            Vector3.forward * 0 + Vector3.right * 2,
            Vector3.forward * 5 + Vector3.right * 7);

        Debug.DrawLine(
            Vector3.forward * 0 + Vector3.right * 4,
            Vector3.forward * 5 + Vector3.right * 9);

        Debug.DrawLine(
            Vector3.forward * 0 + Vector3.right * 6,
            Vector3.forward * 3 + Vector3.right * 9);


        Debug.DrawLine(
            Vector3.forward * 3 + Vector3.right * 0,
            Vector3.forward * 0 + Vector3.right * 3);

        Debug.DrawLine(
            Vector3.forward * 5 + Vector3.right * 0,
            Vector3.forward * 0 + Vector3.right * 5);

        Debug.DrawLine(
            Vector3.forward * 5 + Vector3.right * 2,
            Vector3.forward * 0 + Vector3.right * 7);

        Debug.DrawLine(
            Vector3.forward * 5 + Vector3.right * 4,
            Vector3.forward * 0 + Vector3.right * 9);

        Debug.DrawLine(
            Vector3.forward * 3 + Vector3.right * 6,
            Vector3.forward * 0 + Vector3.right * 9);

        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }








}