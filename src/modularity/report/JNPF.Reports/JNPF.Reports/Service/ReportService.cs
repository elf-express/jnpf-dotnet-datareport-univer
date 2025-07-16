using JNPF.Common.Configuration;
using JNPF.Common.Const;
using JNPF.Common.Core.Manager.Files;
using JNPF.Common.Entitys;
using JNPF.Common.Enums;
using JNPF.Common.Extension;
using JNPF.Common.Filter;
using JNPF.Common.Security;
using JNPF.DataEncryption;
using JNPF.DependencyInjection;
using JNPF.DynamicApiController;
using JNPF.FriendlyException;
using JNPF.Reports.Entitys.Entity;
using JNPF.Reports.Core.Model.Data;
using JNPF.Reports.Core.Model.Report;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Text;
using JNPF.Reports.Core.UReport.Model.Data;
using JNPF.Common.Core;

namespace JNPF.Reports.Service;

/// <summary>
///  报表设计.
/// </summary>
[ApiDescriptionSettings(Tag = "Report", Name = "Report", Order = 173)]
[Route("api/[controller]")]
public class ReportService : IDynamicApiController, ITransient
{
    private string ApiConst = App.Configuration["Message:ApiDoMain"];

    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ReportEntity> _repository;

    private readonly IFileManager _fileManager;

    /// <summary>
    /// 初始化一个新实例.
    /// </summary>
    public ReportService(
        ISqlSugarRepository<ReportEntity> repository,
        IFileManager fileManager)
    {
        _repository = repository;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ReportPagination input)
    {
        var data = await _repository.AsQueryable().Where(a => a.DeleteMark == null)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .WhereIF(input.state != null, a => a.EnabledMark.Equals(input.state))
            .WhereIF(input.enabledMark != null, a => a.EnabledMark.Equals(input.enabledMark))
            .WhereIF(input.category != null, a => a.Category.Equals(input.category))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderByIF(!input.keyword.IsNullOrEmpty(), a => a.LastModifyTime, OrderByType.Desc)
            .Select(a => new ReportListVO
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().EnableTableFilter().Where(u => u.Id == a.CreatorUserId && u.DeleteMark == null && u.EnabledMark == 1).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                lastModifyUser = SqlFunc.Subqueryable<UserEntity>().EnableTableFilter().Where(u => u.Id == a.LastModifyUserId && u.DeleteMark == null && u.EnabledMark == 1).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                category = SqlFunc.Subqueryable<DictionaryDataEntity>().EnableTableFilter().Where(d => d.Id == a.Category && d.DeleteMark == null && d.EnabledMark == 1).Select(d => d.FullName),
                sortCode = a.SortCode,
                enabledMark = a.EnabledMark,
                state = a.EnabledMark,
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ReportListVO>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var entity = await _repository.GetSingleAsync(p => p.Id == id);
        var res = entity.Adapt<ReportInfoVO>();
        return res;
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<dynamic> Create([FromBody] ReportCrForm input)
    {
        return await Creates(input);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ReportUpForm input)
    {
        var entity = input.Adapt<ReportEntity>();
        entity.Id = id;

        // 名称长度验证
        if (entity.FullName.Length > 80) throw Oops.Oh(ErrorCode.SYS10005);

        // 重名验证
        if (await _repository.IsAnyAsync(x => x.FullName.Equals(entity.FullName) && x.DeleteMark == null && !x.Id.Equals(id))) throw Oops.Oh(ErrorCode.SYS10003);

        // 编码验证
        if (await _repository.IsAnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null && !x.Id.Equals(id))) throw Oops.Oh(ErrorCode.SYS10002);

        var isOk = await _repository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetSingleAsync(p => p.Id == id && p.DeleteMark == null);
        var vEntity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId == id).ToListAsync();
        await _repository.AsSugarClient().Updateable(vEntity).CallEntityMethod(it => it.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        var isOk = await _repository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
        if (!(isOk > 0))
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #region 版本增删改

    /// <summary>
    /// 版本详情.
    /// </summary>
    /// <param name="versionId">版本id.</param>
    /// <returns></returns>
    [HttpGet("Info/{versionId}")]
    public async Task<dynamic> VersionInfo(string versionId)
    {
        return await GetVersionInfo(versionId);
    }

    /// <summary>
    /// 版本新增.
    /// </summary>
    /// <param name="versionId"></param>
    /// <returns></returns>
    [HttpPost("Info/{versionId}")]
    public async Task<dynamic> CopyVersion(string versionId)
    {
        var entity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(p => p.Id == versionId);

        entity.Version = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(entity.TemplateId)).MaxAsync(x => x.Version);
        entity.Id = SnowflakeIdHelper.NextId();
        entity.Version++;
        entity.State = 0;
        entity.SortCode = 0L;

        DataSetPagination pagination = new DataSetPagination();
        pagination.objectId = versionId;
        pagination.objectType = CommonConst.DataSetTypeEnum_REPORT_VER;

        var json = await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_LIST, "get", pagination.ToObject<Dictionary<string, object>>());
        var result = json.ToObject<List<DataSetInfo>>();
        List<DataSetInfo> dataSetList = new List<DataSetInfo>();
        if (result != null && result.Any())
        {
            dataSetList.AddRange(result);
        }

        if (dataSetList.Count > 0)
        {
            foreach (DataSetInfo item in dataSetList)
            {
                item.id = null;
                item.objectType = CommonConst.DataSetTypeEnum_REPORT_VER;
                item.objectId = entity.Id;
            }
            var dataForm = new Dictionary<string, object>();
            dataForm["objectId"] = entity.Id;
            dataForm["objectType"] = CommonConst.DataSetTypeEnum_REPORT_VER;
            dataForm["list"] = dataSetList;

            await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_SAVE, "post", dataForm);
        }

