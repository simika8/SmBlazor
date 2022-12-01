using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using DemoModels;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Database
{
    public static class SmDemoProductMongoDatabase
    {
        private static Dictionary<Guid, Product> ProductsDict { get; set; } = new Dictionary<Guid, Product>();
        public static IMongoCollection<Product> Product { get; set; }
        public static bool Initialized { get; set; }

        public static void InitRandomData()
        {
            if (Initialized)
                return;
            Initialized = true;

            var db = GetDb();
            Product = GetCollection<Product>(db);

            var prodCount = 100000;

            var oldcount = Product?.CountDocuments(new MongoDB.Bson.BsonDocument()) ?? 0;
            if (oldcount == prodCount)
                return;

            db.DropCollection("Product");

            Product = GetCollection<Product>(db);

            var inmem = DictionaryDatabase.GetProductDict(prodCount).Select(x => x.Value).ToList();
            var inmemchunked = inmem.ChunkBy(1000);
            foreach (var chunk in inmemchunked)
            {
                Product.InsertMany(chunk);
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
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
            var contextName = nameof(SmDemoProductMongoDatabase);
            var databaseName = contextName.Remove(contextName.Length - "MongoDatabase".Length);

            var res = GetClient().GetDatabase(databaseName);
            return res;
        }
        public static IMongoCollection<T> GetCollection<T>(IMongoDatabase? db = null)
        {
            var collection = (db ?? GetDb()).GetCollection<T>(typeof(T).Name);
            return collection;
        }


    }


    
}
