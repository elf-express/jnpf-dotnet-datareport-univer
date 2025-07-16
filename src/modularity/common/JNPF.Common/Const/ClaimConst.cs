﻿using JNPF.DependencyInjection;

namespace JNPF.Common.Const;

/// <summary>
/// Claim常量.
/// </summary>
[SuppressSniffer]
public class ClaimConst
{
    /// <summary>
    /// 用户Id.
    /// </summary>
    public const string CLAINMUSERID = "UserId";

    /// <summary>
    /// 用户姓名.
    /// </summary>
    public const string CLAINMREALNAME = "UserName";

    /// <summary>
    /// 账号.
    /// </summary>
    public const string CLAINMACCOUNT = "Account";

    /// <summary>
    /// 是否超级管理.
    /// </summary>
    public const string CLAINMADMINISTRATOR = "Administrator";

    /// <summary>
    /// 租户ID.
    /// </summary>
    public const string TENANTID = "TenantId";

    /// <summary>
    /// 单点登录标识.
    /// </summary>
    public const string OnlineTicket = "OnlineTicket";
}