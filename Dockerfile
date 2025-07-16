# 基础镜像
FROM hub.yinmaisoft.com/jnpf-devops-agent/base-dotnet-sdk:8.0 AS build
LABEL maintainer=jnpf-team

# 设置时区
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# 指定临时工作目录
WORKDIR /temp

# 复制项目
COPY --link . .

# Publish framework
RUN dotnet publish framework/JNPF/JNPF.csproj -f net8.0 -c Release

# Publish JNPF.API
RUN dotnet publish src/application/JNPF.API.Report/JNPF.API.Report.csproj -f net8.0 -c Release

# Copy regworkerid_lib_v1.3.1
COPY src/application/JNPF.API.Report/lib/ /temp/src/application/JNPF.API.Report/bin/Release/net8.0/lib/

# clean publish
RUN rm -rf /temp/src/application/JNPF.API.Report/bin/Release/net8.0/publish

# Runtime stage
FROM hub.yinmaisoft.com/jnpf-devops-agent/base-dotnet-aspnet:8.0 as production

# 指定运行时的工作目录
WORKDIR /data/jnpfsoft/dotnetUniverApi

# 将构建文件拷贝到运行时目录中
COPY --link --from=build /temp/src/application/JNPF.API.Report/bin/Release/net8.0/ ${WORKDIR}

# Delete Configurations
#RUN rm -rf src/application/JNPF.API.Report/bin/Release/net8.0/Configurations/
#RUN rm -rf src/application/JNPF.API.Report/bin/Release/net8.0/appsettings.Development.json
#RUN rm -rf src/application/JNPF.API.Report/bin/Release/net8.0/appsettings.json

# 指定容器内运行端口
EXPOSE 32000

# 指定容器启动时要运行的命令
ENTRYPOINT ["dotnet", "JNPF.API.Report.dll","--urls=http://*:32000"]