using JNPF.Common.Configuration;
using JNPF.Common.Const;
using JNPF.Common.Core.Manager.Files;
using JNPF.Common.Enums;
using JNPF.Common.Extension;
using JNPF.Common.Filter;
using JNPF.Common.Manager;
using JNPF.Common.Security;
using JNPF.DataEncryption;
using JNPF.DependencyInjection;
using JNPF.DynamicApiController;
using JNPF.FriendlyException;
using JNPF.Logging.Attributes;
using JNPF.RemoteRequest.Extensions;
using JNPF.Reports.Core.Enum;
using JNPF.Reports.Core.Model.Data;
using JNPF.Reports.Core.Model.Report;
using JNPF.Reports.Core.Univer.Chart;
using JNPF.Reports.Core.Univer.Model;
using JNPF.Reports.Core.UReport.Univer.Data.Custom;
using JNPF.Reports.Core.Util;
using JNPF.Reports.Entitys.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using JNPF.Common.Core;

namespace JNPF.Reports.Service;

/// <summary>
///  报表设计.
/// </summary>
[ApiDescriptionSettings(Tag = "Report", Name = "Data", Order = 173)]
[Route("api/Report/[controller]")]
public class ReportDataService : IDynamicApiController, ITransient
{
    private string ApiConst = App.Configuration["Message:ApiDoMain"];

    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ReportVersionEntity> _repository;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个新实例.
    /// </summary>
    public ReportDataService(
        ISqlSugarRepository<ReportVersionEntity> repository,
        ICacheManager cacheManager,
        IFileManager fileManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 预览.
    /// </summary>
    /// <param name="id">打印模板id.</param>
    /// <param name="params"></param>
    /// <returns></returns>
    [HttpPost("{id}/preview")]
    public async Task<UniverPreview> Preview(string id, [FromBody] Dictionary<string, object> param)
    {
        var pageInput = param.ToObject<PageInputBase>();
        var entity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(x => x.Id.Equals(id) && x.DeleteMark == null);
        if (entity == null) throw Oops.Oh(ErrorCode.R10006);
        if (entity.Snapshot.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.R10007);

        // 获取当前
        var univerWorkBook = entity.Snapshot.ToObject<UniverWorkBook>();
        var sheetOrder = univerWorkBook.sheetOrder;

        var report = await _repository.AsSugarClient().Queryable<ReportEntity>().FirstAsync(x => x.Id.Equals(entity.TemplateId));
        var query = new DataSetQuery();
        query.moduleId = entity.TemplateId;
        query.id = id;
        query.type = CommonConst.DataSetTypeEnum_REPORT_VER;
        query.map = param;
        query.mapStr = param.ToJsonStringOld();
        query.convertConfig = entity.ConvertConfig;
        var json = await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_DATA, "post", query);
        var sheetData = new Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>>();

        // 全部数据
        if (json.IsNotEmptyOrNull())
        {
            var dataList = json.ToObject<Dictionary<string, List<Dictionary<string, object>>>>();
            foreach (var sId in sheetOrder)
            {
                sheetData[sId] = dataList;
            }
        }

        // 当前数据
        var sheetId = (param.ContainsKey("sheetId") && param["sheetId"].IsNotEmptyOrNull()) ? param["sheetId"].ToString() : !sheetOrder.IsEmpty() ? sheetOrder.First() : null;
        if (sheetId.IsNotEmptyOrNull())
        {
            List<DataQuery> dataQueryList = entity.QueryList.IsNotEmptyOrNull() ? entity.QueryList.ToObject<List<DataQuery>>() : new List<DataQuery>();
            List<object> queryList = new List<object>();
            foreach (var dataQuery in dataQueryList)
            {
                if (Equals(sheetId, dataQuery.sheet))
                {
                    queryList.AddRange(dataQuery.queryList);
                }
            }

            if (queryList.Count() > 0)
            {
                query.queryList = queryList.ToJsonString();
                var sheetJson = await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_DATA, "post", query);
                if (sheetJson.IsNotEmptyOrNull())
                {
                    var dataList = sheetJson.ToObject<Dictionary<string, List<Dictionary<string, object>>>>();
                    sheetData[sheetId] = dataList;
                }
            }
        }

