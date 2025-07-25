﻿using JNPF.DependencyInjection;
using Color = System.Drawing.Color;

namespace JNPF.Common.Models.NPOI;

/// <summary>
/// Excel导出列名
/// 版 本：V3.0.0
/// 版 权：引迈信息技术有限公司（https://www.jnpfsoft.com）
/// 作 者：JNPF开发平台组
/// 日 期：2017.03.09.
/// </summary>
[SuppressSniffer]
public class ExcelColumnModel
{
    /// <summary>
    /// 列名.
    /// </summary>
    public string? Column { get; set; }

    /// <summary>
    /// Excel列名.
    /// </summary>
    public string? ExcelColumn { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    public int Width { get; set; } = 15;

    /// <summary>
    /// 前景色.
    /// </summary>
    public Color ForeColor { get; set; }

    /// <summary>
    /// 背景色.
    /// </summary>
    public Color Background { get; set; }

    /// <summary>
    /// 字体.
    /// </summary>
    public string? Font { get; set; }

    /// <summary>
    /// 字号.
    /// </summary>
    public short Point { get; set; }

    /// <summary>
    /// 对齐方式
    /// left 左
    /// center 中间
    /// right 右
    /// fill 填充
    /// justify 两端对齐
    /// centerselection 跨行居中
    /// distributed.
    /// </summary>
    public string? Alignment { get; set; }

    /// <summary>
    /// 是否必填.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 是否列冻结.
    /// </summary>
    public bool FreezePane { get; set; }

    /// <summary>
    /// 下拉选择值.
    /// </summary>
    public List<string> SelectList { get; set; } = new List<string>();
}