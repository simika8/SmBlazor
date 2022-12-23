using Controllers;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Common;
using SmQueryOptionsNs;
using static Controllers.AdminController;
using Database;
using DemoModels;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Reporitory;

public interface IHasId<TKey>
{
    TKey Id { get; set;}
}

public class RepositoryCrud<T, TKey> where T : class, IHasId<TKey>/*, new()*/ where TKey : notnull
{
    

    public RepositoryCrud(){
        RepositoryAdmin.InitRandomData();
    }

    public T? Create(TKey key, T value)
    {
        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                DictionaryDatabase.GetTable<T, TKey>()[key] = value;
                break;
            case DatabaseType.EfPg:
                using (var efDb = new Database.SmDemoProductContext())
                {
                    var efTable = efDb.Set<T>();
                    efTable.Add(value);
                    efDb.SaveChanges();
                }
                break;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                tableMongo.InsertOne(value);
                break;
        }

        return value;
    }


    public bool Read(TKey key, out T value)
    {
        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                return DictionaryDatabase.GetTable<T, TKey>().TryGetValue(key, out value);
            case DatabaseType.EfPg:
                using (var efDb = new Database.SmDemoProductContext())
                {
                    value = efDb.Find<T>(key);
                }
                return value != null;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                value = tableMongo.Find(x => x.Id.Equals(key)).SingleOrDefault();

                return value != null;
        }

        value = null!;
        return false;


    }
    public T? Update(TKey key, T value)
    {
        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                DictionaryDatabase.GetTable<T, TKey>()[key] = value;
                break;
            case DatabaseType.EfPg:
                using (var efDb = new Database.SmDemoProductContext())
                {
                    var efTable = efDb.Set<T>();
                    efTable.Attach(value);
                    efDb.Entry<T>(value).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    efDb.SaveChanges();
                }
                break;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                tableMongo.ReplaceOne(x => x.Id.Equals(key), value);
                break;
        }

        return value;
    }

    public bool Delete(TKey key)
    {
        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                var found = DictionaryDatabase.GetTable<T, TKey>().ContainsKey(key);
                if (found)
                {
                    DictionaryDatabase.GetTable<T, TKey>().Remove(key);
                }
                return found;
            case DatabaseType.EfPg:
                using (var efDb = new Database.SmDemoProductContext())
                {
                    var value = efDb.Find<T>(key);
                    if (value == null)
                        return false;

                    efDb.Remove(value);
                    efDb.SaveChanges();
                }
                return true;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                var res = tableMongo.DeleteOne(x => x.Id.Equals(key));
                return res.DeletedCount == 1;
        }

        return false;
    }

   

}
