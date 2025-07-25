﻿using JNPF.Common.Extension;
using JNPF.DependencyInjection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace JNPF.Common.Helper
{
    /// <summary>
    /// Excel导入操作类
    /// 版 本：V3.2.0
    /// 版 权：引迈信息技术有限公司（https://www.jnpfsoft.com）
    /// 作 者：JNPF开发平台组
    /// 日 期：2017.03.12.
    /// </summary>
    [SuppressSniffer]
    public class ExcelImportHelper
    {
        /// <summary>
        /// 从Excel中获取数据到DataTable.
        /// </summary>
        /// <param name="filePath">Excel文件全路径(服务器路径).</param>
        /// <param name="SheetIndex">要获取数据的工作表序号(从0开始).</param>
        /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始).</param>
        /// <returns></returns>
        public static DataTable ToDataTable(string filePath, int SheetIndex = 0, int HeaderRowIndex = 0)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = null;

                // 2003
                if (filePath.IndexOf(".xlsx") == -1)
                    workbook = new HSSFWorkbook(file);
                else
                    workbook = new XSSFWorkbook(file);
                string SheetName = workbook.GetSheetName(SheetIndex);
                return ToDataTable(workbook, SheetName, HeaderRowIndex);
            }
        }

        /// <summary>
        /// 从Excel中获取数据到DataTable.
        /// </summary>
        /// <param name="filePath">Excel文件全路径(服务器路径).</param>
        /// <param name="sr">文件流.</param>
        /// <param name="SheetIndex">要获取数据的工作表序号(从0开始).</param>
        /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始).</param>
        /// <param name="HeaderRowCount">表头行数.</param>
        /// <returns></returns>
        public static DataTable ToDataTable(string filePath, Stream sr, int SheetIndex = 0, int HeaderRowIndex = 0, int HeaderRowCount = 1)
        {
            IWorkbook workbook = null;
            if (filePath.IndexOf(".xlsx") == -1)//2003
            {
                workbook = new HSSFWorkbook(sr);
            }
            else
            {
                workbook = new XSSFWorkbook(sr);
            }
            string SheetName = workbook.GetSheetName(SheetIndex);
            return ToDataTable(workbook, SheetName, HeaderRowIndex, HeaderRowCount);
        }

        /// <summary>
        /// 从Excel中获取数据到DataTable.
        /// </summary>
        /// <param name="workbook">要处理的工作薄.</param>
        /// <param name="SheetName">要获取数据的工作表名称.</param>
        /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始).</param>
        /// <param name="HeaderRowCount">表头行数.</param>
        /// <returns></returns>
        public static DataTable ToDataTable(IWorkbook workbook, string SheetName, int HeaderRowIndex, int HeaderRowCount = 1)
        {
            ISheet sheet = workbook.GetSheet(SheetName);
            DataTable table = new DataTable();
            try
            {
                IRow headerRow = sheet.GetRow(HeaderRowIndex);
                int cellCount = headerRow.LastCellNum;

                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                {
                    if (headerRow.GetCell(i) != null && headerRow.GetCell(i).StringCellValue != null)
                    {
                        DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                        var cell = headerRow.GetCell(i);
                        if (cell.IsNotEmptyOrNull())
                        {
                            column.ColumnName = cell.CellComment?.String.String;
                            column.Caption = cell.StringCellValue;
                            if (column.ColumnName.IsNullOrEmpty()) column.ColumnName = column.Caption;
                        }
                        table.Columns.Add(column);
                    }
                }

                if (HeaderRowCount == 2)
                {
                    table.Columns.Clear();
                    var headerRow1 = sheet.GetRow(HeaderRowIndex);
                    var headerRow2 = sheet.GetRow(HeaderRowIndex + 1);
                    if (headerRow2.LastCellNum > cellCount) cellCount = headerRow2.LastCellNum;

                    for (int i = headerRow1.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow2.GetCell(i) != null && headerRow2.GetCell(i).StringCellValue.IsNotEmptyOrNull())
                        {
                            DataColumn column = new DataColumn(headerRow2.GetCell(i).StringCellValue);
                            var cell = headerRow2.GetCell(i);
                            if (cell.IsNotEmptyOrNull())
                            {
                                column.ColumnName = cell.CellComment?.String.String;
                                column.Caption = cell.StringCellValue;
                                if (column.ColumnName.IsNullOrEmpty()) column.ColumnName = column.Caption;
                            }
                            table.Columns.Add(column);
                        }
                        else
                        {
                            if (headerRow1.GetCell(i) != null && headerRow1.GetCell(i).StringCellValue != null)
                            {
                                DataColumn column = new DataColumn(headerRow1.GetCell(i).StringCellValue);
                                var cell = headerRow1.GetCell(i);
                                if (cell.IsNotEmptyOrNull())
                                {
                                    column.ColumnName = cell.CellComment?.String.String;
                                    column.Caption = cell.StringCellValue;
                                    if (column.ColumnName.IsNullOrEmpty()) column.ColumnName = column.Caption;
                                }
                                table.Columns.Add(column);
                            }

                        }
                    }
                }

                int rowCount = sheet.LastRowNum;
                if (rowCount > 0)
                {
                    #region 循环各行各列,写入数据到DataTable
                    for (int i = (sheet.FirstRowNum); i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        DataRow dataRow = table.NewRow();
                        if (row != null)
                        {
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                ICell cell = row.GetCell(j);
                                if (cell == null)
                                {
                                    dataRow[j] = null;
                                }
                                else
                                {
                                    //if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                    //{
                                    //    dataRow[j] = cell.DateCellValue.ToString("yyyy/MM/dd").Trim();
                                    //}
                                    //else
                                    //{
                                    dataRow[j] = cell.ToString().Trim();
                                    //}
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < cellCount; j++)
                            {
                                dataRow[j] = string.Empty;
                            }
                        }
                        table.Rows.Add(dataRow);
                    }
                    #endregion
                }
            }
            catch (System.Exception ex)
            {
                table.Clear();
                table.Columns.Clear();
                table.Columns.Add("出错了");
                DataRow dr = table.NewRow();
                dr[0] = ex.Message;
                table.Rows.Add(dr);
                return table;
            }
            finally
            {
                // sheet.Dispose();
                workbook = null;
                sheet = null;
            }
            #region 清除最后的空行
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                bool isnull = true;
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if (table.Rows[i][j] != null)
                    {
                        if (table.Rows[i][j].ToString() != "")
                        {
                            isnull = false;
                            break;
                        }
                    }
                }
                if (isnull)
                {
                    //table.Rows[i].Delete();
                }
            }
            #endregion

            return table;
        }
    }
}