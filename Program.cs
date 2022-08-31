using System;
using System.Globalization;

namespace ConsoleApp_CSCase
{
    interface ITrade
    {
        double Value { get; }
        string ClientSector { get; }
        DateTime NextPaymentDate { get; }
        bool IsPoliticallyExposed { get; }
    }

    class Program
    {
        class Trade : ITrade
        {
            public DateTime ReferenceDate { get; }
            public double Value { get; }
            public string ClientSector { get; }
            public DateTime NextPaymentDate { get; }
            public bool IsPoliticallyExposed { get; }
            public Trade(string _referenceDate, string[] parameters)
            {
                /* _referenceDate: trade reference date in "MM/dd/yyyy"
                 * parameters[x]
                 * x = 0: trade amount
                 * x = 1: client's sector
                 * x = 2: next pending payment in "MM/dd/yyyy"
                 * x = 3: true if it is a PEP (politically exposed person)
                 */

                ReferenceDate = DateTime.ParseExact(_referenceDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                Value = double.Parse(parameters[0]);
                ClientSector = parameters[1];
                NextPaymentDate = DateTime.ParseExact(parameters[2], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                IsPoliticallyExposed = parameters[3] == "true"; // there must be a 4th column in the input file with either true or false value
            }

            public string Classify()
            {
                if (ReferenceDate > NextPaymentDate.AddDays(30))
                {
                    return "EXPIRED";
                }
                else if (Value > 1000000)
                {
                    if (ClientSector == "Private")
                    {
                        return "HIGHRISK";
                    }
                    else if  (ClientSector == "Public")
                    {
                        return "MEDIUMRISK";
                    }
                }
                else if (IsPoliticallyExposed)
                {
                    return "PEP";
                }
                
                return "na";

            }
        }

        static void Main(string[] args)
        {
            // upload portfolio with trades
            string[] lines = System.IO.File.ReadAllLines(@"Portfolio1.txt");
            string referenceDate = lines[0];
            int numberOfTrades;
            if (!int.TryParse(lines[1], out numberOfTrades) || numberOfTrades != lines.Length - 2)
            {
                Console.WriteLine("ERROR: inconsistent number of trades");
                return;
            }

            // classifiy trades
            for (int i = 2; i < lines.Length; i++)
            {
                string[] tradeData = lines[i].Split(" ");
                Trade trade = new Trade(referenceDate, tradeData);
                Console.WriteLine(trade.Classify());
            }
        }
    }
}

/* Question 2
 * In order to have an extensible design allowing new categories to be added easily, I would require the categories to be passed as an array of strings. 
 * In this way, I just need to include the new category in the interface ITrade and implement it in the class Trade recalling I know the position of each category in parameters[].
 */