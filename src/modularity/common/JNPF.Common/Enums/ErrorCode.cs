using JNPF.FriendlyException;

namespace JNPF.Common.Enums;

/// <summary>
/// 系统错误码.
/// </summary>
[ErrorCodeType]
public enum ErrorCode
{
    #region 公用

    /// <summary>
    /// 新增数据失败.
    /// </summary>
    [ErrorCodeItemMetadata("新增数据失败")]
    COM1000,

    /// <summary>
    /// 修改数据失败.
    /// </summary>
    [ErrorCodeItemMetadata("修改数据失败")]
    COM1001,

    /// <summary>
    /// 删除数据失败.
    /// </summary>
    [ErrorCodeItemMetadata("删除数据失败")]
    COM1002,

    /// <summary>
    /// 修改状态失败.
    /// </summary>
    [ErrorCodeItemMetadata("修改状态失败")]
    COM1003,

    /// <summary>
    /// 已存在同名或同编码数据.
    /// </summary>
    [ErrorCodeItemMetadata("已存在同名或同编码数据")]
    COM1004,

    /// <summary>
    /// 检测数据不存在.
    /// </summary>
    [ErrorCodeItemMetadata("检测数据不存在")]
    COM1005,

    /// <summary>
    /// 文件上传失败.
    /// </summary>
    [ErrorCodeItemMetadata("文件上传失败")]
    COM1006,

    /// <summary>
    /// 文件不存在.
    /// </summary>
    [ErrorCodeItemMetadata("文件不存在")]
    COM1007,

    /// <summary>
    /// 已到达该模板复制上限，请复制源模板.
    /// </summary>
    [ErrorCodeItemMetadata("已到达该模板复制上限，请复制源模板")]
    COM1009,

    /// <summary>
    /// .
    /// </summary>
    [ErrorCodeItemMetadata("{0}")]
    COM1010,

    #endregion

    #region system

    /// <summary>
    /// 导入文件格式错误.
    /// </summary>
    [ErrorCodeItemMetadata("文件格式不正确")]
    D3006,

    /// <summary>
    /// 预览失败,单元格配置出现死循环.
    /// </summary>
    [ErrorCodeItemMetadata("预览失败,单元格配置出现死循环")]
    D3007,

    /// <summary>
    /// 最后一条数据不能删除.
    /// </summary>
    [ErrorCodeItemMetadata("最后一条数据不能删除")]
    SYS1043,

    /// <summary>
    /// 启用版本不能删除.
    /// </summary>
    [ErrorCodeItemMetadata("启用版本不能删除")]
    SYS1044,

    /// <summary>
    /// 归档版本不能删除.
    /// </summary>
    [ErrorCodeItemMetadata("归档版本不能删除")]
    SYS1045,

    /// <summary>
    /// 数据集不能重名.
    /// </summary>
    [ErrorCodeItemMetadata("数据集不能重名")]
    SYS1046,

    /// <summary>
    /// SQL语句仅支持查询语句.
    /// </summary>
    [ErrorCodeItemMetadata("SQL语句仅支持查询语句")]
    SYS1047,

    /// <summary>
    /// SQL语句需带上@formId条件.
    /// </summary>
    [ErrorCodeItemMetadata("SQL语句需带上@formId条件")]
    SYS1048,

    /// <summary>
    /// 正在进行同步,请稍等.
    /// </summary>
    [ErrorCodeItemMetadata("正在进行同步,请稍等")]
    SYS1049,

    /// <summary>
    /// 只能输入字母、数字、点、横线和下划线，且以字母开头.
    /// </summary>
    [ErrorCodeItemMetadata("只能输入字母、数字、点、横线和下划线，且以字母开头")]
    SYS1050,

    /// <summary>
    /// 翻译标记不能重复.
    /// </summary>
    [ErrorCodeItemMetadata("翻译标记不能重复")]
    SYS1051,

    /// <summary>
    /// 翻译语言至少填写一项.
    /// </summary>
    [ErrorCodeItemMetadata("翻译语言至少填写一项")]
    SYS1052,

    /// <summary>
    /// 名称不能重复.
    /// </summary>
    [ErrorCodeItemMetadata("名称不能重复")]
    SYS10001,

    /// <summary>
    /// 编码不能重复.
    /// </summary>
    [ErrorCodeItemMetadata("编码不能重复")]
    SYS10002,

    /// <summary>
    /// 模板名已存在.
    /// </summary>
    [ErrorCodeItemMetadata("模板名已存在")]
    SYS10003,

    /// <summary>
    /// 文件夹名称不能重复.
    /// </summary>
    [ErrorCodeItemMetadata("文件夹名称不能重复")]
    SYS10004,

    /// <summary>
    /// 模板名称超过了限制长度.
    /// </summary>
    [ErrorCodeItemMetadata("模板名称超过了限制长度")]
    SYS10005,

    /// <summary>
    /// 名称重复，请重新输入.
    /// </summary>
    [ErrorCodeItemMetadata("名称重复，请重新输入")]
    SYS10006,

    /// <summary>
    /// 编码重复，请重新输入.
    /// </summary>
    [ErrorCodeItemMetadata("编码重复，请重新输入")]
    SYS10007,

    /// <summary>
    /// 不能重复.
    /// </summary>
    [ErrorCodeItemMetadata("不能重复")]
    SYS10008,

    #endregion

    #region 报表

    /// <summary>
    /// 该报表已删除.
    /// </summary>
    [ErrorCodeItemMetadata("该报表已删除")]
    R10006,

    /// <summary>
    /// 预览失败，请先保存再预览数据.
    /// </summary>
    [ErrorCodeItemMetadata("预览失败，请先保存再预览数据")]
    R10007,

    /// <summary>
    /// 获取失败，数据不存在.
    /// </summary>
    [ErrorCodeItemMetadata("获取失败，数据不存在")]
    R10008,

    /// <summary>
    /// 保存失败.
    /// </summary>
    [ErrorCodeItemMetadata("保存失败")]
    R10009,


    #endregion
}