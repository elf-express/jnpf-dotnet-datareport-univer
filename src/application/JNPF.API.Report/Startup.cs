using JNPF.API.Entry.Handlers;
using JNPF.Common.Cache;
using JNPF.UnifyResult;
using JNPF.VirtualFileServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SqlSugar;

namespace JNPF.API.Entry;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConsoleFormatter();

        // SqlSugar
        services.SqlSugarConfigure();

        // Jwt处理程序
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: true, jwtBearerConfigure: options =>
        {
            // 实现 JWT 身份验证过程控制
            options.Events = new JwtBearerEvents
            {
                // 添加读取 Token 的方式
                OnMessageReceived = context =>
                {
                    var httpContext = context.HttpContext;

                    // 判断请求是否包含 token 参数，如果有就设置给 Token
                    if (httpContext.Request.Query.ContainsKey("token"))
                    {
                        var token = httpContext.Request.Query["token"].ToString();

                        switch (token.StartsWith("Bearer") || token.StartsWith("bearer"))
                        {
                            case true:
                                token = token.Replace("Bearer", string.Empty).Replace("bearer", string.Empty);
                                break;
                        }

                        // 去除头部空格
                        token = token.TrimStart();
                        switch (token.StartsWith("%20"))
                        {
                            case true:
                                token = token.Replace("%20", string.Empty);
                                break;
                        }

                        // 设置 Token
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                },

                // Token 验证通过处理
                OnTokenValidated = context => Task.CompletedTask,
            };
        });

        // 跨域
        services.AddCorsAccessor();

        // 注册远程请求
        services.AddRemoteRequest();

        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<ConnectionStringsOptions>();
        services.AddConfigurableOptions<TenantOptions>();

        services.AddControllers()
        .AddInjectWithUnifyResult<RESTfulResultProvider>()
        .AddAppLocalization() // 注册多语言
        .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = 64;

                options.SerializerSettings.Converters.AddDateTimeTypeConverters();

                options.SerializerSettings.Converters.AddClayConverters();

                // 格式化JSON文本
                options.SerializerSettings.Formatting = Formatting.Indented;

                // 默认命名规则
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();

                // 设置时区为 Utc
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                // 格式化json输出的日期格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                // 忽略空值
                //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                // 忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        services.AddUnifyJsonOptions("special", new JsonSerializerSettings
        {
            MaxDepth = 64,

            // 默认命名规则
            ContractResolver = new DefaultContractResolver(),

            // 设置时区为 Utc
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,

            // 格式化json输出的日期格式
            DateFormatString = "yyyy-MM-dd HH:mm:ss",

            // 忽略循环引用
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        services.AddUnifyJsonOptions("datainterfaceSpecial", new JsonSerializerSettings
        {
            MaxDepth = 64,

            // 默认命名规则
            ContractResolver = new DefaultContractResolver(),

            // 设置时区为 Utc
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,

            // 格式化json输出的日期格式
            DateFormatString = "yyyy-MM-dd HH:mm:ss",

            // 忽略空值
            NullValueHandling = NullValueHandling.Ignore,

            // 忽略循环引用
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddSession();

        // 使用本地缓存必须添加
        // services.AddMemoryCache();

        // 日志监听
        // services.AddMonitorLogging(options =>
        //{
        //    options.IgnorePropertyNames = new[] { "Byte" };
        //    options.IgnorePropertyTypes = new[] { typeof(byte[]) };
        //});

        // 日志写入文件-消息、警告、错误
        Array.ForEach(new[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error }, logLevel =>
        {
            services.AddFileLogging(options =>
            {
                options.WithTraceId = true; // 显示线程Id
                options.WithStackFrame = true; // 显示程序集
                options.FileNameRule = fileName => string.Format(fileName, DateTime.Now, logLevel.ToString()); // 每天创建一个文件
                options.WriteFilter = logMsg => logMsg.LogLevel == logLevel; // 日志级别
                options.HandleWriteError = (writeError) => // 写入失败时启用备用文件
                {
                    writeError.UseRollbackFileName(Path.GetFileNameWithoutExtension(writeError.CurrentFileName) + "-oops" + Path.GetExtension(writeError.CurrentFileName));
                };
            });
        });

        services.AddHttpContextAccessor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        // 添加状态码拦截中间件
        app.UseUnifyResultStatusCodes();

        // app.UseHttpsRedirection(); // 强制https
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = FS.GetFileExtensionContentTypeProvider()
        });

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseInject(string.Empty);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}