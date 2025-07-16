using JNPF.Common.Enums;
using JNPF.ConfigurableOptions;

namespace JNPF.Common.Options;

/// <summary>
/// JNPF基本配置.
/// </summary>
public sealed class AppOptions : IConfigurableOptions
{
    /// <summary>
    /// 系统文件路径.
    /// </summary>
    public string SystemPath { get; set; }

    /// <summary>
    /// 微信公众号允许上传文件类型.
    /// </summary>
    public List<string> MPUploadFileType { get; set; }

    /// <summary>
    /// 允许图片类型.
    /// </summary>
    public List<string> AllowUploadImageType { get; set; }

    /// <summary>
    /// 允许上传文件类型.
    /// </summary>
    public List<string> AllowUploadFileType { get; set; }

    /// <summary>
    /// 过滤上传文件名称特殊字符.
    /// </summary>
    public List<string> SpecialString { get; set; }
}