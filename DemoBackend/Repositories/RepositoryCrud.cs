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

    public async Task<T?> Create(TKey key, T value)
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
                    await efDb.SaveChangesAsync();
                }
                break;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                await tableMongo.InsertOneAsync(value);
                break;
        }

        return value;
    }


    public async Task<T?> Read(TKey key)
    {
        T? res;
        switch (RepositoryAdmin.DbType)
        {
            case DatabaseType.Dictionary:
                return DictionaryDatabase.GetTable<T, TKey>().TryGetValue(key, out res)? res:null;
            case DatabaseType.EfPg:
                using (var efDb = new Database.SmDemoProductContext())
                {
                    return await efDb.FindAsync<T>(key);
                }
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                return await tableMongo.Find(x => x.Id.Equals(key)).SingleOrDefaultAsync();

        }
        return null;
    }
    public async Task<T?> Update(TKey key, T value)
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
                    await efDb.SaveChangesAsync();
                }
                break;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                await tableMongo.ReplaceOneAsync(x => x.Id.Equals(key), value);
                break;
        }

        return value;
    }

    public async Task<bool> Delete(TKey key)
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
                    var value = await efDb.FindAsync<T>(key);
                    if (value == null)
                        return false;

                    efDb.Remove(value);
                    await efDb.SaveChangesAsync();
                }
                return true;
            case DatabaseType.Mongo:
                var dbMongo = SmDemoProductMongoDatabase.GetDb();
                var tableMongo = SmDemoProductMongoDatabase.GetCollection<T>(dbMongo);
                var res = await tableMongo.DeleteOneAsync(x => x.Id.Equals(key));
                return res.DeletedCount == 1;
        }

        return false;
    }

   

}