        await _repository.AsSugarClient().Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        return entity.Id;
    }

    /// <summary>
    /// 版本删除.
    /// </summary>
    /// <param name="versionId">主键.</param>
    /// <returns></returns>
    [HttpDelete("Info/{versionId}")]
    public async Task DeleteVersion(string versionId)
    {
        var entity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(p => p.Id == versionId && p.DeleteMark == null);

        if (entity != null)
        {
            if ((await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(entity.TemplateId)).CountAsync()) == 1)
                throw Oops.Oh(ErrorCode.SYS1043);
            if (entity.State.Equals(1))
                throw Oops.Oh(ErrorCode.SYS1044);
            if (entity.State.Equals(2))
                throw Oops.Oh(ErrorCode.SYS1045);

            var isOk = await _repository.AsSugarClient().Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            if (!(isOk > 0))
                throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 版本列表.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Version/{id}")]
    public async Task<dynamic> GetList(string id)
    {
        var data = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(a => a.DeleteMark == null)
            .Where(x => x.TemplateId.Equals(id))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select(a => new ReportVersionListVO
            {
                id = a.Id,
                fullName = SqlFunc.MergeString("报表版本V", SqlFunc.ToString(+a.Version)),
                state = a.State,
                version = SqlFunc.ToString(a.Version)
            }).ToListAsync();
        if (data == null || !data.Any()) throw Oops.Oh(ErrorCode.R10006);
        var firstItem = data.FirstOrDefault(x => x.state.Equals(1));
        if (firstItem != null)
        {
            data.Remove(firstItem);
            data.Insert(0, firstItem);
        }

        return data;
    }

    /// <summary>
    /// 保存或者发布.
    /// </summary>
    [HttpPost("Save")]
    [NonUnify]
    public async Task<dynamic> SaveOrRelease([FromBody] ReportUpForm form)
    {
        var versionEntity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(x => x.Id.Equals(form.versionId));
        var versionNew = form.ToObject<ReportVersionEntity>();
        versionNew.Id = versionEntity.Id;
        versionNew.State = versionEntity.State;
        ReportEntity entity = await _repository.AsQueryable().FirstAsync(x => x.Id.Equals(form.id));

        // 发布流程
        if (form.type.Equals(1))
        {
            // 改流程版本
            if (form.versionId.IsNotEmptyOrNull())
            {
                var isRelease = versionNew.State.Equals(2);
                ReportVersionEntity info = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(versionEntity.TemplateId) && x.State.Equals(1)).FirstAsync();

                if (info != null)
                {
                    // 变更归档状态，排序码
                    info.SortCode = 0L;
                    info.State = 2;

                    await _repository.AsSugarClient().Updateable(info).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                }
                versionNew.State = 1;
                versionNew.SortCode = 1L;
                entity.EnabledMark = 1;
                if (isRelease)
                {
                    await _repository.AsSugarClient().Updateable(versionNew).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                }
            }
        }

        var isOk = await _repository.AsSugarClient().Updateable(versionNew).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

        // 数据集创建
        var versionId = versionNew.Id;
        List<DataSetInfo> dataSetList = form.dataSetList != null ? form.dataSetList : new List<DataSetInfo>();
        var dataForm = new Dictionary<string, object>();
        dataForm["objectId"] = versionId;
        dataForm["objectType"] = CommonConst.DataSetTypeEnum_REPORT_VER;
        dataForm["list"] = dataSetList;

        await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_SAVE, "post", dataForm);

        entity.AllowExport = form.allowExport;
        entity.AllowPrint = form.allowPrint;
        await _repository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
        if (form.type.Equals(1))
        {
            return new { code = 200, msg = "发布成功" };
        }

        return new { code = 200, msg = "保存成功" };
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task Copy(string id)
    {
        ReportEntity entity = await _repository.AsQueryable().FirstAsync(x => x.Id.Equals(id));
        var random = new Random().NextLetterAndNumberString(5);
        var fullName = entity.FullName + ".副本" + random;
        if (fullName.Length > 50)
        {
            throw Oops.Oh(ErrorCode.COM1009);
        }

        ReportInfoVO info = new ReportInfoVO();
        var vEntity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(id) && x.State == 1).FirstAsync();
        if (vEntity != null)
        {
            List<DataSetInfo> listVO = new List<DataSetInfo>();
            info = await GetVersionInfo(vEntity.Id);
            var dataSetList = info.dataSetList != null ? info.dataSetList : new List<DataSetInfo>();
            foreach (var item in dataSetList)
            {
                item.objectId = null;
                item.id = null;
                listVO.Add(item);
            }
        }

        ReportCrForm form = info.Adapt<ReportCrForm>();
        form.fullName = fullName;
        form.enCode = entity.EnCode + random;
        form.category = entity.Category;
        form.sortCode = entity.SortCode.ParseToLong();
        form.description = entity.Description;
        form.id = null;

        await Creates(form);
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    [HttpGet("{id}/Actions/Export")]
    public async Task<dynamic> Export(string id)
    {
        string _filePath = _fileManager.GetPathByType(FileVariable.TemporaryFilePath);
        var entity = await _repository.AsQueryable().FirstAsync(x => x.Id.Equals(id));
        var list = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(id)).ToListAsync();
        if (!list.Any()) throw Oops.Oh(ErrorCode.COM1005);

        var info = await GetVersionInfo(list.FirstOrDefault().Id);
        var json = info.ToJsonString();
        var tableName = CommonConst.ModuleTypeEnum_REPORT_TEMPLATE;
        var fileName = entity.FullName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + tableName;

        var url = "/api/Report/data/Download?name=" + fileName + "&encryption=";
        var byteList = new UTF8Encoding(true).GetBytes(json.ToCharArray());
        var stream = new MemoryStream(byteList);
        await _fileManager.UploadFileByType(stream, _filePath, fileName);
        return new { name= fileName, url= url + DESCEncryption.Encrypt(fileName + "#" + FileVariable.TemporaryFilePath, "JNPF") };
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpPost("Actions/Import")]
    public async Task ActionsImport(IFormFile file, int type)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.rp.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        ReportInfoVO infVo;

        try
        {
            infVo = josn.ToObject<ReportInfoVO>();
        }
        catch
        {
            throw Oops.Oh(ErrorCode.D3006);
        }

        ReportEntity entity = infVo.ToObject<ReportEntity>();
        entity.EnabledMark = 0;
        var stringJoiner = new List<string>();

        // id为空切名称不存在时
        if (await _repository.IsAnyAsync(x => x.Id.Equals(entity.Id)))
        {
            if (type.Equals(0)) stringJoiner.Add("ID");
            else entity.Id = SnowflakeIdHelper.NextId();
        }

        if (await _repository.IsAnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null)) stringJoiner.Add("编码");
        if (await _repository.IsAnyAsync(x => x.FullName.Equals(entity.FullName) && x.DeleteMark == null)) stringJoiner.Add("名称");

        if (stringJoiner.Count > 0 && type.Equals(1))
        {
            var copyNum = new Random().NextLetterAndNumberString(5);
            entity.FullName = entity.FullName + ".副本" + copyNum;
            entity.EnCode = entity.EnCode + copyNum;
        }
        else if (type.Equals(0) && stringJoiner.Count > 0)
        {
            stringJoiner.Add("重复");
            throw Oops.Oh(ErrorCode.COM1010, string.Join("、", stringJoiner));
        }

        await _repository.AsSugarClient().Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

        // 版本添加
        ReportVersionEntity versionEntity = infVo.Adapt<ReportVersionEntity>();
        var versionId = SnowflakeIdHelper.NextId();
        versionEntity.Id = versionId;
        versionEntity.TemplateId = entity.Id;
        versionEntity.Version = 1;
        versionEntity.State = 0;
        versionEntity.SortCode = 0l;
        await _repository.AsSugarClient().Insertable(versionEntity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

        // 数据集创建
        List<DataSetInfo> dataSetList = infVo.dataSetList != null ? infVo.dataSetList : new List<DataSetInfo>();
        if (dataSetList.Count > 0)
        {
            foreach (var item in dataSetList)
            {
                item.id = null;
                item.objectId = null;
            }
            var dataForm = new Dictionary<string, object>();
            dataForm["objectId"] = versionId;
            dataForm["objectType"] = CommonConst.DataSetTypeEnum_REPORT_VER;
            dataForm["list"] = dataSetList;
            await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_SAVE, "post", dataForm);
        }
    }

    /// <summary>
    /// 下拉列表.
    /// </summary>
    [HttpGet("Selector")]
    public async Task<dynamic> SelectorList()
    {
        var repList = await _repository.AsQueryable().Where(x => x.EnabledMark.Equals(1) && x.DeleteMark == null).OrderBy(x => x.SortCode).OrderByDescending(x => x.CreatorTime).ToListAsync();

        List<DictionaryDataEntity> dictionList = await _repository.AsSugarClient().Queryable<DictionaryDataEntity>().Where(x => repList.Select(xx => xx.Category).Contains(x.Id)).ToListAsync();

        Dictionary<string, List<ReportEntity>> map = new Dictionary<string, List<ReportEntity>>();
        foreach (var it in repList.Select(x => x.Category).Distinct().ToList())
        {
            map.Add(it, repList.Where(x => x.Category.Equals(it)).ToList());
        }

        List<ReportSelectVO> listVO = new List<ReportSelectVO>();
        foreach (var entity in dictionList)
        {
            List<ReportEntity> entityList = map[entity.Id] != null ? map[entity.Id] : new List<ReportEntity>();
            if (entityList.Count > 0)
            {
                ReportSelectVO vo = new ReportSelectVO();
                vo.id = entity.Id;
                vo.fullName = entity.FullName;
                vo.hasChildren = true;
                vo.children = entityList.ToObject<List<ReportSelectVO>>();
                listVO.Add(vo);
            }
        }

        return new { list = listVO };
    }

    /// <summary>
    /// 报表发布菜单.
    /// </summary>
    /// <param name="id">id.</param>
    /// <param name="model">model.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Module")]
    public async Task Module(string id, [FromBody] MenuModel model)
    {
        ReportEntity entity = await _repository.AsSugarClient().Queryable<ReportEntity>().FirstAsync(p => p.Id == id);
        if (entity == null) throw Oops.Oh(ErrorCode.R10008);
        model.id = id;
        model.fullName = entity.FullName;
        model.encode = entity.EnCode;
        model.type = 10;
        model.app = 0;
        entity.platformRelease = model.platformRelease;
        var json = await ReportUtil.http(ApiConst + CommonConst.ApiConst_SAVE_MENU, "post", model);
        if (json.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.R10009);

        await _repository.AsSugarClient().Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取报表发布菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/getReleaseMenu")]
    public async Task<dynamic> GetReleaseMenu(string id)
    {
        ReportEntity entity = await _repository.AsSugarClient().Queryable<ReportEntity>().FirstAsync(p => p.Id == id);
        if (entity == null) throw Oops.Oh(ErrorCode.R10008);
        MenuModel model = new MenuModel();
        model.id = id;
        var json = await ReportUtil.http(ApiConst + CommonConst.ApiConst_GET_MENU, "get", model.ToObject<Dictionary<string, object>>());
        ModuleNameVO moduleNameVO = new ModuleNameVO();
        if (json.IsNotEmptyOrNull()) moduleNameVO = json.ToObject<ModuleNameVO>();
        ReportInfoVO vo = entity.Adapt<ReportInfoVO>();
        vo.appIsRelease = 0;
        vo.pcIsRelease = 0;
        if (moduleNameVO != null)
        {
            if (moduleNameVO.pcNames.IsNotEmptyOrNull())
            {
                vo.pcIsRelease = 1;
                vo.pcReleaseName = moduleNameVO.pcNames;
            }

            if (moduleNameVO.appNames.IsNotEmptyOrNull())
            {
                vo.appIsRelease = 1;
                vo.appReleaseName = moduleNameVO.appNames;
            }
        }

        return vo;
    }

    #endregion

    #region 私有方法
    public async Task<ReportInfoVO> GetVersionInfo(string versionId)
    {
        var versionEntity = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().FirstAsync(p => p.Id == versionId);
        var entity = await _repository.AsSugarClient().Queryable<ReportEntity>().FirstAsync(p => p.Id == versionEntity.TemplateId);
        var vo = versionEntity.Adapt<ReportInfoVO>();
        vo.versionId = versionId;
        vo.id = entity.Id;
        vo.fullName = entity.FullName;
        vo.allowExport = entity.AllowExport;
        vo.allowPrint = entity.AllowPrint;
        vo.category= entity.Category;
        vo.enCode = entity.EnCode;
        vo.sortCode = entity.SortCode.ParseToLong();

        DataSetPagination pagination = new DataSetPagination();
        pagination.objectId = versionId;
        pagination.objectType = CommonConst.DataSetTypeEnum_REPORT_VER;

        var json = await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_LIST, "get", pagination.ToObject<Dictionary<string, object>>());
        try
        {
            var result = json.ToObject<List<DataSetInfo>>();
            var dataSetList = new List<DataSetInfo>();
            if (result != null)
            {
                dataSetList = result;
            }

            vo.dataSetList = dataSetList;
        }
        catch
        {
            var errorInfo = json.ToObject<Dictionary<string, object>>();
            if (errorInfo.ContainsKey("msg"))
            {
                throw Oops.Oh(ErrorCode.COM1010, errorInfo["msg"].ToString().Split("]").Last());
            }
        }

        return vo;
    }

    public async Task<dynamic> Creates(ReportCrForm form)
    {
        ReportEntity entity = form.ToObject<ReportEntity>();

        // 名称长度验证
        if (entity.FullName.Length > 80) throw Oops.Oh(ErrorCode.SYS10005);

        // 重名验证
        if (await _repository.IsAnyAsync(x => x.FullName.Equals(entity.FullName) && x.DeleteMark == null)) throw Oops.Oh(ErrorCode.SYS10003);

        // 编码验证
        if (await _repository.IsAnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null)) throw Oops.Oh(ErrorCode.SYS10002);

        var id = entity.Id.IsNotEmptyOrNull() ? entity.Id : SnowflakeIdHelper.NextId();
        entity.Id = id;
        entity.EnabledMark = 0;
        entity.AllowExport = 1; // 默认开启
        entity.AllowPrint = 1; // 默认开启
        form.id = id;

        // 添加
        await _repository.AsSugarClient().Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

        if (!await _repository.AsSugarClient().Queryable<ReportVersionEntity>().AnyAsync(x => x.TemplateId.Equals(id)))
        {
            ReportVersionEntity vEntity = form.Adapt<ReportVersionEntity>();
            var versionId = SnowflakeIdHelper.NextId();
            vEntity.Id = versionId;
            vEntity.TemplateId = form.id;
            var maxVersion = await _repository.AsSugarClient().Queryable<ReportVersionEntity>().Where(x => x.TemplateId.Equals(id)).MaxAsync(x => x.Version);
            vEntity.Version = maxVersion != null ? maxVersion + 1 : 1;
            vEntity.State = 0;
            vEntity.SortCode = 0L;
            await _repository.AsSugarClient().Insertable(vEntity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
            var dataSetList = form.dataSetList != null ? form.dataSetList : new List<DataSetInfo>();
            if (dataSetList.Count > 0)
            {
                // 数据集创建
                var dataForm = new Dictionary<string, object>();
                dataForm["objectId"] = versionId;
                dataForm["objectType"] = CommonConst.DataSetTypeEnum_REPORT_VER;
                dataForm["list"] = dataSetList;

                await ReportUtil.http(ApiConst + CommonConst.ApiConst_DATASET_SAVE, "post", dataForm);
            }
        }

        return id;
    }

    #endregion
}
