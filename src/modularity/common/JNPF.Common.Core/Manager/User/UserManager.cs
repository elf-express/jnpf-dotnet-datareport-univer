using JNPF.Common.Const;
using JNPF.Common.Entitys;
using JNPF.Common.Models.User;
using JNPF.DependencyInjection;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.Security.Claims;

namespace JNPF.Common.Core.Manager;

/// <summary>
/// 当前登录用户.
/// </summary>
public class UserManager : IUserManager, IScoped
{
    /// <summary>
    /// 用户表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<UserEntity> _repository;

    /// <summary>
    /// 当前Http请求.
    /// </summary>
    private readonly HttpContext _httpContext;

    /// <summary>
    /// 用户Claim主体.
    /// </summary>
    private readonly ClaimsPrincipal _user;

    /// <summary>
    /// 初始化一个<see cref="UserManager"/>类型的新实例.
    /// </summary>
    /// <param name="repository">用户仓储.</param>
    public UserManager(
        ISqlSugarRepository<UserEntity> repository)
    {
        _repository = repository;
        _httpContext = App.HttpContext;
        _user = _httpContext?.User;
    }

    /// <summary>
    /// 用户信息.
    /// </summary>
    public UserEntity User
    {
        get
        {
            if (_userEntity == null) _userEntity = _repository.GetSingle(u => u.Id == UserId);
            return _userEntity;
        }
    }
    private UserEntity _userEntity { get; set; }

    /// <summary>
    /// 用户ID.
    /// </summary>
    public string UserId
    {
        get => _user.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
    }

    /// <summary>
    /// 用户账号.
    /// </summary>
    public string Account
    {
        get => _user.FindFirst(ClaimConst.CLAINMACCOUNT)?.Value;
    }

    /// <summary>
    /// 用户昵称.
    /// </summary>
    public string RealName
    {
        get => _user.FindFirst(ClaimConst.CLAINMREALNAME)?.Value;
    }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId
    {
        get => _user.FindFirst(ClaimConst.TENANTID)?.Value;
    }

    /// <summary>
    /// 当前用户 token.
    /// </summary>
    public string ToKen
    {
        get => string.IsNullOrEmpty(App.HttpContext?.Request.Headers["Authorization"]) ? App.HttpContext?.Request.Query["token"] : App.HttpContext?.Request.Headers["Authorization"];
    }

    /// <summary>
    /// 获取请求端类型 pc 、 app.
    /// </summary>
    public string UserOrigin
    {
        get => _httpContext?.Request.Headers["jnpf-origin"];
    }

    /// <summary>
    /// 获取用户登录信息.
    /// </summary>
    /// <returns></returns>
    public async Task<UserInfoModel> GetUserInfo()
    {
        return await _repository.AsQueryable().Where(it => it.Id == UserId).Select(a => new UserInfoModel
        {
            userId = a.Id,
            headIcon = SqlFunc.MergeString("/api/File/Image/userAvatar/", a.HeadIcon),
            userAccount = a.Account,
            userName = a.RealName,
            gender = a.Gender,
            organizeId = a.OrganizeId,
            managerId = a.ManagerId,
            positionId = a.PositionId,
            roleId = a.RoleId,
            prevLoginTime = a.PrevLogTime,
            landline = a.Landline,
            telePhone = a.TelePhone,
            manager = SqlFunc.Subqueryable<UserEntity>().EnableTableFilter().Where(u => u.Id == a.ManagerId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
            mobilePhone = a.MobilePhone,
            email = a.Email,
            birthday = a.Birthday,
            systemId = a.SystemId,
            appSystemId = a.AppSystemId,
            changePasswordDate = a.ChangePasswordDate
        }).FirstAsync();
    }
}