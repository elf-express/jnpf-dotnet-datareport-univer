using SqlSugar;

namespace JNPF.Common.Core.Manager;

/// <summary>
/// 切换数据库抽象.
/// </summary>
public interface IDataBaseManager
{
    #region 公共

    /// <summary>
    /// 获取租户SqlSugarClient客户端.
    /// </summary>
    /// <param name="tenantId">租户id.</param>
    /// <returns></returns>
    ISqlSugarClient GetTenantSqlSugarClient(string tenantId, GlobalTenantCacheModel globalTenantCache = null);
    #endregion
}