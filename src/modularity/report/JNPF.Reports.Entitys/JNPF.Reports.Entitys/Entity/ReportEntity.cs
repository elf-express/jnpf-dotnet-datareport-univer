using JNPF.Common.Contracts;
using SqlSugar;

namespace JNPF.Reports.Entitys.Entity;

[SugarTable("REPORT_TEMPLATE")]
public class ReportEntity : CLDSEntityBase
{
    /// <summary>
    /// 主版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION_ID")]
    public string VersionId { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULL_NAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_EN_CODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY")]
    public string Category { get; set; }

    /// <summary>
    /// 导出.
    /// </summary>
    [SugarColumn(ColumnName = "F_ALLOW_EXPORT")]
    public int AllowExport { get; set; }

    /// <summary>
    /// 打印.
    /// </summary>
    [SugarColumn(ColumnName = "F_ALLOW_PRINT")]
    public int AllowPrint { get; set; }

    /// <summary>
    /// 发布时勾选平台类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_PLATFORM_RELEASE")]
    public string platformRelease { get; set; }

    /// <summary>
    /// 描述或说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}
