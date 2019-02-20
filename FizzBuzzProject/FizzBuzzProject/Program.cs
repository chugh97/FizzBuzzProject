using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FizzBuzzProject
{
    class Program
    {
        enum BooleanAliases
        {
            N = 0,
            Y = 1
        }

        public interface ITransaction
        {
            string PolicyNumber { get; set; }
            DateTime PolicyStartDate { get; set; }
            decimal Premiums { get; set; }
            bool Membership { get; set; }
            decimal DiscretionaryBonus { get; set; }
            decimal UpliftPercentage { get; set; }
            decimal CalculatePolicyPercentage();
        }

        public class Transaction : ITransaction
        {
            public string PolicyNumber { get; set; }
            public DateTime PolicyStartDate { get; set; }
            public decimal Premiums { get; set; }
            public bool Membership { get; set; }
            public decimal DiscretionaryBonus { get; set; }
            public decimal UpliftPercentage { get; set; }

            public decimal CalculatePolicyPercentage()
            {

                return ComputePercentage();
            }

            protected virtual decimal ComputePercentage()
            {
                decimal result = 0.0m;
                var minDate = new DateTime(1900, 1, 1);

                if (PolicyStartDate <= minDate)
                    result = 3.0m;

                if (PolicyStartDate >= minDate && Membership) 
                    result = 7.00m;

                if (PolicyStartDate <= minDate && Membership)
                    result = 5.00m;

                return result;
            }
        }

        public interface ITransactionManager
        {
            void CSVToXmlConvertor(string outputFileName);
        }

        public class TransactionManager : ITransactionManager
        {
            private readonly string _path;
            private readonly bool _hasHeaders;

            public TransactionManager(string path, bool hasHeaders)
            {
                _path = path;
                _hasHeaders = hasHeaders;
            }

            public void CSVToXmlConvertor(string outputFileName)
            {
                var transactions = ReadTransactions();

                XDocument xdoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Policies",
                    from transaction in transactions
                    select
                      new XElement("Policy", new XAttribute("StartDate", transaction.PolicyStartDate),
                      new XElement("PolicyNumber", transaction.PolicyNumber),
                      new XElement("MaturityAmount", CalculateMaturity(transaction)))));

                xdoc.Save(outputFileName);
            }

            private IList<ITransaction> ReadTransactions()
            {
                var list = new List<ITransaction>();
                //check if file exists - TODO: PB
                foreach (var line in File.ReadLines(_path).Skip(_hasHeaders ? 1 : 0))
                {
                    list.Add(GenerateTransaction(line));
                }

                return list;
            }

            private ITransaction GenerateTransaction(string line)
            {
                var data = line.Split(',');
                return new Transaction
                {
                    PolicyNumber = data[0],
                    PolicyStartDate = DateTime.Parse(data[1]),
                    Premiums = decimal.Parse(data[2]),
                    Membership = FromString(data[3]),
                    DiscretionaryBonus = decimal.Parse(data[4]),
                    UpliftPercentage = decimal.Parse(data[5])
                };
            }

            private bool FromString(string str)
            {
                return Convert.ToBoolean(Enum.Parse(typeof(BooleanAliases), str));
            }
            
            private decimal CalculateMaturity(ITransaction transaction)
            {
                int managementFee = Convert.ToInt32(transaction.CalculatePolicyPercentage());
                return ((transaction.Premiums - (transaction.Premiums * managementFee / 100)) + transaction.DiscretionaryBonus) * transaction.UpliftPercentage / 100;
            }
            
        }

        static void Main(string[] args)
        {
            args = new[] { "../../MaturityData.csv" };
            string outputPath = @"C:/FizzBuzzProject/FizzBuzzProject/output.xml";

            ITransactionManager tm = new TransactionManager(args[0], true);
            tm.CSVToXmlConvertor(outputFileName: outputPath);

            Console.ReadLine();
        }
    }
}
