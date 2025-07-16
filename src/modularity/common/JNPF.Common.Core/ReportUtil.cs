using JNPF.Common.Const;
using JNPF.Common.Enums;
using JNPF.Common.Models.User;
using JNPF.Common.Security;
using JNPF.DataEncryption;
using JNPF.FriendlyException;
using JNPF.Logging;
using JNPF.RemoteRequest.Extensions;
using JNPF.UnifyResult;
using Microsoft.AspNetCore.Http;

namespace JNPF.Common.Core;

public class ReportUtil
{
    public static async Task<UserInfoModel> InitUserInfo(string token)
    {
        UserInfoModel userInfo = GetLocalLoginUser();
        if (userInfo == null)
        {
            string ApiConst = App.Configuration["Message:ApiDoMain"];
            var dicHerader = new Dictionary<string, object>();
            dicHerader.Add("jnpf_api", true);
            if (token != null && !token.Contains("::")) dicHerader.Add("Authorization", token);
            try
            {
                var dataStr = await (ApiConst + CommonConst.ApiConst_ME).SetHeaders(dicHerader).GetAsStringAsync();
                var result = dataStr.ToObjectOld<RESTfulResult<object>>();

                if (!result.code.Equals(StatusCodes.Status200OK))
                {
                    result.msg = result.msg.ToString().Split("]")[1];
                    throw Oops.Oh(ErrorCode.COM1010, result.msg);
                }

                userInfo = result.data.ToObject<UserInfoModel>();
                SetLocalLoginUser(userInfo);
            }
            catch (Exception e)
            {
                Log.Error("获取用户信息错误:" + e.Message);
            }
        }

        return userInfo;
    }

    public static async Task<string> http(string url, string method, Dictionary<string, object> param)
    {
        var token = GetToken();
        var dicHerader = new Dictionary<string, object>();
        //dicHerader.Add("jnpf_api", true);
        if (token != null && !token.Contains("::")) dicHerader.Add("Authorization", token);

        try
        {
            var res = string.Empty;
            if (method.ToUpper().Equals("GET"))
            {
                var resStr = await url.SetHeaders(dicHerader).SetQueries(param).GetAsStringAsync();
                var resDic = resStr.ToObject<Dictionary<string, object>>();
                if (resDic.ContainsKey("data") && resDic["data"] != null) res = resDic["data"].ToString();
                else if (resDic.ContainsKey("code") && resDic["code"] != null && resDic["code"].ToString() == "200") res = resDic["code"].ToString();
                else if (resDic.ContainsKey("msg") && resDic["msg"] != null) throw new Exception(resDic["msg"].ToString().Split("]").Last());
            }
            else
            {
                var resStr = await url.SetHeaders(dicHerader).SetBody(param).PostAsStringAsync();
                var resDic = resStr.ToObject<Dictionary<string, object>>();
                if (resDic.ContainsKey("data") && resDic["data"] != null) res = resDic["data"].ToString();
                else if (resDic.ContainsKey("code") && resDic["code"] != null && resDic["code"].ToString() == "200") res = resDic["code"].ToString();
                else if (resDic.ContainsKey("msg") && resDic["msg"] != null) throw new Exception(resDic["msg"].ToString().Split("]").Last());
            }

            return res;
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1010, e.Message);
        }
    }

    public static async Task<string> http(string url, string method, object param)
    {
        var token = GetToken();
        var dicHerader = new Dictionary<string, object>();
        //dicHerader.Add("jnpf_api", true);
        if (token != null && !token.Contains("::")) dicHerader.Add("Authorization", token);

        try
        {
            var res = string.Empty;
            if (method.ToUpper().Equals("GET"))
            {
                var resStr = await url.SetHeaders(dicHerader).SetQueries(param).GetAsStringAsync();
                var resDic = resStr.ToObject<Dictionary<string, object>>();
                if (resDic.ContainsKey("data") && resDic["data"] != null) res = resDic["data"].ToString();
                else if (resDic.ContainsKey("code") && resDic["code"] != null && resDic["code"].ToString() == "200") res = resDic["code"].ToString();
                else if (resDic.ContainsKey("msg") && resDic["msg"] != null) throw new Exception(resDic["msg"].ToString().Split("]").Last());
            }
            else
            {
                var resStr = await url.SetHeaders(dicHerader).SetBody(param).PostAsStringAsync();
                var resDic = resStr.ToObject<Dictionary<string, object>>();
                if (resDic.ContainsKey("data") && resDic["data"] != null) res = resDic["data"].ToString();
                else if (resDic.ContainsKey("code") && resDic["code"] != null && resDic["code"].ToString() == "200") res = resDic["code"].ToString();
                else if (resDic.ContainsKey("msg") && resDic["msg"] != null) throw new Exception(resDic["msg"].ToString().Split("]").Last());
            }

            return res;
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.COM1010, e.Message);
        }
    }

    private static UserInfoModel GetLocalLoginUser()
    {
        var _user = App.HttpContext?.User;
        if (_user != null)
        {
            return new UserInfoModel()
            {
                userId = _user.FindFirst(ClaimConst.CLAINMUSERID)?.Value,
                userAccount = _user.FindFirst(ClaimConst.CLAINMACCOUNT)?.Value,
                userName = _user.FindFirst(ClaimConst.CLAINMREALNAME)?.Value,
                tenantId = _user.FindFirst(ClaimConst.TENANTID)?.Value,
            };
        }
        else
        {
            return null;
        }
    }

    private static void SetLocalLoginUser(UserInfoModel user)
    {
        // 生成Token令牌
        string accessToken = JWTEncryption.Encrypt(
            new Dictionary<string, object>
            {
                { ClaimConst.CLAINMUSERID, user.userId },
                { ClaimConst.CLAINMACCOUNT, user.userAccount },
                { ClaimConst.CLAINMREALNAME, user.userName },
                { ClaimConst.CLAINMADMINISTRATOR, false },
                { ClaimConst.TENANTID, user.tenantId},
                { ClaimConst.OnlineTicket, string.Empty }
            }, 30);

        // 设置刷新Token令牌
        App.HttpContext.Response.Headers["x-access-token"] = JWTEncryption.GenerateRefreshToken(accessToken, 120);
    }

    private static string GetToken()
    {
        return string.IsNullOrEmpty(App.HttpContext?.Request.Headers["Authorization"]) ? App.HttpContext?.Request.Query["token"] : App.HttpContext?.Request.Headers["Authorization"];
    }
}
