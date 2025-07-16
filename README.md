> 特别说明：源码、JDK、数据库、Redis等安装或存放路径禁止包含中文、空格、特殊字符等

# 项目说明

## 一 环境要求

### 1.1 开发环境

| 类目 | 版本说明或建议    |
| --- |----------|
| 电脑配置 | 建议开发电脑I3及以上CPU，内存16G及以上   |
| 操作系统 | Windows 10/11，MacOS      |
| SDK | `.NET 8.0 SDK`  |
| Redis | 数据缓存，推荐使用 `5.0` 及以上版本 |
| 数据库 | 兼容 `MySQL 5.7.x/8.x`、`SQLServer 2012+`、`Oracle 11g`、`PostgreSQL 12+`、`达梦数据库(DM8)`、`人大金仓数据库(KingbaseES_V8R6)` |
| IDE | `Visual Studio 2022` |
| 前端开发 | `Node.js` v16.15.0(某些情况下可能需要安装 `Python3`)及以上版本；<br/>`pnpm` v8.10及以上版本；<br/>浏览器推荐使用 `Chrome` 最新版本；<br/>`Visual Studio Code`(简称VSCode) |
| 移动端开发 | `Node.js` v16.15.0(某些情况下可能需要安装 Python3)及以上版本；<br/> `HBuilder X` (最新版)    |
| 文件存储 | 默认使用本地存储，兼容 `MinIO` 及多个云对象存储，如 `阿里云 OSS`、`华为云 OBS`、`七牛云 Kodo`、`腾讯云 COS`等  |

### 1.2 运行环境

> 适用于测试或生产环境

| 类目 | 版本说明或建议    |
| --- |----------|
| 服务器配置 | 建议至少在 4C/16G/50G 的机器配置下运行；   |
| 操作系统 | 建议使用 `Windows Server 2019` 及以上版本或主流 `Linux` 发行版，推荐使用 `Linux` 环境；兼容 `统信UOS`，`OpenEuler`，`麒麟服务器版` 等信创环境；    |
| Runtime | `.NET 8.0 SDK` 或 `.NET 8.0 Runtime` |
| Redis | 数据缓存，推荐使用 `5.0` 及以上版本 |
| 数据库 | 兼容 `MySQL 5.7.x/8.x`、`SQLServer 2012+`、`Oracle 11g`、`PostgreSQL 12+`、`达梦数据库(DM8)`、`人大金仓数据库(KingbaseES_V8R6)` |
| 文件存储 | 默认使用本地存储，兼容 `MinIO` 及多个云对象存储，如 `阿里云 OSS`、`华为云 OBS`、`七牛云 Kodo`、`腾讯云 COS` 等  |
| 前端服务器 | `Nginx` 建议使用 1.18.0 及以上版本、`OpenResty` 或 `TongHttpServer 6.0`(国产信创)   |

## 二 配套项目

| 项目 | 分支 |  说明 |
| --- | --- | --- |
| jnpf-dotnet | v5.2.x-stable | .NET单体后端项目源码 |
| jnpf-dotnet-cloud | v5.2.x-stable | .NET微服务后端项目源码 |