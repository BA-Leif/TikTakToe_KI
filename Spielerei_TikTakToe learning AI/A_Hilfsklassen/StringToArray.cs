using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spielerei_TikTakToe_learning_AI.Hilfsklassen
{
    class StringToArray
    {
        public int[] ToIntArray(string input)
        {
            string[] input_Array = input.Split(',');
            int[] output = new int[input_Array.Length];

            for (int i = 0; i < input_Array.Length; i++)
            {
                output[i] = Int32.Parse(input_Array[i]);
            }

            return output;
        }
    }
}
