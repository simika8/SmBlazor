using Controllers;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Common;
using SmQueryOptionsNs;
using static Controllers.AdminController;

namespace Database;

public class CrudRepo<T, TKey> where T : class where TKey : notnull
{
    public static DatabaseType DbType { get; set; } = DatabaseType.Dictionary;

    public Dictionary<TKey, T> Table { get; set; } = new() { };

    public CrudRepo(Dictionary<TKey, T> table){
        Table = table;
    }
    public void InitRandomData()
    {
        switch (CrudRepo<T, TKey>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                Database.DictionaryDatabase.InitRandomData();
                break;
            case AdminController.DatabaseType.EfPg:
                Database.SmDemoProductContext.InitRandomData();
                break;
            case AdminController.DatabaseType.Mongo:
                Database.SmDemoProductMongoDatabase.InitRandomData();
                break;
        }

    }

    public T? Create(TKey key, T value)
    {
        switch (CrudRepo<T, TKey>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                Table[key] = value;
                break;
            case AdminController.DatabaseType.EfPg:
                break;
            case AdminController.DatabaseType.Mongo:
                break;
        }

        return value;
    }


    public bool Read(TKey key, out T value)
    {
        using var db = new Database.SmDemoProductContext();
        switch (CrudRepo<T, TKey>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                return Table.TryGetValue(key, out value);
            case AdminController.DatabaseType.EfPg:
                value = db.Find<T>(key);
                return true;
            case AdminController.DatabaseType.Mongo:
                break;
        }

        value = null;
        return false;


    }
    public T? Update(TKey key, T value)
    {
        switch (CrudRepo<T, TKey>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                Table[key] = value;
                break;
            case AdminController.DatabaseType.EfPg:
                value = UpdateEf(key, value);
                break;
            case AdminController.DatabaseType.Mongo:
                break;
        }

        return value;
    }
    public T? UpdateEf(TKey key, T value)
    {
        using var db = new Database.SmDemoProductContext();
        var table = db.Set<T>();

        table.Attach(value);
        db.Entry<T>(value).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        db.SaveChanges();

        return value;
    }

    public bool Delete(TKey key)
    {
        switch (CrudRepo<T, TKey>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                var found = Table.ContainsKey(key);
                if (found)
                {
                    Table.Remove(key);
                }
                return found;
            case AdminController.DatabaseType.EfPg:
                break;
            case AdminController.DatabaseType.Mongo:
                break;
        }

        return false;
    }

   

}
