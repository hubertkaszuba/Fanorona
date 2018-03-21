using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    public static BoardManager Instance { set; get; }
    private bool [,] allowedMoves { set; get; }



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


    private void Start()
    {
        Instance = this;
        SpawnAll();
    }

    private void Update()
    {
        UpdateSelection();
        DrawBoard();
        if(Input.GetMouseButtonDown(0))
        {
            if(selectionX >= 0 && selectionY >= 0)
            {
                if(selectedPawn == null)
                {
                    SelectPawn(selectionX, selectionY);
                }
                else
                {
                    MovePawn(selectionX, selectionY);
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

    private void MovePawn(int x, int y)
    {
        if(allowedMoves[x,y])
        {
            if (selectedPawn.isWhite)
            {
                if (selectedPawn.CurrentX == x && selectedPawn.CurrentY < y)
                {

                    if (y+1 != 5 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x;
                        int pomy = y + 1;
                        while (pomy < 5 && Pawns[pomx, pomy] != null && !Pawns[pomx,pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomy++;
                        }
                    }

                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY < y)
                {
                    if (x+1 != 9 && y+1 != 5 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y + 1;
                        while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                            pomy++;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY < y)
                {
                    if (x -1 != -1 && y + 1 != 5 && Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y + 1;
                        while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                            pomy++;
                        }

                    }
                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY == y)
                {
                    if (x + 1 != 9 &&  Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y;
                        while (pomx < 9 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY == y)
                {
                    if (x - 1 != -1 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y;
                        while (pomx > -1 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                        }

                    }
                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY > y)
                {
                    if (x + 1 != 9 && y - 1 != -1 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y - 1;
                        while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                            pomy--;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY > y)
                {
                    if (x - 1 != -1 && y - 1 != -1 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y - 1;
                        while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                            pomy--;
                        }

                    }
                }

                if (selectedPawn.CurrentX == x && selectedPawn.CurrentY > y)
                {
                    if (y - 1 != -1 && Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x;
                        int pomy = y - 1;
                        while (pomy > -1 && Pawns[pomx, pomy] != null && !Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomy--;
                        }

                    }
                }
            }
            else
            {
                if (selectedPawn.CurrentX == x && selectedPawn.CurrentY > y)
                {

                    if (y - 1 != -1 && Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x;
                        int pomy = y - 1;
                        while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomy--;
                        }
                    }

                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY > y)
                {
                    if (x + 1 != 9 && y - 1 != -1 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y - 1;
                        while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                            pomy--;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY > y)
                {
                    if (x - 1 != -1 && y -1 != -1 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y - 1;
                        while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                            pomy--;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY == y)
                {
                    if (x - 1 != -1 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y;
                        while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                        }

                    }
                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY == y)
                {
                    if (x + 1 != 9 && Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y;
                        while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                        }

                    }
                }

                if (selectedPawn.CurrentX > x && selectedPawn.CurrentY < y)
                {
                    if (x - 1 != -1 && y + 1 != 5 && Pawns[x - 1, y + 1] != null && Pawns[x - 1, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x - 1;
                        int pomy = y + 1;
                        while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx--;
                            pomy++;
                        }

                    }
                }

                if (selectedPawn.CurrentX < x && selectedPawn.CurrentY < y)
                {
                    if (x + 1 != 9 && y + 1 != 5 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x + 1;
                        int pomy = y + 1;
                        while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomx++;
                            pomy++;
                        }

                    }
                }

                if (selectedPawn.CurrentX == x && selectedPawn.CurrentY < y)
                {
                    if (y + 1 != 5 && Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn)
                    {
                        int pomx = x;
                        int pomy = y + 1;
                        while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite)
                        {

                            activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                            Destroy(Pawns[pomx, pomy].gameObject);
                            pomy++;
                        }

                    }
                }
            }



            Pawns[selectedPawn.CurrentX, selectedPawn.CurrentY] = null;
            selectedPawn.transform.position = GetTileCenter(x, y);
            selectedPawn.setPosition(x, y);
            Pawns[x, y] = selectedPawn;
            isWhiteTurn = !isWhiteTurn;
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
