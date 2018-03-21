using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }
    public bool isWhite;

    public void setPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }
    public bool [,] PossibleMove()
    {

        bool[,] r = new bool[9, 5];
        

        if(isWhite)
        {

            //Diagonal Left
            if (CurrentX != 0 && CurrentY != 4 && ((CurrentX % 2 != 0 && CurrentY % 2 != 0) || (CurrentX % 2 == 0 && CurrentY % 2 == 0))) ;
            {               
              
                if (BoardManager.Instance.Pawns[CurrentX - 1, CurrentY + 1] != null)
                    r[CurrentX - 1, CurrentY + 1] = false;
                else
                    r[CurrentX - 1, CurrentY + 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 8 && CurrentY != 4)
            {

                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY + 1] != null)
                {
                    r[CurrentX + 1, CurrentY + 1] = false;
                }
                else
                {
                    r[CurrentX + 1, CurrentY + 1] = true;
                }
            }

            //Forward
            if (CurrentY != 4)
            {               
                if (BoardManager.Instance.Pawns[CurrentX, CurrentY + 1] != null)
                    r[CurrentX, CurrentY + 1] = false;
                else
                    r[CurrentX, CurrentY + 1] = true;
            }

            //Left
            if (CurrentX != 0)
            {
                if (BoardManager.Instance.Pawns[CurrentX -1, CurrentY] != null)
                    r[CurrentX - 1, CurrentY] = false;
                else
                    r[CurrentX - 1, CurrentY] = true;
            }

            //Right
            if (CurrentX != 8)
            {
                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY] != null)
                    r[CurrentX + 1, CurrentY] = false;
                else
                    r[CurrentX + 1, CurrentY] = true;
            }

            //Diagonal Left backward
            if (CurrentX != 0 && CurrentY != 0)
            {

                if (BoardManager.Instance.Pawns[CurrentX - 1, CurrentY - 1] != null)
                    r[CurrentX - 1, CurrentY - 1] = false;
                else
                    r[CurrentX - 1, CurrentY - 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 8 && CurrentY != 0)
            {

                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY - 1] != null)
                {
                    r[CurrentX + 1, CurrentY - 1] = false;
                }
                else
                {
                    r[CurrentX + 1, CurrentY - 1] = true;
                }
            }

            //Backward
            if (CurrentY != 0)
            {
                if (BoardManager.Instance.Pawns[CurrentX, CurrentY - 1] != null)
                    r[CurrentX, CurrentY - 1] = false;
                else
                    r[CurrentX, CurrentY - 1] = true;
            }


        }
        else
        {
            //Diagonal Left
            if (CurrentX != 8 && CurrentY != 0)
            {

                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY - 1] != null)
                    r[CurrentX + 1, CurrentY - 1] = false;
                else
                    r[CurrentX + 1, CurrentY - 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 0 && CurrentY != 0)
            {

                if (BoardManager.Instance.Pawns[CurrentX - 1, CurrentY - 1] != null)
                {
                    r[CurrentX - 1, CurrentY - 1] = false;
                }
                else
                {
                    r[CurrentX - 1, CurrentY - 1] = true;
                }
            }

            //Forward
            if (CurrentY != 0)
            {
                if (BoardManager.Instance.Pawns[CurrentX, CurrentY - 1] != null)
                    r[CurrentX, CurrentY - 1] = false;
                else
                    r[CurrentX, CurrentY - 1] = true;
            }

            //Left
            if (CurrentX != 8)
            {
                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY] != null)
                    r[CurrentX + 1, CurrentY] = false;
                else
                    r[CurrentX + 1, CurrentY] = true;
            }

            //Right
            if (CurrentX != 0)
            {
                if (BoardManager.Instance.Pawns[CurrentX - 1, CurrentY] != null)
                    r[CurrentX - 1, CurrentY] = false;
                else
                    r[CurrentX - 1, CurrentY] = true;
            }

            //Diagonal Left backward
            if (CurrentX != 8 && CurrentY != 4)
            {

                if (BoardManager.Instance.Pawns[CurrentX + 1, CurrentY + 1] != null)
                    r[CurrentX + 1, CurrentY + 1] = false;
                else
                    r[CurrentX + 1, CurrentY + 1] = true;
            }

            //Diagonal Right
            if (CurrentX != 0 && CurrentY != 4)
            {

                if (BoardManager.Instance.Pawns[CurrentX - 1, CurrentY + 1] != null)
                {
                    r[CurrentX - 1, CurrentY + 1] = false;
                }
                else
                {
                    r[CurrentX - 1, CurrentY + 1] = true;
                }
            }

            //Backward
            if (CurrentY != 4)
            {
                if (BoardManager.Instance.Pawns[CurrentX, CurrentY + 1] != null)
                    r[CurrentX, CurrentY + 1] = false;
                else
                    r[CurrentX, CurrentY + 1] = true;
            }
        }
        return r;

    }

}
