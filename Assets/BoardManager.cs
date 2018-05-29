using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.SceneManagement;


public class BoardManager : MonoBehaviour
{

    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }
    private bool[,] forcedMoves { get; set; }
    public bool[,] previousPositions { set; get; }
    public bool[,] beats = new bool[9, 5];
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

    /// SZTUCZNA INTELIGENCJA

    //sklonowana lista pionków
    public Pawn[,] SPawns { get; set; } 
    //zaznaczony pionek do symulacji
    public Pawn SselectedPawn;
    //lista ścieżek
    public List<List<Point>> Paths = new List<List<Point>>();
    //najlepsza wybrana ścieżka
    public List<Point> BestPath = new List<Point>();
    //lista pionków gracza komputerowego gdy nie ma bicia
    public List<Pawn> AIPawnsWithoutBeats = new List<Pawn>();

    
    /// <summary> Informuje czy wypełniona została tablica z informacjami o pionkach dla AI</summary>
    private bool AIstart = true;
    /// <summary>
    /// Implementacja sztucznej inteligencji
    /// </summary>
    /// 

    //flaga oznaczająca czy tryb gry przeciwko człowiekowi, czy komputerowi 
    public int flagVersion = 0;

    //ustawienie na gre przeciwko komputerowi
    public void AIvsPlayer()
    {
        flagVersion = 1;
    }
    //ustawienie na gre przeciwko czlowiekowi
     public void PlayervsPlayer()
    {
        flagVersion = 0;
    }

    //glowna funkcja maszyny grajacej
    private void ArtificialIntelligence()
    {
        AIisRunning = true;
        SPawns = (Pawn[,])Pawns.Clone();        
        Paths.Clear();
        BestPath.Clear();
        CleanPreviousPositions();
        if (forcedToMove.Count != 0)
        {
            foreach (Pawn pawn in forcedToMove)
            {
                SSelectPawn(pawn.CurrentX, pawn.CurrentY);
                List<Point> list = getForcedPoints(pawn, pawn.CurrentX, pawn.CurrentY);

                foreach (var p in list)
                {
                    List<Point> listP = new List<Point>();
                    listP.Add(p);
                    Paths.Add(listP);
                }

            }

            findPath();

            foreach (var x in Paths)
            {
                x.First().pawn.CurrentX = x.First().pawnX;
                x.First().pawn.CurrentY = x.First().pawnY;
            }
            int max = 0;
            int suma = 0;
            foreach (var x in Paths)
            {
                foreach (var y in x)
                {
                    suma += y.beats;
                }
                if (suma > max)
                {
                    BestPath = x;
                    max = suma;
                }
                suma = 0;
            }

            Paths.Clear();
            CleanPreviousPositions();
        }
        else
        {
            findBestMove();
            if (BestPath.Count != 0)
            {
                System.Random rnd = new System.Random();
                int index = rnd.Next(0, BestPath.Count - 1);
                var pom = BestPath[index];
                BestPath.Clear();
                BestPath.Add(pom);
            }
            else
            {
                findFirstMove();
                System.Random rnd = new System.Random();
                int index = rnd.Next(0, BestPath.Count - 1);
                var pom = BestPath[index];
                BestPath.Clear();
                BestPath.Add(pom);
            }
            AIPawnsWithoutBeats.Clear();
        }
    }
    //znajduje ruch, gdy nie ma bicia, oraz gdy nie ma takiego wolnego ruchu, który nie dawałby bić przeciwnikowi
    public void findFirstMove()
    {
        foreach (var x in AIPawnsWithoutBeats)
        {
            allowedMoves = x.SPossibleMove();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (allowedMoves[i, j])
                    {
                        Point point = new Point(x, i, j, x.CurrentX, x.CurrentY);
                        BestPath.Add(point);
                    }
                }
            }
        }
    }
    //znajduje najlepszy ruch, gdy nie ma bicia, czyli taki, który po ruszeniu się, nie da bicia przeciwnikowi 
    public void findBestMove()
    {
        for(int i = 0; i < 9; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                if(SPawns[i,j] != null && SPawns[i,j].isWhite == isWhiteTurn)
                {
                    AIPawnsWithoutBeats.Add(SPawns[i, j]);
                }
            }
        }

        foreach(var x in AIPawnsWithoutBeats)
        {
            allowedMoves = x.SPossibleMove();
            int pomx = x.CurrentX;
            int pomy = x.CurrentY;
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    if(allowedMoves[i,j])
                    {
                        SSelectPawn(pomx, pomy);
                        SimulateMovePawn(i, j);
                        SPawns[x.CurrentX, x.CurrentY] = null;
                        SselectedPawn.setPosition(i, j);
                        SPawns[i, j] = SselectedPawn;                       
                        if(SCheckForced())
                        {
                            Point point = new Point(x, i, j, pomx, pomy);
                            BestPath.Add(point);
                        }
                    }
                    x.CurrentX = pomx;
                    x.CurrentY = pomy;
                    allowedMoves = x.SPossibleMove();
                    SPawns = (Pawn[,])Pawns.Clone();
                }
                x.CurrentX = pomx;
                x.CurrentY = pomy;
                allowedMoves = x.SPossibleMove();
                SPawns = (Pawn[,])Pawns.Clone();
            }
        }
    }
    //sprawdza, czy po symulacji ruchu bez bicia komputera, czy w turze białej będzie biały pionek z biciem
    public bool SCheckForced()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (SPawns[i, j] != null && SPawns[i, j].isWhite != isWhiteTurn)
                {
                    isWhiteTurn = !isWhiteTurn;
                    if (SNextBeating(i, j))
                    {
                        isWhiteTurn = !isWhiteTurn;
                        return false;
                    }
                    isWhiteTurn = !isWhiteTurn;
                }
            }
        }
        return true;
    }
    //funkcja zwracająca wszystkie ścieżki ruchów dla komputera
    public void findPath()
    {
        int flag = 0;
        var pomPaths = new List<List<Point>>(Paths);
        foreach (List<Point> x in Paths)
        {
            SPawns = (Pawn[,])Pawns.Clone();
            int pomx = 0;
            int pomy = 0;
            foreach (var z in Paths)
            {
                z[0].pawn.CurrentX = z[0].pawnX;
                z[0].pawn.CurrentY = z[0].pawnY;
            }
            CleanPreviousPositions();
            foreach (var y in x)
            {
                SSelectPawn(y.pawn.CurrentX, y.pawn.CurrentY);
                SimulateMovePawn(y.pointX, y.pointY);
                y.beats = beatCount;
                SPawns[y.pawnX, y.pawnY] = null;
                SselectedPawn.setPosition(y.pointX, y.pointY);
                SPawns[y.pointX, y.pointY] = SselectedPawn;
                previousPositions[y.pawnX, y.pawnY] = true;
                SaddToForcedMoves(y.pointX, y.pointY);
                AddPreviousPositions();
                allowedMoves = forcedMoves;
                pomx = y.pointX;
                pomy = y.pointY;
            }
            List<Point> list = getForcedPoints(SselectedPawn, pomx, pomy);
            if (list.Count == 1)
            {
                x.Add(list[0]);
            }
            if (list.Count > 1)
            {
                var pom = new List<Point>(x);
                x.Add(list[0]);
                for (int i = 1; i < list.Count; i++)
                {
                    var addnew = new List<Point>(pom);
                    addnew.Add(list[i]);
                    pomPaths.Add(addnew);
                }
            }

        }

        for (int i = 0; i < pomPaths.Count; i++)
            if (Paths.Count <= i)
                Paths.Add(pomPaths[i]);

        foreach (var x in Paths)
        {
            x[0].pawn.CurrentX = x[0].pawnX;
            x[0].pawn.CurrentY = x[0].pawnY;
        }

        foreach (List<Point> x in Paths)
        {
            SPawns = (Pawn[,])Pawns.Clone();
            int pomx = 0;
            foreach (var z in Paths)
            {
                z[0].pawn.CurrentX = z[0].pawnX;
                z[0].pawn.CurrentY = z[0].pawnY;
            }
            int pomy = 0;
            CleanPreviousPositions();
            foreach (var y in x)
            {

                SSelectPawn(y.pawn.CurrentX, y.pawn.CurrentY);
                SimulateMovePawn(y.pointX, y.pointY);
                SPawns[y.pawnX, y.pawnY] = null;
                SselectedPawn.setPosition(y.pointX, y.pointY);
                SPawns[y.pointX, y.pointY] = SselectedPawn;
                previousPositions[y.pawnX, y.pawnY] = true;
                SaddToForcedMoves(y.pointX, y.pointY);
                AddPreviousPositions();
                allowedMoves = forcedMoves;
                pomx = y.pointX;
                pomy = y.pointY;
            }           
            List<Point> list = getForcedPoints(SselectedPawn, pomx, pomy);
            if (list.Count == 0 && flag == 0)
                flag = 0;
            else
                flag = 1;
        }

        if (flag == 1)
            findPath();
    }
    //zwraca listę punktów, w które pionek komputerowy musi się ruszyć, ponieważ ma bicie (do symulacji ścieżek)
    public List<Point> getForcedPoints(Pawn pawn, int x, int y)
    {
        List<Point> points = new List<Point>();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (forcedMoves[i, j])
                {
                    Point point = new Point(pawn, i, j, x ,y);
                    points.Add(point);
                }
            }
        }
        return points;
    }
    //licznik bić
    private int beatCount = 0;
    //funkcja symulująca ruchy komputera (do wyznaczania ścieżek)
    private void SimulateMovePawn(int x, int y)
    {
        beatCount = 0;
        if (SselectedPawn.CurrentX == x && SselectedPawn.CurrentY < y)
        {
            if (y + 1 != 5 && SselectedPawn.CurrentY != 0 && SselectedPawn.CurrentY != 4 && SPawns[x, y + 1] != null && SPawns[x, y + 1].isWhite != isWhiteTurn && SPawns[x, SselectedPawn.CurrentY - 1] != null && SPawns[x, SselectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
            {
                int pomx = x;
                int pomy = y + 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomy++;
                }

                pomx = x;
                pomy = y - 2;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomy--;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x;
                    pomy = y + 1;
                    while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy++;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x;
                    pomy = y - 2;
                    while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy--;
                    }
                }
            }
            else if (y + 1 != 5 && SPawns[x, y + 1] != null && SPawns[x, y + 1].isWhite != isWhiteTurn)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    SPawns[pomx, pomy] = null;
                    beatCount++;
                    pomy++;
                }
            }
            else if (y - 2 > -1 && SPawns[x, y - 2] != null && SPawns[x, y - 2].isWhite != isWhiteTurn)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    SPawns[pomx, pomy] = null;
                    beatCount++;
                    pomy--;
                }
            }
        }
        if (SselectedPawn.CurrentX < x && SselectedPawn.CurrentY < y) 
        {
            if (x + 1 != 9 && y + 1 != 5 && SselectedPawn.CurrentY != 0 && SselectedPawn.CurrentX != 0 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentY != 4 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentY != 4 && SPawns[x + 1, y + 1] != null && SPawns[x + 1, y + 1].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY - 1] != null && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
            {

                int pomx = x + 1;
                int pomy = y + 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomy++;
                    pomx++;
                }


                pomx = x - 2;
                pomy = y - 2;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomy--;
                    pomx--;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x + 1;
                    pomy = y + 1;
                    while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy++;
                        pomx++;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x - 2;
                    pomy = y - 2;
                    while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy--;
                        pomx--;
                    }
                }

            }
            else if (x + 1 != 9 && y + 1 != 5 && SPawns[x + 1, y + 1] != null && SPawns[x + 1, y + 1].isWhite != isWhiteTurn)
            {
                int pomx = x + 1;
                int pomy = y + 1;
                while (pomx < 9 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx++;
                    pomy++;
                }
            }
            else if (x - 2 > -1 && y - 2 > -1 && SPawns[x - 2, y - 2] != null && SPawns[x - 2, y - 2].isWhite != isWhiteTurn)
            {
                int pomx = x - 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {                   
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomy--;
                    pomx--;
                }
            }
        }

        if (SselectedPawn.CurrentX > x && SselectedPawn.CurrentY < y) //Diagonal Forward Left
        {
            if (x - 1 != -1 && y + 1 != 5 && SselectedPawn.CurrentY != 0 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentX != 0 && SselectedPawn.CurrentY != 4 && SPawns[x - 1, y + 1] != null && SPawns[x - 1, y + 1].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY - 1] != null && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomy++;
                    pomx--;
                }


                pomx = x + 2;
                pomy = y - 2;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomy--;
                    pomx++;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x - 1;
                    pomy = y + 1;
                    while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy++;
                        pomx--;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x + 2;
                    pomy = y - 2;
                    while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy--;
                        pomx++;
                    }
                }
            }
            else if (x - 1 != -1 && y + 1 != 5 && SPawns[x - 1, y + 1] != null && SPawns[x - 1, y + 1].isWhite != isWhiteTurn)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx--;
                    pomy++;
                }
               

            }
            else if (x + 2 < 9 && y - 2 > -1 && SPawns[x + 2, y - 2] != null && SPawns[x + 2, y - 2].isWhite != isWhiteTurn)
            {
                int pomx = x + 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomy--;
                    pomx++;
                }
                
            }
        }

        if (SselectedPawn.CurrentX < x && SselectedPawn.CurrentY == y) //Right
        {
            if (x + 1 != 9 && SselectedPawn.CurrentX != 0 && SselectedPawn.CurrentX != 8 && SPawns[x + 1, y] != null && SPawns[x + 1, y].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY] != null && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY].isWhite != isWhiteTurn)
            {


                int pomx = x + 1;
                int pomy = y;
                int beat1 = 0;
                int beat2 = 0;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomx++;
                }


                pomx = x - 2;
                pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomx--;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x + 1;
                    pomy = y;
                    while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx++;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x - 2;
                    pomy = y;
                    while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx--;
                    }
                }

            }
            else if (x + 1 != 9 && SPawns[x + 1, y] != null && SPawns[x + 1, y].isWhite != isWhiteTurn)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx++;
                }               
            }
            else if (x - 2 > -1 && SPawns[x - 2, y] != null && SPawns[x - 2, y].isWhite != isWhiteTurn)
            {
                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx--;
                }                
            }

        }

        if (SselectedPawn.CurrentX > x && SselectedPawn.CurrentY == y)
        {
            if (x - 1 != -1 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentX != 0 && SPawns[x - 1, y] != null && SPawns[x - 1, y].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY] != null && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY].isWhite != isWhiteTurn)
            {
                int pomx = x - 1;
                int pomy = y;
                int beat1 = 0;
                int beat2 = 0;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomx--;
                }


                pomx = x + 2;
                pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomx++;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x - 1;
                    pomy = y;

                    while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx--;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x + 2;
                    pomy = y;
                    while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx++;
                    }
                }
            }
            else if (x - 1 != -1 && SPawns[x - 1, y] != null && SPawns[x - 1, y].isWhite != isWhiteTurn)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx--;
                }              
            }
            else if (x + 2 < 9 && SPawns[x + 2, y] != null && SPawns[x + 2, y].isWhite != isWhiteTurn)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx++;
                }
                
            }
        }


        if (SselectedPawn.CurrentX < x && SselectedPawn.CurrentY > y)
        {
            if (x + 1 != 9 && y - 1 != -1 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentY != 0 && SselectedPawn.CurrentX != 0 && SselectedPawn.CurrentY != 4 && SPawns[x + 1, y - 1] != null && SPawns[x + 1, y - 1].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY + 1] != null && SPawns[SselectedPawn.CurrentX - 1, SselectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
            {


                int pomx = x + 1;
                int pomy = y - 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomx < 9 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomx++;
                    pomy--;
                }

                pomx = x - 2;
                pomy = y + 2;
                while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomx--;
                    pomy++;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x + 1;
                    pomy = y - 1;
                    while (pomx < 9 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx++;
                        pomy--;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x - 2;
                    pomy = y + 2;
                    while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx--;
                        pomy++;
                    }
                }
            }
            else if (x + 1 != 9 && y - 1 != -1 && SPawns[x + 1, y - 1] != null && SPawns[x + 1, y - 1].isWhite != isWhiteTurn)
            {
                int pomx = x + 1;
                int pomy = y - 1;
                while (pomx < 9 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx++;
                    pomy--;
                }
               
            }
            else if (x - 2 > -1 && y + 2 < 5 && SPawns[x - 2, y + 2] != null && SPawns[x - 2, y + 2].isWhite != isWhiteTurn)
            {
                int pomx = x - 2;
                int pomy = y + 2;
                while (pomx > -1 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx--;
                    pomy++;
                }
                
            }
        }

        if (SselectedPawn.CurrentX > x && SselectedPawn.CurrentY > y)
        {
            if (x - 1 != -1 && y - 1 != -1 && SselectedPawn.CurrentX != 0 && SselectedPawn.CurrentY != 0 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentY != 4 && SselectedPawn.CurrentX != 8 && SselectedPawn.CurrentY != 4 && SPawns[x - 1, y - 1] != null && SPawns[x - 1, y - 1].isWhite != isWhiteTurn && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY + 1] != null && SPawns[SselectedPawn.CurrentX + 1, SselectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
            {

                int pomx = x - 1;
                int pomy = y - 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomx > -1 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomx--;
                    pomy--;
                }

                pomx = x +2;
                pomy = y +2;
                while (pomx < 9 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++;
                    pomx++;
                    pomy++;
                }
                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x - 1;
                    pomy = y - 1;
                    while (pomx > -1 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx--;
                        pomy--;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x + 2;
                    pomy = y + 2;
                    while (pomx < 9 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomx++;
                        pomy++;
                    }
                }

            }
            else if (x - 1 != -1 && y - 1 != -1 && SPawns[x - 1, y - 1] != null && SPawns[x - 1, y - 1].isWhite != isWhiteTurn)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomx > -1 && pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx--;
                    pomy--;
                }
            }
            else if (x + 2 < 9 && y + 2 < 5 && SPawns[x + 2, y + 2] != null && SPawns[x + 2, y + 2].isWhite != isWhiteTurn)
            {
                int pomx = x + 2;
                int pomy = y + 2;
                while (pomx < 9 && pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomx++;
                    pomy++;
                }

            }
        }

        if (SselectedPawn.CurrentX == x && SselectedPawn.CurrentY > y)
        {
            if (y - 1 != -1 && SselectedPawn.CurrentY != 4 && SselectedPawn.CurrentY != 0 && SPawns[x, y - 1] != null && SPawns[x, y - 1].isWhite != isWhiteTurn && SPawns[x, SselectedPawn.CurrentY + 1] != null && SPawns[x, SselectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
            {

                int pomx = x;
                int pomy = y - 1;
                int beat1 = 0;
                int beat2 = 0;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat1++;
                    pomy--;
                }
               

                pomx = x;
                pomy = y + 2;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beat2++; 
                    pomy++;
                }


                if (beat1 >= beat2)
                {
                    beatCount = beat1;
                    pomx = x;
                    pomy = y - 1;

                    while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy--;
                    }
                }
                else
                {
                    beatCount = beat2;
                    pomx = x;
                    pomy = y + 2;
                    while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                    {
                        SPawns[pomx, pomy] = null;
                        pomy++;
                    }
                }

            }
            else if (y - 1 != -1 && SPawns[x, y - 1] != null && SPawns[x, y - 1].isWhite != isWhiteTurn)
            {
                int pomx = x;
                int pomy = y - 1;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomy--;
                }
                
            }
            else if (y + 2 < 5 && SPawns[x, y + 2] != null && SPawns[x, y + 2].isWhite != isWhiteTurn)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beatCount++;
                    SPawns[pomx, pomy] = null;
                    pomy++;
                }
                
            }
        }
    }
    //funkcja do symulacji, która sprawdza, czy dany pionek ma kolejne bicia (działa na sklonowanej tablicy pionków)
    public bool SNextBeating(int x, int y)
    {
        allowedMoves = SPawns[x, y].PossibleMove();
        bool[,] f = new bool[9, 5];
        int flag = 0;
        if (y < 3 && allowedMoves[x,y+1])//
        {
            if (SPawns[x, y + 2] != null && SPawns[x, y + 2].isWhite != isWhiteTurn && SPawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 2, y + 2] != null && SPawns[x + 2, y + 2].isWhite != isWhiteTurn && SPawns[x + 1, y + 1] == null)
            {
                int pomx = x + 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 2, y + 2] != null && SPawns[x - 2, y + 2].isWhite != isWhiteTurn && SPawns[x - 1, y + 1] == null)
            {
                int pomx = x - 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 2, y - 2] != null && SPawns[x - 2, y - 2].isWhite != isWhiteTurn && SPawns[x - 1, y - 1] == null)
            {
                int pomx = x - 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 2, y - 2] != null && SPawns[x + 2, y - 2].isWhite != isWhiteTurn && SPawns[x + 1, y - 1] == null)
            {
                int pomx = x + 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    pomy--;
                    nextBeat[4]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 1 && allowedMoves[x,y-1])//
        {
            if (SPawns[x, y - 2] != null && SPawns[x, y - 2].isWhite != isWhiteTurn && SPawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy--;
                    nextBeat[5]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 1 && allowedMoves[x-1,y])
        {
            if (SPawns[x - 2, y] != null && SPawns[x - 2, y].isWhite != isWhiteTurn && SPawns[x - 1, y] == null)
            {
                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x < 7 && allowedMoves[x+1,y])
        {
            if (SPawns[x + 2, y] != null && SPawns[x + 2, y].isWhite != isWhiteTurn && SPawns[x + 1, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[7]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        if (y > 0 && y < 4 && allowedMoves[x,y+1])
        {
            if (SPawns[x, y - 1] != null && SPawns[x, y - 1].isWhite != isWhiteTurn && SPawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y-1;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 1, y - 1] != null && SPawns[x - 1, y - 1].isWhite != isWhiteTurn && SPawns[x + 1, y + 1] == null)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 1, y - 1] != null && SPawns[x + 1, y - 1].isWhite != isWhiteTurn && SPawns[x - 1, y + 1] == null)
            {
                int pomx = x + 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 1, y + 1] != null && SPawns[x + 1, y + 1].isWhite != isWhiteTurn && SPawns[x - 1, y - 1] == null)
            {
                int pomx = x + 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 1, y + 1] != null && SPawns[x - 1, y + 1].isWhite != isWhiteTurn && SPawns[x + 1, y - 1] == null)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    pomx--;
                    nextBeat[12]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4 && allowedMoves[x,y-1])
        {
            if (SPawns[x, y + 1] != null && SPawns[x, y + 1].isWhite != isWhiteTurn && SPawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomy++;
                    nextBeat[13]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 0 && x < 8 && allowedMoves[x-1,y])
        {
            if (SPawns[x + 1, y] != null && SPawns[x + 1, y].isWhite != isWhiteTurn && SPawns[x - 1, y] == null)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    pomx++;
                    nextBeat[14]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x > 0 && x < 8 && allowedMoves[x+1,y])
        {
            if (SPawns[x - 1, y] != null && SPawns[x - 1, y].isWhite != isWhiteTurn && SPawns[x + 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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

    //funkcja do symulacji zwracająca tablicę bool dla danego pionka, z pozycjami, w które musi się ruszyć, ponieważ ma bicie
    public bool SaddToForcedMoves(int x, int y)
    {
        allowedMoves = SPawns[x, y].SPossibleMove();
        bool[,] f = new bool[9, 5];
        int flag = 0;
        if (y < 3)
        {
            if (SPawns[x, y + 2] != null && SPawns[x, y + 2].isWhite != isWhiteTurn && SPawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 2, y + 2] != null && SPawns[x + 2, y + 2].isWhite != isWhiteTurn && SPawns[x + 1, y + 1] == null)
            {
                int pomx = x + 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 2, y + 2] != null && SPawns[x - 2, y + 2].isWhite != isWhiteTurn && SPawns[x - 1, y + 1] == null)
            {
                int pomx = x - 2;
                int pomy = y + 2;
                while (pomy < 5 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 2, y - 2] != null && SPawns[x - 2, y - 2].isWhite != isWhiteTurn && SPawns[x - 1, y - 1] == null)
            {
                int pomx = x - 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 2, y - 2] != null && SPawns[x + 2, y - 2].isWhite != isWhiteTurn && SPawns[x + 1, y - 1] == null)
            {
                int pomx = x + 2;
                int pomy = y - 2;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x, y - 2] != null && SPawns[x, y - 2].isWhite != isWhiteTurn && SPawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 2, y] != null && SPawns[x - 2, y].isWhite != isWhiteTurn && SPawns[x - 1, y] == null)
            {
                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 2, y] != null && SPawns[x + 2, y].isWhite != isWhiteTurn && SPawns[x + 1, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x, y - 1] != null && SPawns[x, y - 1].isWhite != isWhiteTurn && SPawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y;
                while (pomy > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 1, y - 1] != null && SPawns[x - 1, y - 1].isWhite != isWhiteTurn && SPawns[x + 1, y + 1] == null)
            {
                int pomx = x - 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 1, y - 1] != null && SPawns[x + 1, y - 1].isWhite != isWhiteTurn && SPawns[x - 1, y + 1] == null)
            {
                int pomx = x + 1;
                int pomy = y - 1;
                while (pomy > -1 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 1, y + 1] != null && SPawns[x + 1, y + 1].isWhite != isWhiteTurn && SPawns[x - 1, y - 1] == null)
            {
                int pomx = x + 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 1, y + 1] != null && SPawns[x - 1, y + 1].isWhite != isWhiteTurn && SPawns[x + 1, y - 1] == null)
            {
                int pomx = x - 1;
                int pomy = y + 1;
                while (pomy < 5 && pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x, y + 1] != null && SPawns[x, y + 1].isWhite != isWhiteTurn && SPawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x + 1, y] != null && SPawns[x + 1, y].isWhite != isWhiteTurn && SPawns[x - 1, y] == null)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
            if (SPawns[x - 1, y] != null && SPawns[x - 1, y].isWhite != isWhiteTurn && SPawns[x + 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && SPawns[pomx, pomy] != null && SPawns[pomx, pomy].isWhite != isWhiteTurn)
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
    
    public void restart()
    {
        if (flagVersion == 0)
            SceneManager.LoadScene("FanoronaGameRestart");
        else
            SceneManager.LoadScene("FanoronaGameRestartAI");
    }

    public void backtomenu()
    {
        SceneManager.LoadScene("FanoronaBackToMenu");
    }
    public GameObject canvasEnd;
    public GameObject canvasEnd1;
    public GameObject canvasMenu;
    private void Start()
    {

        Instance = this;
        SpawnAll();
        CleanPreviousPositions();
        //canvasEnd1.SetActive(false);
        //canvasEnd.SetActive(false);
    }

    public void CheckEnd()
    {
        int countWhite = 0;
        int countBlack = 0;
        for(int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (Pawns[i, j] != null && Pawns[i, j].isWhite)
                    countWhite++;
                else if (Pawns[i, j] != null && !Pawns[i, j].isWhite)
                    countBlack++;
            }
        }
        if(countBlack == 0)
        {
            canvasEnd.SetActive(true);
        }
        if(countWhite == 0)
        {
            canvasEnd1.SetActive(true);
        }
    }

    public bool AIisRunning = false;
    private void Update()
    {
        if (Input.GetKeyDown("escape"))
            if (canvasMenu.activeSelf)
                canvasMenu.SetActive(false);
            else
                canvasMenu.SetActive(true);
        DrawBoard();
        CheckEnd();
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
            if(didBeat && !hasMoved && !AIisRunning)
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
                        BoardHighlights.Instance.HighlightBeats(beats);
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
        if (flagVersion == 1)
        {
            if (!isWhiteTurn && !hasMoved)
            {

                if (!AIisRunning)
                    ArtificialIntelligence();
                foreach (var x in BestPath)
                {
                    SelectPawn(x.pawnX, x.pawnY);
                    addToForcedMoves(selectedPawn.CurrentX, selectedPawn.CurrentY);
                    StartCoroutine(MovePawn(x.pointX, x.pointY));
                    previousPositions[x.pawnX, x.pawnY] = true;
                    AddPreviousPositions();
                    BoardHighlights.Instance.HighlightPastMoves(previousPositions);
                    BestPath.RemoveAt(0);
                    AIisRunning = true;
                }

            }
        }
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && (!hasMoved || isWaitForMouseClickIsRunning) && !AIisRunning)
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
            if(endMove)
            {
                if (BestPath.Count == 0)
                {
                    BoardHighlights.Instance.HideHighlights();
                    AIisRunning = false;
                }
                if(AIisRunning)
                { isWhiteTurn = !isWhiteTurn; }
                Pawns[selectedPawn.CurrentX, selectedPawn.CurrentY] = null;
                selectedPawn.setPosition(moveX, moveY);
                Pawns[moveX, moveY] = selectedPawn;
                selectedPawn = null;
                checkedForceToMove = false;
                hasMoved = false;
                endMove = false;
                isWhiteTurn = !isWhiteTurn;
            }
            StartCoroutine(transformPosition(moveX, moveY));
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
            BoardHighlights.Instance.HighlightBeats(beats);
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
        else
        {
            allowedMoves = Pawns[x, y].PossibleMove();
            selectedPawn = Pawns[x, y];
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
    }

    private void SSelectPawn(int x, int y)
    {
        if (SPawns[x, y] == null)
            return;
        if (SPawns[x, y].isWhite != isWhiteTurn)
            return;
        if (SNextBeating(x, y))
        {
            SaddToForcedMoves(x, y);
            allowedMoves = forcedMoves;
            SselectedPawn = SPawns[x, y];
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
        else
        {
            allowedMoves = SPawns[x, y].PossibleMove();
            SselectedPawn = SPawns[x, y];
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
    }


    public bool isWaitForMouseClickIsRunning = false;
    public IEnumerator WaitForMouseClick(int x, int y, int flag)
    {
        while (true)
        {
            isWaitForMouseClickIsRunning = true;
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane")))
                {
                    int pomx = (int)hit.point.x;
                    int pomy = (int)hit.point.z;
                    if (flag == 1 && pomx != x && pomy != y)
                    {
                        isWaitForMouseClickIsRunning = false;
                        yield break;
                    }
                    else if (flag == 0 && pomy != y)
                    {
                        isWaitForMouseClickIsRunning = false;
                        yield break;
                    }
                    else if (flag == 2 && pomx != x)
                    {
                        isWaitForMouseClickIsRunning = false;
                        yield break;
                    }
                }


            }
            yield return null;
        }
    }

    public bool NextBeating(int x, int y)
    {
        allowedMoves = Pawns[x, y].PossibleMove();
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 5; j++)
                beats[i, j] = false;
        bool[,] f = new bool[9, 5];
        int flag = 0;
        if (y < 3 && allowedMoves[x,y+1])
        {
            if (Pawns[x, y + 2] != null && Pawns[x, y + 2].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y + 2;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
                    pomx++;
                    pomy--;
                    nextBeat[4]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 1 && allowedMoves[x,y-1])
        {
            if (Pawns[x, y - 2] != null && Pawns[x, y - 2].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y - 2;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
                    pomy--;
                    nextBeat[5]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 1 && allowedMoves[x-1,y])
        {
            if (Pawns[x - 2, y] != null && Pawns[x - 2, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {

                int pomx = x - 2;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
                    pomx--;
                    nextBeat[6]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x < 7 && allowedMoves[x+1,y])
        {
            if (Pawns[x + 2, y] != null && Pawns[x + 2, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x + 2;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
                    pomx++;
                    nextBeat[7]++;
                }
                flag = 1;
                f[x + 1, y] = true;
            }
        }
        if (y > 0 && y < 4 && allowedMoves[x,y+1])
        {
            if (Pawns[x, y - 1] != null && Pawns[x, y - 1].isWhite != isWhiteTurn && Pawns[x, y + 1] == null)
            {
                int pomx = x;
                int pomy = y;
                while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
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
                    beats[pomx, pomy] = true;
                    pomy++;
                    pomx--;
                    nextBeat[12]++;
                }
                flag = 1;
                f[x + 1, y - 1] = true;
            }
        }
        if (y > 0 && y < 4 && allowedMoves[x,y-1])
        {
            if (Pawns[x, y + 1] != null && Pawns[x, y + 1].isWhite != isWhiteTurn && Pawns[x, y - 1] == null)
            {
                int pomx = x;
                int pomy = y + 1;
                while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
                    pomy++;
                    nextBeat[13]++;
                }
                flag = 1;
                f[x, y - 1] = true;
            }
        }
        if (x > 0 && x < 8 && allowedMoves[x-1,y])
        {
            if (Pawns[x + 1, y] != null && Pawns[x + 1, y].isWhite != isWhiteTurn && Pawns[x - 1, y] == null)
            {
                int pomx = x + 1;
                int pomy = y;
                while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
                    pomx++;
                    nextBeat[14]++;
                }
                flag = 1;
                f[x - 1, y] = true;
            }
        }
        if (x > 0 && x < 8 && allowedMoves[x+1,y])
        {
            if (Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[x + 1, y] == null)
            {
                int pomx = x - 1;
                int pomy = y;
                while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                {
                    beats[pomx, pomy] = true;
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
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,0));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomy--;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x;
                        int pomy = y + 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomy++;
                        }
                        pomx = x;
                        pomy = y - 2;
                        while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomy--;
                        }

                        if(beat1 >= beat2)
                        {
                            pomx = x;
                            pomy = y + 1;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {
                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomy++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x;
                            pomy = y - 2;
                            while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
                        pomy--;
                    }
                    didBeat = true;
                }

            }

            if (selectedPawn.CurrentX < x && selectedPawn.CurrentY < y) //Diagonal Forward Right
            {
                if (x + 1 != 9 && y + 1 != 5 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && Pawns[x + 1, y + 1] != null && Pawns[x + 1, y + 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY - 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY - 1].isWhite != isWhiteTurn)
                {
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,1));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomy--;
                                    pomx--;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x + 1;
                        int pomy = y + 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomy++;
                            pomx++;
                        }
                        pomx = x - 2;
                        pomy = y - 2;
                        while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomy--;
                            pomx--;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x + 1;
                            pomy = y + 1;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {
                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomy++;
                                pomx++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x - 2;
                            pomy = y - 2;
                            while (pomy > -1 && pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,1));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomy--;
                                    pomx++;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x - 1;
                        int pomy = y + 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomy++;
                            pomx--;
                        }
                        pomx = x + 2;
                        pomy = y - 2;
                        while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomy--;
                            pomx++;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x - 1;
                            pomy = y + 1;
                            while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomy++;
                                pomx--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x + 2;
                            pomy = y - 2;
                            while (pomy > -1 && pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,2));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomx--;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x + 1;
                        int pomy = y;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomx++;
                        }
                        pomx = x - 2;
                        pomy = y;
                        while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomx--;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x + 1;
                            pomy = y;
                            while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomx++;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x - 2;
                            pomy = y;
                            while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
                        pomx--;
                    }
                    didBeat = true;
                }

            }

            if (selectedPawn.CurrentX > x && selectedPawn.CurrentY == y)
            {
                if (x - 1 != -1 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentX != 0 && Pawns[x - 1, y] != null && Pawns[x - 1, y].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY].isWhite != isWhiteTurn)
                {
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,2));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomx++;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x - 1;
                        int pomy = y;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomx--;
                        }
                        pomx = x + 2;
                        pomy = y;
                        while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomx++;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x - 1;
                            pomy = y;
                            while (pomx > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomx--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x + 2;
                            pomy = y;
                            while (pomx < 9 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
                        pomx++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX < x && selectedPawn.CurrentY > y)
            {
                if (x + 1 != 9 && y - 1 != -1 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 4 && Pawns[x + 1, y - 1] != null && Pawns[x + 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX - 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
                {
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,1));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomx--;
                                    pomy++;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x + 1;
                        int pomy = y - 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomx++;
                            pomy--;
                        }
                        pomx = x - 2;
                        pomy = y + 2;
                        while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomx--;
                            pomy++;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x + 1;
                            pomy = y - 1;
                            while (pomx < 9 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomx++;
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x - 2;
                            pomy = y + 2;
                            while (pomx > -1 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
                        pomx--;
                        pomy++;
                    }
                    didBeat = true;
                }
            }

            if (selectedPawn.CurrentX > x && selectedPawn.CurrentY > y)
            {
                if (x - 1 != -1 && y - 1 != -1 && selectedPawn.CurrentX != 0 && selectedPawn.CurrentY != 0 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && selectedPawn.CurrentX != 8 && selectedPawn.CurrentY != 4 && Pawns[x - 1, y - 1] != null && Pawns[x - 1, y - 1].isWhite != isWhiteTurn && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1] != null && Pawns[selectedPawn.CurrentX + 1, selectedPawn.CurrentY + 1].isWhite != isWhiteTurn)
                {
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,1));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomx++;
                                    pomy++;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x - 1;
                        int pomy = y - 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomx--;
                            pomy--;
                        }
                        pomx = x + 2;
                        pomy = y + 2;
                        while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomx++;
                            pomy++;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x - 1;
                            pomy = y - 1;
                            while (pomx > -1 && pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomx--;
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x + 2;
                            pomy = y + 2;
                            while (pomx < 9 && pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                    if (!AIisRunning)
                    {
                        yield return StartCoroutine(WaitForMouseClick(x, y,0));
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
                                    Pawns[pomx, pomy] = null;
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
                                    Pawns[pomx, pomy] = null;
                                    pomy++;
                                }
                                didBeat = true;
                            }
                        }
                    }
                    else
                    {
                        int pomx = x;
                        int pomy = y - 1;
                        int beat1 = 0;
                        int beat2 = 0;
                        while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat1++;
                            pomy--;
                        }
                        pomx = x;
                        pomy = y + 2;
                        while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                        {
                            beat2++;
                            pomy++;
                        }
                        if(beat1 >= beat2)
                        {
                            pomx = x;
                            pomy = y - 1;
                            while (pomy > -1 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
                                pomy--;
                            }
                            didBeat = true;
                        }
                        else
                        {
                            pomx = x;
                            pomy = y + 2;
                            while (pomy < 5 && Pawns[pomx, pomy] != null && Pawns[pomx, pomy].isWhite != isWhiteTurn)
                            {

                                activeDraughtsman.Remove(Pawns[pomx, pomy].gameObject);
                                Destroy(Pawns[pomx, pomy].gameObject);
                                Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
                        Pawns[pomx, pomy] = null;
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
            newPosition = Vector3.Lerp(selectedPawn.transform.position, GetTileCenter(x, y), 0.005f);
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