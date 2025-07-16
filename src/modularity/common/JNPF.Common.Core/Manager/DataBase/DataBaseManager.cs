using JNPF.Common.Const;
using JNPF.Common.Manager;
using JNPF.DependencyInjection;
using JNPF.Extras.DatabaseAccessor.SqlSugar.Models;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace JNPF.Common.Core.Manager;

/// <summary>
/// 实现切换数据库后操作.
/// </summary>
public class DataBaseManager : IDataBaseManager, ITransient
{
    /// <summary>
    /// 初始化客户端.
    /// </summary>
    private static SqlSugarScope? _sqlSugarClient;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 多租户配置选项.
    /// </summary>
    private readonly TenantOptions _tenant;

    /// <summary>
    /// 默认数据库配置.
    /// </summary>
    private readonly DbConnectionConfig defaultConnectionConfig;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public DataBaseManager(
        IOptions<ConnectionStringsOptions> connectionOptions,
        IOptions<TenantOptions> tenantOptions,
        ISqlSugarClient context,
        ICacheManager cacheManager)
    {
        _sqlSugarClient = (SqlSugarScope)context;
        _tenant = tenantOptions.Value;
        _cacheManager = cacheManager;
        defaultConnectionConfig = connectionOptions.Value.DefaultConnectionConfig;
    }

    #region 公共

    /// <summary>
    /// 获取租户SqlSugarClient客户端.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    public ISqlSugarClient GetTenantSqlSugarClient(string tenantId, GlobalTenantCacheModel globalTenantCache = null)
    {
        var tenant = globalTenantCache ?? GetGlobalTenantCache(tenantId);
        if (!_sqlSugarClient.AsTenant().IsAnyConnection(tenantId))
        {
            _sqlSugarClient.AddConnection(new ConnectionConfig()
            {
                ConfigId = tenant.TenantId,
                DbType = tenant.connectionConfig.ConfigList.FirstOrDefault().dbType,
                ConnectionString = tenant.connectionConfig.ConfigList.FirstOrDefault().connectionStr,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
            });
        }
        _sqlSugarClient.ChangeDatabase(tenantId);
        var tenantFilterValue = tenant != null && !"default".Equals(tenantId) && tenant.type == 1 ? tenant.connectionConfig.IsolationField : "0";
        _sqlSugarClient.QueryFilter.Clear();
        _sqlSugarClient.QueryFilter.AddTableFilter<ITenantFilter>(it => it.TenantId == tenantFilterValue);
        _sqlSugarClient.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                entityInfo.SetValue(tenantFilterValue);
            }
            if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                entityInfo.SetValue(tenantFilterValue);
            }
            if (entityInfo.PropertyName == "TenantId" && entityInfo.OperationType == DataFilterType.DeleteByObject)
            {
                entityInfo.SetValue(tenantFilterValue);
            }
        };

        _sqlSugarClient.Ado.CommandTimeOut = 30;

        _sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
        {
            if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Green;
            if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.White;
            if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                Console.ForegroundColor = ConsoleColor.Blue;

            // 在控制台输出sql语句
            Console.WriteLine("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(_sqlSugarClient.CurrentConnectionConfig.DbType, sql, pars) + "\r\n");
        };

        _sqlSugarClient.Aop.OnError = (ex) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            var pars = _sqlSugarClient.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
            Console.WriteLine("【" + DateTime.Now + "——错误SQL】\r\n" + UtilMethods.GetSqlString(_sqlSugarClient.CurrentConnectionConfig.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n");
        };

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
        {
            _sqlSugarClient.Aop.OnExecutingChangeSql = (sql, pars) =>
            {
                if (pars != null)
                {
                    foreach (var item in pars)
                    {
                        // 如果是DbTppe=string设置成OracleDbType.Nvarchar2
                        item.IsNvarchar2 = true;
                    }
                }
                return new KeyValuePair<string, SugarParameter[]>(sql, pars);
            };
        }
        return _sqlSugarClient;
    }

    /// <summary>
    /// 获取全局租户缓存.
    /// </summary>
    /// <returns></returns>
    private GlobalTenantCacheModel GetGlobalTenantCache(string tenantId)
    {
        string cacheKey = string.Format("{0}", CommonConst.GLOBALTENANT);
        return _cacheManager.Get<List<GlobalTenantCacheModel>>(cacheKey).Find(it => it.TenantId.Equals(tenantId));
    }
    #endregion
}