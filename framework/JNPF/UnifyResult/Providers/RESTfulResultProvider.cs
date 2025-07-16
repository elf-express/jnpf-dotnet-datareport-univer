using JNPF.DataValidation;
using JNPF.FriendlyException;
using JNPF.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JNPF.UnifyResult;

/// <summary>
/// RESTful 风格返回值
/// </summary>
[SuppressSniffer, UnifyModel(typeof(RESTfulResult<>))]
public class RESTfulResultProvider : IUnifyResultProvider
{
    /// <summary>
    /// JWT 授权异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnAuthorizeException(DefaultHttpContext context, ExceptionMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors)
            , UnifyContext.GetSerializerSettings(context));
    }

    /// <summary>
    /// 异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        if (context.HttpContext.Request.Headers.Keys.Contains("Accept-Language"))
        {
            var Language = context.HttpContext.Request.Headers["Accept-Language"].ToString().Replace("_", "-");
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            if (localizationSettings.SupportedCultures.Contains(Language))
            {
                L.SetCulture(Language);
            }
        }
        var error = L.Text["系统异常"];
        if (metadata.ErrorCode == null)
        {
            metadata.Errors = error.Value;
        }
        return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors)
            , UnifyContext.GetSerializerSettings(context));
    }

    /// <summary>
    /// 成功返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public IActionResult OnSucceeded(ActionExecutedContext context, object data)
    {
        if (context.HttpContext.Request.Headers.Keys.Contains("Accept-Language"))
        {
            var Language = context.HttpContext.Request.Headers["Accept-Language"].ToString().Replace("_","-");
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            if (localizationSettings.SupportedCultures.Contains(Language))
            {
                L.SetCulture(Language);
            }
        }
        var succeeded = L.Text["操作成功"];
        var apiType = context.HttpContext.Request.Headers.Keys.Contains("jnpf_api");
        if (apiType)
            return new JsonResult(data, UnifyContext.GetSerializerSettings(context));
        else
            return new JsonResult(RESTfulResult(StatusCodes.Status200OK, true, data, errors: succeeded.Value)
                , UnifyContext.GetSerializerSettings(context));
    }

    /// <summary>
    /// 验证失败/业务异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.ValidationResult)
            , UnifyContext.GetSerializerSettings(context));
    }

    /// <summary>
    /// 特定状态码返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <param name="unifyResultSettings"></param>
    /// <returns></returns>
    public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
    {
        // 设置响应状态码
        UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);
        if (context.Request.Headers.Keys.Contains("Accept-Language"))
        {
            var Language = context.Request.Headers["Accept-Language"].ToString().Replace("_", "-");
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            if (localizationSettings.SupportedCultures.Contains(Language))
            {
                L.SetCulture(Language);
            }
        }
        var error = L.Text["登录过期,请重新登录"];
        switch (statusCode)
        {
            // 处理 401 状态码
            case StatusCodes.Status401Unauthorized:
                await context.Response.WriteAsJsonAsync(RESTfulResult(600, errors: error.Value)
                    , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            // 处理 403 状态码
            case StatusCodes.Status403Forbidden:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "403 Forbidden")
                    , App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;

            default: break;
        }
    }

    /// <summary>
    /// 返回 RESTful 风格结果集
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="succeeded"></param>
    /// <param name="data"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static RESTfulResult<object> RESTfulResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
    {
        return new RESTfulResult<object>
        {
            code = statusCode,
            data = data,
            msg = errors,
            extras = UnifyContext.Take(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}