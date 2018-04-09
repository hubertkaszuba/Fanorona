using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    public static BoardManager Instance { set; get; }
    private bool [,] allowedMoves { set; get; }
    private int[] nextBeat = new int[8];
    public bool next = false;
    public bool move = false;
    public Pawn [,] Pawns { set; get; }
    private Pawn selectedPawn;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> draughtsmanPrefabs;
    private List<GameObject> activeDraughtsman;
    private Quaternion orientation = Quaternion.Euler(0, 180, 0);

    public bool isWhiteTurn = true;
    public bool isFirstRound = true;
    public bool didBeat = false;
    private void Start()
    {
        Instance = this;
        SpawnAll();
    }

    private void Update()
    {
            if(didBeat)
            {
                isWhiteTurn = !isWhiteTurn;
                selectedPawn = Pawns[selectionX, selectionY];
                if (!NextBeating(selectedPawn.CurrentX, selectedPawn.CurrentY))
                {
                    isWhiteTurn = !isWhiteTurn;
                    BoardHighlights.Instance.HideHighlights();
                    selectedPawn = null;
                    didBeat = false;
                    return;
                }
                else if (NextBeating(selectedPawn.CurrentX, selectedPawn.CurrentY))
                {                  
                    allowedMoves = Pawns[selectedPawn.CurrentX, selectedPawn.CurrentY].PossibleMove();
                    BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
                    didBeat = false;
                }

            }

        if (isWaitForMouseClickIsRunning == false)
                UpdateSelection();
            DrawBoard();
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (selectionX >= 0 && selectionY >= 0)
                {
                    if (selectedPawn == null)
                    {
                        SelectPawn(selectionX, selectionY);
                    }
                    else
                    {                       
                        StartCoroutine(MovePawn(selectionX, selectionY));
                    }
                }
            }

    }



    private void SelectPawn(int x, int y)
    {
        if (Pawns[x, y] == null)
            return;
        if (Pawns[x, y].isWhite != isWhiteTurn)
            return;
        allowedMoves = Pawns[x, y].PossibleMove();
        selectedPawn = Pawns[x, y];
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
    }

    public bool isWaitForMouseClickIsRunning = false;
    public IEnumerator WaitForMouseClick(int x, int y)
    {
        while(true)
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
        int flag = 0;
        if(y < 3)
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
            }
        }
        if (y < 3 && x < 7 && allowedMoves[x+1,y+1])
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
            }
        }
        if (x > 1)
        {
            if (Pawns[x - 2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
            }
        }
        if (x < 7)
        {
            if (Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn && Pawns[x + 2, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[6]++;
                }
                flag = 1;
            }
        }
        if(y > 0 && y < 4)
        {
            if (Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[6]++;
                }
                flag = 1;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y + 1])
        {
            if (Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[x + 1, y + 1] == null)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx > - 1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
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
                    nextBeat[6]++;
                }
                flag = 1;
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
                    nextBeat[6]++;
                }
                flag = 1;
            }
        }
        if (y > 0 && y < 4 && x > 0 && x < 8 && allowedMoves[x + 1, y - 1])
        {
            if (Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn && Pawns[x + 1, y - 1] == null)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx > - 1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
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
                    nextBeat[6]++;
                }
                flag = 1;
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
                    nextBeat[6]++;
                }
                flag = 1;
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
                    nextBeat[6]++;
                }
                flag = 1;
            }
        }
        if (flag == 0)
           return false;
        else
           return true;
    }


    
    private IEnumerator MovePawn(int x, int y)
    {
        int pomx2 = x;
        int pomy2 = y;
        if (allowedMoves[x,y])
        {
                if (selectedPawn.CurrentX == x && selectedPawn.CurrentY < y) //Forward White
                {
                    if (y+1 != 5 && selectedPawn.CurrentY != 0 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn && Pawns[x, selectedPawn.CurrentY - 1] != null && Pawns[x, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
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
                    else if (y+1 != 5 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn)
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
                    else if (y - 2 > -1 && Pawns[x,y-2] != null && Pawns[x, y-2].isWhite != isWhiteTurn)
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
                    if (x+1 != 9 && y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 0 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX -1, selectedPawn.CurrentY - 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
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
                                while (pomy > -1 && pomx > - 1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
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
                    else if (x+1 != 9 && y+1 != 5 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn)
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
                    else if(x-2 > -1 && y - 2 > -1 && Pawns[x - 2, y - 2] != null && Pawns[x - 2, y - 2].isWhite != isWhiteTurn)
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
                    if (x - 1 != 0 && y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 8 && Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY - 1] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
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
                    if (x + 1 != 9 && selectedPawn.CurrentX != 0 && Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY].isWhite != isWhiteTurn)
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
                                while (pomx > - 1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                                {

                                    activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                    Destroy(Pawns[pomx, pomy].gameObject);
                                    pomx--;
                                }
                                didBeat = true;
                            }
                        }

                    }
                    else if (x + 1 != 9 &&  Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn)
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
                    else if(x - 2 > -1 && Pawns[x -2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn)
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
                    if (x - 1 != -1 && selectedPawn.CurrentX != 8 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY].isWhite != isWhiteTurn)
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
                    else if(x + 2 < 9 && Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn)
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
                    if (x + 1 != 9 && y - 1 !=  -1 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 0 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
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
                                while (pomx > - 1 && pomy < 5  && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
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
                    else if(x - 2 > -1 && y + 2 < 5 && Pawns[x - 2, y + 2] != null && Pawns[x - 2, y + 2].isWhite != isWhiteTurn)
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
                    if (x - 1 != 9 && y - 1 != -1 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 0 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
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
                    else if(x + 2 < 9 && y + 2 < 5 && Pawns[x + 2, y + 2] != null && Pawns[x + 2, y + 2].isWhite != isWhiteTurn)
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
                    if (y - 1 != -1 && selectedPawn.CurrentY != 4 && Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, selectedPawn.CurrentY + 1] != null && Pawns[x, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
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
            
           


                Pawns[selectedPawn.CurrentX, selectedPawn.CurrentY] = null;
                selectedPawn.transform.position = GetTileCenter(x, y);
                selectedPawn.setPosition(x, y);
                Pawns[x, y] = selectedPawn;

            isWhiteTurn = !isWhiteTurn;
            if(selectedPawn.CurrentX != x || selectedPawn.CurrentY != y)

            if (isFirstRound == true && !isWhiteTurn)
                isFirstRound = false;
             
        }

        BoardHighlights.Instance.HideHighlights();
        selectedPawn = null;
    }
    private void SpawnBlack(int index, int x, int y)
    {
        GameObject go = Instantiate(draughtsmanPrefabs[index], GetTileCenter(x,y), Quaternion.identity) as GameObject;
        go.transform.SetParent(transform);
        go.GetComponent<Renderer>().material.color = Color.black;
        Pawns[x, y] = go.GetComponent<Pawn>();
        Pawns[x, y].setPosition(x, y);
        activeDraughtsman.Add(go);
    }

    private void SpawnWhite(int index, int x, int y)
    {
        GameObject go = Instantiate(draughtsmanPrefabs[index], GetTileCenter(x,y), orientation) as GameObject;
        go.transform.SetParent(transform);
        go.GetComponent<Renderer>().material.color = Color.white;
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
       /* SpawnWhite(1, 0, 1);
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
        SpawnWhite(1, 8, 2);*/

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

        for(int i = 0; i <= 5; i++)
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