        try
        {
            UniverConvert convert = new UniverConvert();
            UniverPreview vo = convert.transform(entity.Snapshot, entity.Cells, entity.SortList, sheetData);
            vo.versionId = id;
            vo.queryList = entity.QueryList;
            vo.fullName = report.FullName;
            vo.allowExport = report.AllowExport;
            vo.allowPrint = report.AllowPrint;
            return vo;
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.COM1010, ex.Message);
        }
    }

    /// <summary>
    /// 预览.
    /// </summary>
    /// <param name="id">大id.</param>
    /// <param name="params"></param>
    /// <returns></returns>
    [HttpPost("{id}/previewTemplate")]
    public async Task<UniverPreview> PreviewTemplate(string id, [FromBody] Dictionary<string, object> param)
    {
        var entity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(x => x.TemplateId.Equals(id) && x.State.Equals(1) && x.DeleteMark == null);
        if (entity == null)
        {
            throw Oops.Oh(ErrorCode.R10006);
        }
        else
        {
            return await Preview(entity.Id, param);
        }
    }

    /// <summary>
    /// 上传图片.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("upload/file")]
    public async Task<dynamic> Upload(IFormFile file)
    {
        UploaderVO vo = new UploaderVO();
        try
        {
            string _filePath = _fileManager.GetPathByType(FileVariable.TemporaryFilePath);
            string _fileName = SnowflakeIdHelper.NextId() + ".jpeg";
            var stream = file.OpenReadStream();
            var url = "/api/Report/data/Download?name=" + _fileName + "&encryption=";
            await _fileManager.UploadFileByType(stream, _filePath, _fileName);
            vo.name = _fileName;
            vo.url = url + DESCEncryption.Encrypt(_fileName + "#" + FileVariable.TemporaryFilePath, "JNPF");
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1010, e.Message);
        }

        return vo;
    }

    /// <summary>
    /// 远端接口下载图片.
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpPost("downImg")]
    public async Task<dynamic> upload([FromBody] ReportPagination pagination)
    {
        UploaderVO vo = new UploaderVO();
        try
        {
            Stream bytes = null;
            var imgValue = pagination.imgValue;
            var imgType = pagination.imgType;
            if (imgValue.IsNotEmptyOrNull())
            {
                if (Equals(ImageEnum.BASE64.ToString(), imgType))
                {
                    var regex = "data:image/\\w+;base64,";
                    var base64Img = imgValue;
                    if (Regex.IsMatch(imgValue, regex))
                    {
                        base64Img = imgValue.Split(";base64,").Last();
                    }
                    bytes = new MemoryStream(Convert.FromBase64String(base64Img));
                }
                else
                {
                    bytes = (await imgValue.GetAsStreamAsync()).Stream;
                }
            }
            if (bytes != null && bytes.Length > 0)
            {
                string _filePath = _fileManager.GetPathByType(FileVariable.TemporaryFilePath);
                string _fileName = SnowflakeIdHelper.NextId() + ".jpeg";
                var url = "/api/Report/data/Download?name=" + _fileName + "&encryption=";
                await _fileManager.UploadFileByType(bytes, _filePath, _fileName);
                vo.name = _fileName;
                vo.url = url + DESCEncryption.Encrypt(_fileName + "#" + FileVariable.TemporaryFilePath, "JNPF");
            }
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1010, e.Message);
        }
        return vo;
    }

    /// <summary>
    /// 上传excel.
    /// </summary>
    /// <param name="multipartFile"></param>
    /// <returns></returns>
    [HttpPost("ImportExcel")]
    public dynamic ImportExcel(IFormFile file)
    {
        if (file.FileName.Split(".").Last() != "xlsx" && file.FileName.Split(".").Last() != "xls") throw Oops.Oh(ErrorCode.COM1010, "不支持该文件格式.");
        var cellData = new UniverCustom();
        UniverWorkBook univerWorkBook = UniverExcel.formFile(file);
        UniverPreview vo = new UniverPreview()
        {
            snapshot = univerWorkBook.ToJsonString(),
            cells = cellData.ToJsonString()
        };
        return vo;
    }

    /// <summary>
    /// 下载excel.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="@params"></param>
    [HttpPost("{id}/DownExcel")]
    public async Task<dynamic> downExcel(string id, ReportPagination pagination)
    {
        UploaderVO vo = new UploaderVO();
        if (pagination.snapshot.IsNotEmptyOrNull())
        {
            pagination.id = id;
            try
            {
                List<UniverChartModel> chartList = new List<UniverChartModel>();
                var sheetList = new List<string>() { pagination.sheetId };
                var outputStream = UniverExcel.downExcel(pagination.snapshot, new List<UniverChartModel>(), sheetList, FileVariable.TemporaryFilePath);
                var fullName = pagination.fullName + ".xlsx";
                fullName = fullName.Replace("/", "");
                var url = "/api/Report/data/Download?name=" + fullName + "&encryption=";
                string? addPath = Path.Combine(FileVariable.TemporaryFilePath, fullName);
                await _fileManager.UploadFileByType(outputStream, FileVariable.TemporaryFilePath, fullName);
                _cacheManager.Set(fullName, string.Empty);
                vo.name = fullName;
                vo.url = url + DESCEncryption.Encrypt(fullName + "#tempFilePath", "JNPF");
            }
            catch (Exception e)
            {
                throw Oops.Oh(ErrorCode.COM1010, string.Format("报表导出excel异常：{0}", e.Message));
            }
        }
        return vo;
    }

    [NonAction]
    public async Task<dynamic> Down(string id, [FromBody] Dictionary<string, object> param)
    {
        ReportPagination pagination = param.ToObject<ReportPagination>();
        var versionEntity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(x => x.Id.Equals(id));
        if (versionEntity == null) return new { name = string.Empty, url = string.Empty };
        ReportEntity entity = await _repository.AsSugarClient().Queryable<ReportEntity>().FirstAsync(x => x.Id.Equals(versionEntity.TemplateId));
        UniverPreview preview = await Preview(id, param);
        try
        {
            var sheetList = new List<string>() { pagination.sheetId };
            var outputStream = UniverExcel.downExcel(preview.snapshot, new List<UniverChartModel>(), sheetList, FileVariable.TemporaryFilePath);

            var fullName = entity.FullName + ".xlsx";
            fullName = fullName.Replace("/", "");
            var url = "/api/Report/data/Download?name=" + fullName + "&encryption=";
            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, fullName);
            await _fileManager.UploadFileByType(outputStream, FileVariable.TemporaryFilePath, fullName);
            _cacheManager.Set(fullName, string.Empty);
            return new { name = fullName, url = url + DESCEncryption.Encrypt(fullName + "#tempFilePath", "JNPF") };
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1010, string.Format("报表导出excel异常：{0}", e.Message));
        }
    }

    /// <summary>
    /// 下载文件.
    /// </summary>
    /// <param name="encryption"></param>
    /// <param name="name"></param>
    [HttpGet("Download")]
    [AllowAnonymous]
    [IgnoreLog]
    public async Task downExcel(string encryption, string name)
    {
        var fileNameAll = DESCEncryption.Decrypt(encryption, "JNPF").Replace("\n", "");

        if (fileNameAll.IsNotEmptyOrNull())
        {
            var data = fileNameAll.Split("#");
            var fileUrl = data.Length > 0 ? data.First() : "";
            var type = data.Length > 1 ? data.Last() : "";

            if (fileUrl.Contains(".jpeg"))
            {
                await FileDown(fileUrl, type);
            }
            else
            {
                await FileDown(fileUrl, type, name);
            }
        }
    }

    private async Task FileDown(string fileUrl, string fileName)
    {
        string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileUrl);
        var fileStreamResult = await _fileManager.DownloadFileByType(filePath, fileUrl);
        byte[] bytes = new byte[fileStreamResult.FileStream.Length];
        fileStreamResult.FileStream.Read(bytes, 0, bytes.Length);
        fileStreamResult.FileStream.Close();
        var httpContext = App.HttpContext;
        httpContext.Response.ContentType = "image/jpeg";
        await httpContext.Response.Body.WriteAsync(bytes);
        await httpContext.Response.Body.FlushAsync();
        httpContext.Response.Body.Close();
    }

    private async Task FileDown(string fileUrl, string type, string fileName)
    {
        string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileUrl);
        if (type.IsNotEmptyOrNull())
        {
            filePath = Path.Combine(_fileManager.GetPathByType(type), fileUrl);
        }
        var fileStreamResult = await _fileManager.DownloadFileByType(filePath, fileUrl);
        byte[] bytes = new byte[fileStreamResult.FileStream.Length];

        fileStreamResult.FileStream.Read(bytes, 0, bytes.Length);

        fileStreamResult.FileStream.Close();
        var httpContext = App.HttpContext;
        httpContext.Response.ContentType = "application/octet-stream";
        httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
        httpContext.Response.Headers.Add("Content-Length", bytes.Length.ToString());
        await httpContext.Response.Body.WriteAsync(bytes);
        await httpContext.Response.Body.FlushAsync();
        httpContext.Response.Body.Close();
    }
}
