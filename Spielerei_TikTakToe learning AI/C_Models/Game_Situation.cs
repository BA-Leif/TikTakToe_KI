using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spielerei_TikTakToe_learning_AI.Model
{
    class Game_Situation
    {
        public int ID { get; set; }
        public string Board { get; set; }
        public string PossibleMoves { get; set; }

        public Game_Situation(string board, string possibleMoves)
        {
            this.Board = board;
            this.PossibleMoves = possibleMoves;
        }

        public Game_Situation()
        {

        }

        //public Game_Situation(int[] board)
        //{

        //    string board_String = "";
        //    string move_String = "";

        //    for (int i = 0; i < board.Length; i++)
        //    {
        //        if (board_String == "")
        //        {
        //            board_String += board[i].ToString();
        //        }
        //        else
        //        {
        //            board_String += "," + board[i].ToString();
        //        }

        //        if (board[i] == 0)
        //        {
        //            move_String += 3;
        //        }
        //        else
        //        {
        //            move_String += 0;
        //        }
        //    }

        //    //this.ID = id + 1;
        //    this.Board = board_String;
        //    this.PossibleMoves = move_String;

        //}
    }

}
