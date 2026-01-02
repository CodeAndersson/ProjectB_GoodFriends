using Microsoft.Extensions.Configuration;

namespace Services;

public class DataSourceActive : IDataSourceActive
{
    //classic singleton lock pattern using an instance lock object
    private readonly object s_instanceLock = new();

    //allow datasource shift att application level, through service injected as singleton
    private DataSource _datasource;
    public DataSource ActiveDataSource 
    {
        get 
        {
            lock (s_instanceLock)
            { 
                return _datasource;
            }
        }
        set 
        {
            lock (s_instanceLock)
            { 
                _datasource = value;
            }
        }
    }

    public DataSourceActive(IConfiguration configuration)
    {
        //At startup, read from configuration to determine the active datasource
        _datasource = configuration["DataService:DataSource"] switch {
            "WebApi" => DataSource.WebApi,
            _ => DataSource.SQLDatabase
        };
    }
}

