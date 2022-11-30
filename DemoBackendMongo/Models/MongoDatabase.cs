using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace DemoModels
{
    public static class MongoDatabase
    {
        private static Dictionary<Guid, Product> ProductsDict { get; set; } = new Dictionary<Guid, Product>();
        public static IMongoCollection<Product> Products { get; set; }
        public static bool Initielized { get; set; }

        public static void InitRandomData()
        {
            if (Initielized)
                return;
            Initielized = true;

            var db = GetDb();
            Products = GetCollection<Product>(db);

            var prodcount = 100000;
            
            if ((Products?.CountDocuments(new MongoDB.Bson.BsonDocument()) ?? 0 ) == prodcount)
                return;

            db.DropCollection("Product");

            Products = GetCollection<Product>(db);

            for (int i = 1; i <= prodcount; i++)
            {
                var p = RandomProduct.GenerateProduct(i, prodcount);
                if (i == 1)
                    p.Name = "Product, with spec chars:(', &?) in it's name, asdf. fdsafasdf sadfasd .";
                if (i == 2)
                    p.Code = "Prod C0000002";
                ProductsDict.Add(p.Id, p);

            }
            var inmem = ProductsDict.Select(x => x.Value).AsEnumerable();
            Products.InsertMany(inmem);
            ;
        }


        private static MongoClient GetClient()
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            var dbConnectionString = Environment.GetEnvironmentVariable("SmBlazorMongoConnectionString") ?? "";
            var settings = MongoClientSettings.FromUrl(new MongoUrl(dbConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
#if DEBUG
                    var actSettingsJson = e.Command.ToJson(new MongoDB.Bson.IO.JsonWriterSettings() { Indent = true,});
                    File.WriteAllText($@"d:\temp\LastMongoCommand.{e.CommandName}.json", actSettingsJson);
# endif
                });
            };
            return new MongoClient(settings);
        }
        public static IMongoDatabase GetDb()
        {
            var dbName = "SmDemoData";
            var res = GetClient().GetDatabase(dbName);
            return res;
        }
        public static IMongoCollection<T> GetCollection<T>(IMongoDatabase? db = null)
        {
            var collection = (db ?? GetDb()).GetCollection<T>(typeof(T).Name);
            return collection;
        }


    }


    
}
