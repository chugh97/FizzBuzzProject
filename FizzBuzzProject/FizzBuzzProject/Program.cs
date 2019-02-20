using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FizzBuzzProject
{
    class Program
    {
        enum PolicyType
        {
            A = 3,
            B = 5,
            C = 7,
            Unknown
        }

        enum BooleanAliases
        {
            N = 0,
            Y = 1
        }

        class Transaction
        {
            public string PolicyNumber { get; set; }
            public DateTime PolicyStartDate { get; set; }
            public decimal Premiums { get; set; }
            public bool Membership { get; set; }
            public decimal DiscretionaryBonus { get; set; }
            public decimal UpliftPercentage { get; set; }

            public static Transaction FromLine(string line)
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

            public static bool FromString(string str)
            {
                return Convert.ToBoolean(Enum.Parse(typeof(BooleanAliases), str));
            }

            public static DateTime GetTransactionDate(string input)
            {
                return DateTime.ParseExact(input, "yyyy-MM-dd", CultureInfo.CurrentCulture);
            }
        } 
        
        static IList<Transaction> ReadTransactions(string path, bool hasHeaders = true)
        {
            var list = new List<Transaction>();
            foreach(var line in File.ReadLines(path).Skip(hasHeaders ? 1 : 0))
            {
                list.Add(Transaction.FromLine(line));
            }

            return list;
        }
        
        static void GenerateXML(IEnumerable<Transaction> transactions)
        {
            XDocument xdoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Policies", 
                from transaction in transactions
                select
                  new XElement("Policy", new XAttribute("StartDate", transaction.PolicyStartDate),
                  new XElement("PolicyNumber", transaction.PolicyNumber),
                  new XElement("MaturityAmount", CalculateMaturity(transaction)))));

            xdoc.Save("C:/FizzBuzzProject/FizzBuzzProject/output.xml");
        }  
        
        static decimal CalculateMaturity(Transaction transaction)
        {
            int managementFee = GetPercentageOffer(transaction);
            return ((transaction.Premiums - (transaction.Premiums * managementFee / 100)) + transaction.DiscretionaryBonus) * transaction.UpliftPercentage / 100;
        }

        static int GetPercentageOffer(Transaction transaction)
        {
            string criteriaCheck = "1900-01-01";

            DateTime checkPolicyDate = Transaction.GetTransactionDate(criteriaCheck);

            if (transaction.PolicyStartDate >= checkPolicyDate && transaction.Membership) return (int)PolicyType.C;
            if (transaction.PolicyStartDate <= checkPolicyDate && transaction.Membership) return (int)PolicyType.B;
            if (transaction.PolicyStartDate <= checkPolicyDate) return (int)PolicyType.A;

            return 0;
        }

        static void Main(string[] args)
        {
            args = new[] { "../../MaturityData.csv" };

            var transactions = ReadTransactions(args[0]);
            GenerateXML(transactions);

            Console.ReadLine();
        }
    }
}
