using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Authentication.ExtendedProtection;
using ConsoleApp_CSCase.Category;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using Container = StructureMap.Container;

namespace ConsoleApp_CSCase
{
    interface ICategory
    {
        string Name { get; }
        bool Identify(ITrade trade);
    }
    interface ITrade
    {
        double Value { get; }
        string ClientSector { get; }
        DateTime NextPaymentDate { get; }
        bool IsPoliticallyExposed { get; }
    }


    [Flags]
    enum EnumAllCategories
    {
        Expired,
        MediumRisk,
        HighRisk,
        PEP,
        NotCategorised,
    }

    class Program
    {
        public class GetCategoryFactory
        {
            public ICategory Create(string categoryAssemblyName, string referenceDate)
            {
                var type = Type.GetType(categoryAssemblyName ?? throw new InvalidOperationException());
                if (type == typeof(Expired))
                {
                    return Activator.CreateInstance(type, referenceDate) as ICategory;
                }
                else
                {
                    return Activator.CreateInstance(type) as ICategory;
                }
            }

            public ICategory GetCategory(EnumAllCategories category, string referenceDate)
            {
                var categoryAssemblyName = $"ConsoleApp_CSCase.Category.{category}, ConsoleApp-CSCase";
               // Type t = typeof(MediumRisk);
               // var name = t.AssemblyQualifiedName;
                var type = Type.GetType(categoryAssemblyName ?? throw new InvalidOperationException());
                if (type == typeof(Expired))
                {
                    return Activator.CreateInstance(type, referenceDate) as ICategory;
                }
                else
                {
                    return Activator.CreateInstance(type) as ICategory;
                }
                
            }
            public ICategory GetCategory(string categoryType, string referenceDate)
            {
                if (categoryType == null)
                {
                    return null;
                }
                else if (categoryType == EnumAllCategories.Expired.ToString())
                {
                    return new Expired(referenceDate);
                }
                else if (categoryType == EnumAllCategories.MediumRisk.ToString())
                {
                    return new MediumRisk();
                }
                else if (categoryType == EnumAllCategories.HighRisk.ToString())
                {
                    return new HighRisk();
                }
                else if (categoryType == EnumAllCategories.PEP.ToString())
                {
                    return new PEP();
                }
                else if (categoryType == EnumAllCategories.NotCategorised.ToString())
                {
                    return new NotCategorised();
                }

                return null;
            }

            public List<ICategory> GetAllCategories(string referenceDate)
            {
                List<ICategory> categories = new List<ICategory>();
                // Option using Switch
                /*foreach (string categoryName in Enum.GetNames(typeof(EnumAllCategories)))
                {
                    categories.Add(this.GetCategory(categoryName, referenceDate));
                    
                }*/

                // Option using Activator
                /* foreach (EnumAllCategories category in Enum.GetValues(typeof(EnumAllCategories)))
                 {
                     categories.Add(this.GetCategory(category, referenceDate));
                 }*/

                // using classes in a namespace instead of enum
                 var types = Assembly
                     .GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.Namespace == "ConsoleApp_CSCase.Category");
                foreach (var category in types)
                {
                    categories.Add(this.Create(category.FullName, referenceDate));
                }

                return categories;

            }
        }

        public class Trade : ITrade
        {
            public double Value { get; }
            public string ClientSector { get; }
            public DateTime NextPaymentDate { get; }
            public bool IsPoliticallyExposed { get; }

            public Trade(string[] parameters)
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

            // Category Factory
            GetCategoryFactory categoryFactory = new GetCategoryFactory();
            List<ICategory> categories = categoryFactory.GetAllCategories(referenceDate);

            // Getting categories using dependency injection
            /*
            var services = new ServiceCollection()
                .AddSingleton<ICategory>(new  Expired(referenceDate))
                .AddSingleton<ICategory, HighRisk>()
                .AddSingleton<ICategory, MediumRisk>()
                .AddSingleton<ICategory, PEP>()
                .AddSingleton<ICategory, NotCategorised>()
                .BuildServiceProvider();

            var categories = services.GetServices<ICategory>();
            */

            // Getting categories using StructuredMap and dependency injection
            /*
            var services = new ServiceCollection();
            var container = new Container();
            container.Configure(config =>
            {
                config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(Program));
                    _.WithDefaultConventions();
                });
                config.Populate(services);
            });

            var serviceProvider = container.GetInstance<IServiceProvider>();
            
            IEnumerable<ICategory> categories = serviceProvider.GetServices<ICategory>();
            */

            // classifiy trades
            for (int i = 2; i < lines.Length; i++)
            {
                string[] tradeData = lines[i].Split(" ");
                Trade trade = new Trade(tradeData);

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