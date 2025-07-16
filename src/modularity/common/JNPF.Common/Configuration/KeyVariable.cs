﻿using JNPF.Common.Enums;
using JNPF.Common.Options;
using JNPF.DependencyInjection;
using SqlSugar;

namespace JNPF.Common.Configuration;

/// <summary>
/// Key常量.
/// </summary>
[SuppressSniffer]
public class KeyVariable
{
    private static readonly TenantOptions _tenant = App.GetConfig<TenantOptions>("Tenant", true);

    private static readonly AppOptions _jnfp = App.GetConfig<AppOptions>("JNPF_App", true);
    /// <summary>
    /// 多租户模式.
    /// </summary>
    public static bool MultiTenancy
    {
        get
        {
            return _tenant.MultiTenancy;
        }
    }

    /// <summary>
    /// 多租户模式.
    /// </summary>
    public static string MultiTenancyType
    {
        get
        {
            return _tenant.MultiTenancyType;
        }
    }

    /// <summary>
    /// 系统文件路径.
    /// </summary>
    public static string SystemPath
    {
        get
        {
            return _jnfp.SystemPath;
        }
    }

    /// <summary>
    /// 允许上传图片类型.
    /// </summary>
    public static List<string> AllowImageType
    {
        get
        {
            return string.IsNullOrEmpty(_jnfp.AllowUploadImageType.ToString()) ? new List<string>() : _jnfp.AllowUploadImageType;
        }
    }

    /// <summary>
    /// 允许上传文件类型.
    /// </summary>
    public static List<string> AllowUploadFileType
    {
        get
        {
            return string.IsNullOrEmpty(_jnfp.AllowUploadFileType.ToString()) ? new List<string>() : _jnfp.AllowUploadFileType;
        }
    }

    /// <summary>
    /// 过滤上传文件名称特殊字符.
    /// </summary>
    public static List<string> SpecialString
    {
        get
        {
            return string.IsNullOrEmpty(_jnfp.SpecialString.ToString()) ? new List<string>() : _jnfp.SpecialString;
        }
    }

    /// <summary>
    /// App版本.
    /// </summary>
    public static string AppVersion
    {
        get
        {
            return string.IsNullOrEmpty(App.Configuration["JNPF_APP:AppVersion"]) ? string.Empty : App.Configuration["JNPF_APP:AppVersion"];
        }
    }

    /// <summary>
    /// 文件储存类型.
    /// </summary>
    public static string AppUpdateContent
    {
        get
        {
            return string.IsNullOrEmpty(App.Configuration["JNPF_APP:AppUpdateContent"]) ? string.Empty : App.Configuration["JNPF_APP:AppUpdateContent"];
        }
    }
}