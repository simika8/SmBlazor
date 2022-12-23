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
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Database
{
    public static class DictionaryDatabase
    {
        public static Dictionary<Guid, Product> Products { get; set; } = new Dictionary<Guid, Product>();

        public static Dictionary<TKey, T> GetTable<T, TKey>() where T : class where TKey : notnull
        {
            if (typeof(T) == typeof(Product))
                return (Dictionary<TKey, T>)Convert.ChangeType(Products, typeof(Dictionary<TKey, T>));
            throw new NotImplementedException();
        }

    }


    
}
