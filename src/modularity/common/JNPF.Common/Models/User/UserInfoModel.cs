using JNPF.DependencyInjection;

namespace JNPF.Common.Models.User
{
    /// <summary>
    /// 登录者信息
    /// 版 本：V3.2.0
    /// 版 权：引迈信息技术有限公司（https://www.jnpfsoft.com）
    /// 作 者：JNPF开发平台组.
    /// </summary>
    [SuppressSniffer]
    public class UserInfoModel
    {
        /// <summary>
        /// 用户主键.
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 用户账户.
        /// </summary>
        public string userAccount { get; set; }

        /// <summary>
        /// 用户姓名.
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 用户头像.
        /// </summary>
        public string headIcon { get; set; }

        /// <summary>
        /// 用户性别.
        /// </summary>
        public string gender { get; set; }

        /// <summary>
        /// 座机号.
        /// </summary>
        public string landline { get; set; }

        /// <summary>
        /// 电话.
        /// </summary>
        public string telePhone { get; set; }

        /// <summary>
        /// 所属组织.
        /// </summary>
        public string organizeId { get; set; }

        /// <summary>
        /// 我的主管.
        /// </summary>
        public string managerId { get; set; }

        /// <summary>
        /// 岗位主键.
        /// </summary>
        public string positionId { get; set; }

        /// <summary>
        /// 角色主键.
        /// </summary>
        public string roleId { get; set; }

        /// <summary>
        /// 上次登录时间.
        /// </summary>
        public DateTime? prevLoginTime { get; set; }

        /// <summary>
        /// 直属主管.
        /// </summary>
        public string manager { get; set; }

        /// <summary>
        /// 手机号.
        /// </summary>
        public string mobilePhone { get; set; }

        /// <summary>
        /// 邮箱.
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 生日.
        /// </summary>
        public DateTime? birthday { get; set; }

        /// <summary>
        /// 当前系统Id.
        /// </summary>
        public string systemId { get; set; }

        /// <summary>
        /// app当前系统Id.
        /// </summary>
        public string appSystemId { get; set; }

        /// <summary>
        /// 默认签名.
        /// </summary>
        public DateTime? changePasswordDate { get; set; }

        /// <summary>
        /// 租户编码.
        /// </summary>
        public string tenantId { get; set; }

    }
}