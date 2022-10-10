﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace DemoModels
{
    public static class DataTables
    {
        public static Dictionary<Guid, Product> Products { get; set; } = new Dictionary<Guid, Product>();
        public static bool Initielized { get; set; }

        public static void InitRandomData()
        {
            if (Initielized)
                return;
            Initielized = true;

            var prodcount = 100000;
            for (int i = 1; i <= prodcount; i++)
            {
                var p = RandomProduct.GenerateProduct(i, prodcount);
                if (i == 1)
                    p.Name = "Product, with spec chars:(', &?) in it's name, asdf. fdsafasdf sadfasd .";
                if (i == 2)
                    p.Code = "Prod C0000002";
                Products.Add(p.Id, p);

            }
            
        }


    }


    
}
