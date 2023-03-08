using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleApp_CSCase.Category
{
    internal class Expired : ICategory
    {
        private readonly DateTime referenceDate;

        public string Name
        {
            get
            {
                return "EXPIRED";
            }
        }
        public Expired(string _referenceDate)
        {
            referenceDate = DateTime.ParseExact(_referenceDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        }

        public bool Identify(ITrade trade)
        {
            return referenceDate > trade.NextPaymentDate.AddDays(30);
        }
    }

    internal class MediumRisk : ICategory
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

    internal class HighRisk : ICategory
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

    internal class PEP : ICategory
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

    internal class NotCategorised : ICategory
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
}
