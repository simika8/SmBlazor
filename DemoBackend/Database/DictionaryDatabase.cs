using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using DemoModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace Database
{
    public static class DictionaryDatabase
    {
        public static Dictionary<Guid, Product> Products { get; set; } = new Dictionary<Guid, Product>();
        public static bool Initialized { get; set; }

        public static void InitRandomData()
        {
            if (Initialized)
                return;
            Initialized = true;

            /*var prodcount = 100000;
            for (int i = 1; i <= prodcount; i++)
            {
                var p = RandomProduct.GenerateProduct(i, prodcount);
                if (i == 1)
                    p.Name = "Product, with spec chars:(', &?) in it's name, asdf. fdsafasdf sadfasd .";
                if (i == 2)
                    p.Code = "Prod C0000002";
                Products.Add(p.Id, p);

            }*/
            Products = GetProductDict(100000);


        }
        public static Dictionary<Guid, Product> GetProductDict(int prodCount)
        {
            var products = new Dictionary<Guid, Product>();
            for (int i = 1; i <= prodCount; i++)
            {
                var p = RandomProduct.GenerateProduct(i, prodCount);
                if (i == 1)
                    p.Name = "Product, with spec chars:(', &?) in it's name, asdf. fdsafasdf sadfasd .";
                if (i == 2)
                    p.Code = "Prod C0000002";
                products.Add(p.Id, p);

            }
            return products;

        }

    }


    
}
