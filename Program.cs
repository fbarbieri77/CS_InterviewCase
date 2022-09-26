using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    interface ICategory
    {
        string Name { get; }
        bool Identify(ITrade trade);
    }

    enum EnumAllCategories
    {
        EXPIRED,
        MEDIUMRISK,
        HIGHRISK,
        PEP,
        NA,
    }

    class Program
    {
        public class Expired : ICategory
        {
            private readonly DateTime referenceDate;

            public string Name
            {
                get
                {
                    return "EXPIRED";
                }
            }
            public Expired (string _referenceDate)
            {
                referenceDate = DateTime.ParseExact(_referenceDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }

            public bool Identify(ITrade trade)
            {
                return referenceDate > trade.NextPaymentDate.AddDays(30);
            }
        }

        public class MediumRisk : ICategory
        {
            public string Name
            {
                get
                {
                    return "MEDIUMRISK";
                }
            }

            public bool Identify(ITrade trade)
            {
                return trade.Value > 1000000 && trade.ClientSector == "Public";
            }
        }

        public class HighRisk : ICategory
        {
            public string Name 
            {
                get
                {
                    return "HIGHRISK";
                }
            }

            public bool Identify(ITrade trade) 
            {
                return trade.Value > 1000000 && trade.ClientSector == "Private";
            }
        }

        public class PEP : ICategory
        {
            public string Name
            {
                get
                {
                    return "PEP";
                }
            }

            public bool Identify(ITrade trade)
            {
                return trade.IsPoliticallyExposed;
            }
        }

        public class NotCategorised : ICategory
        {
            public string Name
            {
                get
                {
                    return "Not categorised";
                }
            }

            public bool Identify(ITrade trade)
            {
                return true;
            }
        }

        class GetCategoryFactory
        {
            public ICategory GetCategory(string categoryType, string referenceDate)
            {
                if (categoryType == null)
                {
                    return null;
                }
                else if (categoryType == EnumAllCategories.EXPIRED.ToString())
                {
                    return new Expired(referenceDate);
                }
                else if (categoryType == EnumAllCategories.MEDIUMRISK.ToString())
                {
                    return new MediumRisk();
                }
                else if (categoryType == EnumAllCategories.HIGHRISK.ToString())
                {
                    return new HighRisk();
                }
                else if (categoryType == EnumAllCategories.PEP.ToString())
                {
                    return new PEP();
                }
                else if (categoryType == EnumAllCategories.NA.ToString())
                {
                    return new NotCategorised();
                }

                return null;
            }

            public List<ICategory> GetAllCategories(string referenceDate)
            {
                List<ICategory> categories = new List<ICategory>();
                foreach (string categoryName in Enum.GetNames(typeof(EnumAllCategories)))
                {
                    categories.Add(this.GetCategory(categoryName, referenceDate));
                }
                return categories;
            }
        }

        class Trade : ITrade
        {
            public double Value { get; }
            public string ClientSector { get; }
            public DateTime NextPaymentDate { get; }
            public bool IsPoliticallyExposed { get; }

            public Trade(string _referenceDate, string[] parameters)
            {
                Value = double.Parse(parameters[0]);
                ClientSector = parameters[1];
                NextPaymentDate = DateTime.ParseExact(parameters[2], "MM/dd/yyyy", CultureInfo.InvariantCulture);
                IsPoliticallyExposed = parameters[3] == "true";
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
            GetCategoryFactory categoryFactory = new GetCategoryFactory();
            List<ICategory> categories = categoryFactory.GetAllCategories(referenceDate);
            for (int i = 2; i < lines.Length; i++)
            {
                string[] tradeData = lines[i].Split(" ");
                Trade trade = new Trade(referenceDate, tradeData);

                foreach (ICategory category in categories)
                {
                    if (category.Identify(trade))
                    {
                        Console.WriteLine(category.Name);
                        break;
                    }
                }
            }
        }

    }
}