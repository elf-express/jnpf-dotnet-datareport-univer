using JNPF.Common.Contracts;
using SqlSugar;

namespace JNPF.Reports.Entitys.Entity;

[SugarTable("REPORT_VERSION")]
public class ReportVersionEntity : CLDSEntityBase
{
    /// <summary>
    /// 主版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATE_ID")]
    public string TemplateId { get; set; }

    /// <summary>
    /// 版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_VERSION")]
    public int? Version { get; set; }

    /// <summary>
    /// 状态(0.设计中,1.启用中,2.已归档).
    /// </summary>
    [SugarColumn(ColumnName = "F_STATE")]
    public int? State { get; set; }

    /// <summary>
    /// 模板json.
    /// </summary>
    [SugarColumn(ColumnName = "F_SNAPSHOT")]
    public string Snapshot { get; set; }

    /// <summary>
    /// 模板json.
    /// </summary>
    [SugarColumn(ColumnName = "F_CELLS")]
    public string Cells { get; set; }

    /// <summary>
    /// 模板json.
    /// </summary>
    [SugarColumn(ColumnName = "F_QUERY_LIST")]
    public string QueryList { get; set; }

    /// <summary>
    /// 模板json.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONVERT_CONFIG")]
    public string ConvertConfig { get; set; }

    /// <summary>
    /// 模板json.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORT_LIST")]
    public string SortList { get; set; }

    /// <summary>
    /// 描述或说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}
