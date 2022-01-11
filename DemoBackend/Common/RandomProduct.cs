using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class RandomProduct
    {
        private static readonly char[] BaseChars =
         "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly Dictionary<char, int> CharValues = BaseChars
                   .Select((c, i) => new { Char = c, Index = i })
                   .ToDictionary(c => c.Char, c => c.Index);

        private static int FirstWordLength = 6;
        private static int MinProductNameValue = (int)BaseToLong("".PadLeft(FirstWordLength, 'a'));
        private static int MaxProductNameValue = (int)BaseToLong("".PadLeft(FirstWordLength, 'z'));
        public static Guid TestTenantId = TestGuid("TestTenant");


        public static List<Product> Generate(string keyword, string? cursor, int take)
        {
            var res = new List<Product>();


            var cursorDecoded = cursor == null ? "" : System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(cursor));
            var range = RandomProduct.GetVisibleProductNumberRange(keyword, cursorDecoded, 1, 200000);


            for (var x = 0; x < 1; x++)
            {
                res.Clear();


                for (var productNumber = range.first; productNumber <= Math.Min(range.first + take - 1, range.last); productNumber++)
                {
                    var rnd = new Random(productNumber);
                    var name = RandomProduct.NumberToProductName(productNumber, 1, 200000);
                    if (name != null)
                    {
                        var id = TestGuid("Product" + productNumber.ToString());
                        var p = GenerateProduct(productNumber, 200000);

                        res.Add(p);
                    }
                }
            }
            return res;
        }

        public static Product GenerateProduct(int productNumber, int lastProductNumber)
        {
            var rnd = new Random(productNumber);
            var id = TestGuid("Product" + productNumber.ToString());
            var name = RandomProduct.NumberToProductName(productNumber, 1, lastProductNumber);
            var p = new Product()
            {
                //Id = Guid.NewGuid(),
                Id = id,
                Active = rnd.Next(2) == 0,
                Ext = new ProductExt()
                {
                    ProductId = id,
                    Description = RandomProduct.RandomSentences(rnd.Next(5, 10), rnd.Next(3, 10), rnd),
                    MinimumStock = rnd.Next(1, 10),
                },
                Name = name + " (" + productNumber + ")",
                Price = rnd.Next(5000) + 100 + ((productNumber % 10) == 2 ? 0 : 0.1),
                Rating = rnd.Next(1, 5),
                
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2000, 01, 01), DateTimeKind.Utc).AddMilliseconds(productNumber * 30).AddHours(productNumber),
                Type = productNumber % 10 == 3? ProductType.Service: ProductType.Product,
            };
            if (IsPrime(productNumber) && p.Type == ProductType.Product)
            {
                var stockcount = (productNumber % 10) == 1 ?2:1;
                for (int i = 0; i < stockcount ; i++)
                {
                    p.Stocks = new List<InventoryStock>();
                    p.Stocks.Add(new InventoryStock()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = id,
                        StoreId = TestGuid("Store " + i.ToString()),
                        Quantity = rnd.Next(1, 10),
                    });
                }
            }

            bool IsPrime(int candidate)
            {
                if (candidate % 2 <= 0)
                {
                    return candidate == 2;
                }
                int power2 = 9;
                for (int divisor = 3; power2 <= candidate; divisor += 2)
                {
                    if (candidate % divisor == 0)
                        return false;
                    power2 += divisor * 4 + 4;
                }
                return true;
            }

            return p;

        }
        public static string? NumberToProductName(int productNumber, int firstProductNumber, int lastProductNumber)
        {
            var rnd = new Random(productNumber);
            var productNameValuePercent = (double)(productNumber - firstProductNumber) 
                / (lastProductNumber - firstProductNumber);
            var productNameValue = (int)(productNameValuePercent
                * (MaxProductNameValue - MinProductNameValue)
                + MinProductNameValue);
            try
            {
                var firstWord = LongToBase(productNameValue).PadLeft(FirstWordLength, 'a');
                return FirstCharToUpper(firstWord) + " " + RandomWord(rnd.Next(5) + 3, rnd).ToLower() + " " + RandomWord(rnd.Next(5) + 3, rnd).ToLower();
            } catch
            {
                return null;
            }
        } 
        public static double ProductNameToNumber(string productName, int firstProductNumber, int lastProductNumber)
        {
            

            var productNameIndexOfSpace = productName.IndexOf(" ");
            var firstWord = productNameIndexOfSpace > -1
                ? productName.Substring(0, productNameIndexOfSpace)
                : productName;
            try
            {
                var productNameValue = BaseToLong(firstWord.ToLower());
                var productNameValuePercent = (double)(productNameValue - MinProductNameValue) / (MaxProductNameValue - MinProductNameValue);
                var productNumber = productNameValuePercent * (lastProductNumber - firstProductNumber) + firstProductNumber;
                //return (int)Math.Round(productNumber);
                return productNumber;
            }
            catch
            {
                return 0;
            }
        }
        public static (int first, int last) GetVisibleProductNumberRange(string keyword, string cursorDecoded, int firstProductNumber, int lastProductNumber)
        {
            var cursor = ProductNameToNumber(cursorDecoded.PadRight(FirstWordLength, 'a').Substring(0, FirstWordLength), firstProductNumber, lastProductNumber);
            var first = ProductNameToNumber(keyword.PadRight(FirstWordLength, 'a').Substring(0, FirstWordLength), firstProductNumber, lastProductNumber);
            var last = ProductNameToNumber(keyword.PadRight(FirstWordLength, 'z').Substring(0, FirstWordLength), firstProductNumber, lastProductNumber);
            
            if (cursor > first)
                return ((int)Math.Round(cursor)+1, (int)Math.Floor(last));

            return ((int)Math.Ceiling(first), (int)Math.Floor(last));
            //return ((int)first, (int)last);
        }


        private static char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static char[] numbers = "0123456789".ToCharArray();
        public static string RandomWord(int count, Random rnd)
        {
            string res = "";
            for (int i = 0; i < count; i++)
            {
                res += letters[rnd.Next(0, letters.Length - 1)];
            }
            return res;
        }
        public static string RandomSentence(int count, Random rnd)
        {
            string res = RandomWord(1, rnd).ToUpper();
            for (int i = 0; i < count; i++)
            {
                res += RandomWord(rnd.Next(4) * rnd.Next(4) + rnd.Next(3) + 1, rnd).ToLower();
                if (i != 0)
                {
                    if (i == count - 1)
                    {
                        res += ".";
                    }
                    else
                    {
                        res += " ";
                    }

                }
            }
            return res;
        }
        public static string RandomSentences(int count, int Wordcount, Random rnd)
        {
            string res = "";
            for (int i = 0; i < count; i++)
            {
                res += RandomSentence(Wordcount, rnd);
                if (i != count - 1)
                {
                    res += " ";
                }

            }
            return res;
        }
        public static string RandomNumber(int count, Random rnd)
        {
            string res = "";
            for (int i = 0; i < count; i++)
            {
                res += numbers[rnd.Next(0, numbers.Length - 1)];
            }
            return res;
        }



        public static string LongToBase(long value)
        {
            long targetBase = BaseChars.Length;
            // Determine exact number of characters to use.
            char[] buffer = new char[Math.Max(
                       (int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = buffer.Length;
            do
            {
                buffer[--i] = BaseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            return new string(buffer, i, buffer.Length - i);
        }

        public static long BaseToLong(string number)
        {
            char[] chrs = number.ToCharArray();
            int m = chrs.Length - 1;
            int n = BaseChars.Length, x;
            long result = 0;
            for (int i = 0; i < chrs.Length; i++)
            {
                x = CharValues[chrs[i]];
                result += x * (long)Math.Pow(n, m--);
            }
            return result;
        }
        
        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };
        public static Guid TestGuid(string s)
        {
            var s2 = s.PadLeft(16, '\0');
            byte[] b = Encoding.UTF8.GetBytes(s2);
            return new Guid(b);


        }
    }
}
