using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenServer
{
    public class Tokenizer
    {
        private int[] VALID_START = new int[] { 3, 4, 5, 6 };

        // 369345657653147
        public string MakeToken(string ID)
        {
            if (!IsIDValid(ID))
                return null;

            string lastFourDigits = ID.Substring(ID.Length - 4, 4);

            return BuildToken(ID) + lastFourDigits;
        }
        
        private string BuildToken(string ID)
        {
            int[] token = new int[ID.Length - 4];
            Random random = new Random((int)DateTime.Now.Ticks);
            int sum = 0;

            for (int i = 0; i < token.Length; ++i)
            {
                token[i] = GenerateCorrectNumber(ID[i], random);
                sum += token[i];
            }

            while (sum % 10 == 0)
            {
                sum -= token[token.Length - 1];
                token[token.Length - 1] = GenerateCorrectNumber(ID[token.Length - 1], random);
                sum += token[token.Length - 1];
            }


            char[] result = new char[token.Length];
            for (int i = 0; i < token.Length; ++i)
                result[i] = token[i].ToString()[0];

            return new string(result);
        }

        private int GenerateCorrectNumber(char c, Random random)
        {
            bool correctStart = false;
            int rand = 0;

            while (!correctStart)
            {
                rand = random.Next(1, 10);
                
                if (rand != Convert.ToInt32(c.ToString()))
                {
                    correctStart = true;

                    foreach (int digit in VALID_START)
                        if (digit == rand)
                            correctStart = false;
                }
            }

            return rand;
        }

        private bool IsIDValid(string ID)
        {
            bool correctStart = false;

            foreach(int digit in VALID_START)
                if(digit == Convert.ToInt32(ID[0].ToString()))
                    correctStart = true;

            return correctStart && LuhnTest(ID);
        }

        private bool LuhnTest(string ID)
        {
            int[] numberArray = new int[ID.Length];

            for (int i = 0; i < ID.Length; ++i)
                numberArray[i] = Convert.ToInt32(ID[i].ToString());

            int sum = 0;

            for (int i = 0; i < numberArray.Length; ++i)
            {
                if (i % 2 != 0)
                    numberArray[i] *= 2;

                if (numberArray[i] > 9)
                    numberArray[i] = SumDigits(numberArray[i]);

                sum += numberArray[i];
            }
            return (sum % 10 == 0);
        }

        private int SumDigits(int number)
        {
            int sum = 0;

            while(number != 0)
            {
                sum += number % 10;
                number /= 10;
            }

            return sum;
        }
    }
}
