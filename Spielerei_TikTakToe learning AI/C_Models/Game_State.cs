using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spielerei_TikTakToe_learning_AI.Model
{
    class Game_State
    {
        public int[] Board { get; set; }
        public Boolean GameOver { get; set; }
        public int ActivePlayer { get; set; }
        public int Winner { get; set; }

        public Game_State()
        {
            Board = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            GameOver = false;
            //Winner = 0;
            ActivePlayer = -1;
        }  
    }
}
