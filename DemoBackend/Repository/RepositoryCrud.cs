using Controllers;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Common;
using SmQueryOptionsNs;
using static Controllers.AdminController;
using Database;

namespace Reporitory;

public class RepositoryCrud<T, TKey> where T : class where TKey : notnull
{
    

    public RepositoryCrud(){
        RepositoryAdmin.InitRandomData();
    }

    public T? Create(TKey key, T value)
    {
        switch (RepositoryAdmin.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                DictionaryDatabase.GetTable<T, TKey>()[key] = value;
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
        switch (RepositoryAdmin.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                return DictionaryDatabase.GetTable<T, TKey>().TryGetValue(key, out value);
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
        switch (RepositoryAdmin.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                DictionaryDatabase.GetTable<T, TKey>()[key] = value;
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
        switch (RepositoryAdmin.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                var found = DictionaryDatabase.GetTable<T, TKey>().ContainsKey(key);
                if (found)
                {
                    DictionaryDatabase.GetTable<T, TKey>().Remove(key);
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
