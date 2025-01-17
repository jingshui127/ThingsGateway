﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion;
using Furion.Logging.Extensions;

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

using UAParser;
namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 实时数据库后台服务
/// </summary>
public class HistoryValueWorker : BackgroundService
{
    private readonly ILogger<HistoryValueWorker> _logger;
    private GlobalDeviceData _globalDeviceData;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="HistoryValueWorker"/>
    public HistoryValueWorker(ILogger<HistoryValueWorker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");

    private ConcurrentQueue<HistoryValue> ChangeDeviceVariables { get; set; } = new();
    private ConcurrentQueue<HistoryValue> DeviceVariables { get; set; } = new();
    /// <summary>
    /// 获取数据库链接
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<SqlSugarClient>> GetHisDbAsync()
    {
        var ConfigService = App.GetService<IConfigService>();
        var hisEnable = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_HisConfig_Base, ThingsGatewayConfigConst.Config_His_Enable))?.ConfigValue?.ToBoolean();
        var hisDbType = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_HisConfig_Base, ThingsGatewayConfigConst.Config_His_DbType))?.ConfigValue;
        var hisConnstr = (await ConfigService.GetByConfigKeyAsync(ThingsGatewayConfigConst.ThingGateway_HisConfig_Base, ThingsGatewayConfigConst.Config_His_ConnStr))?.ConfigValue;

        if (!(hisEnable == true))
        {
            return new OperResult<SqlSugarClient>("历史数据已配置为Disable");
        }

        var configureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 修改列可空-1、带?问号 2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                    column.IsNullable = true;
            },
        };

        DbType type = DbType.QuestDB;
        if (!string.IsNullOrEmpty(hisDbType))
        {
            if (Enum.TryParse<DbType>(hisDbType, ignoreCase: true, out var result))
            {
                type = result;
            }
            else
            {
                return new OperResult<SqlSugarClient>("数据库类型转换失败");
            }
        }

        var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = hisConnstr,//连接字符串
            DbType = type,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        });
        AopSetting(sqlSugarClient);
        return OperResult.CreateSuccessResult(sqlSugarClient);
    }


    #region db

    /// <summary>
    /// Aop设置
    /// </summary>
    /// <param name="db"></param>
    private static void AopSetting(SqlSugarClient db)
    {
        var config = db.CurrentConnectionConfig;

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;

        // 打印SQL语句
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            //如果不是开发环境就打印sql
            if (App.HostEnvironment.IsDevelopment())
            {
                if (sql.StartsWith("SELECT"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (sql.StartsWith("UPDATE"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                if (sql.StartsWith("INSERT"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                if (sql.StartsWith("DELETE"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                WriteSqlLog(UtilMethods.GetSqlString(config.DbType, sql, pars));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
        };
        //异常
        db.Aop.OnError = (ex) =>
        {
            //如果不是开发环境就打印日志
            if (App.WebHostEnvironment.IsDevelopment())
            {
                if (ex.Parametres == null) return;
                Console.ForegroundColor = ConsoleColor.Red;
                var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                WriteSqlLogError(UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres));
                Console.ForegroundColor = ConsoleColor.White;
            }
        };

    }
    private static void WriteSqlLog(string msg)
    {
        Console.WriteLine("【Sql执行时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }
    private static void WriteSqlLogError(string msg)
    {
        Console.WriteLine("【Sql执行错误时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
        Console.WriteLine("【Sql语句】：" + msg + Environment.NewLine);
    }

    #endregion


    #region worker服务
    private EasyLock easyLock = new();


    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        _logger?.LogInformation("历史服务启动");
        await easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { easyLock.Release(); easyLock = null; });
        await base.StartAsync(cancellationToken);
    }
    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("历史服务停止");
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await easyLock?.WaitAsync();

        _globalDeviceData = App.GetService<GlobalDeviceData>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
        }
    }


    #endregion

    #region core
    /// <summary>
    /// 全部重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();

    private Task HistoryValueTask;

    private bool IsExited;

    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    private ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    /// <summary>
    /// 离线缓存
    /// </summary>
    protected CacheDb CacheDb { get; set; }
    /// <summary>
    /// 重启
    /// </summary>
    /// <returns></returns>
    public async Task RestartAsync()
    {
        await StopAsync();
        await StartAsync();
    }

    internal async Task StartAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            foreach (var device in _globalDeviceData.CollectDevices)
            {
                device.DeviceVariableRunTimes?.Where(a => a.HisEnable == true)?.ForEach(v => { v.VariableCollectChange += DeviceVariableCollectChange; });
                device.DeviceVariableRunTimes?.Where(a => a.HisEnable == true)?.ForEach(v => { v.VariableValueChange += DeviceVariableValueChange; });
            }
            StoppingTokens.Add(new());
            await InitAsync();
            if (HistoryValueTask.Status == TaskStatus.Created)
                HistoryValueTask.Start();
            IsExited = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启错误");
        }
        finally
        {
            restartLock.Release();
        }
    }

    internal async Task StopAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            IsExited = true;

            foreach (var device in _globalDeviceData.CollectDevices)
            {
                device.DeviceVariableRunTimes?.Where(a => a.HisEnable == true)?.ForEach(v => { v.VariableCollectChange -= DeviceVariableCollectChange; });
                device.DeviceVariableRunTimes?.Where(a => a.HisEnable == true)?.ForEach(v => { v.VariableValueChange -= DeviceVariableValueChange; });
            }

            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.Cancel();
            }

            if (HistoryValueTask != null)
            {
                try
                {
                    _logger?.LogInformation($"历史数据线程停止中");
                    await HistoryValueTask.WaitAsync(TimeSpan.FromSeconds(10));
                    _logger?.LogInformation($"历史数据线程已停止");
                }
                catch (ObjectDisposedException)
                {

                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning($"历史数据线程停止超时，已强制取消");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "等待历史数据线程停止错误");
                }
            }

            HistoryValueTask?.SafeDispose();
            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.SafeDispose();
            }
            StoppingTokens.Clear();

            ChangeDeviceVariables.Clear();
            DeviceVariables.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启错误");
        }
        finally
        {
            restartLock.Release();
        }
    }

    private void DeviceVariableCollectChange(DeviceVariableRunTime variable)
    {
        if (variable.HisType == HisType.Collect && !IsExited)
        {
            DeviceVariables.Enqueue(variable.Adapt<HistoryValue>());
        }
    }

    private void DeviceVariableValueChange(DeviceVariableRunTime variable)
    {
        if (variable.HisType == HisType.Change && !IsExited)
        {
            ChangeDeviceVariables.Enqueue(variable.Adapt<HistoryValue>());
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync()
    {
        CacheDb = new("HistoryValueCache");
        var stoppingToken = StoppingTokens.Last().Token;
        HistoryValueTask = await Task.Factory.StartNew(async () =>
        {
            _logger?.LogInformation($"历史数据线程开始");
            await Task.Yield();//返回线程控制，不再阻塞
            try
            {
                var result = await GetHisDbAsync();
                if (!result.IsSuccess)
                {
                    _logger?.LogWarning($"历史数据线程即将退出：{result.Message}");
                    StatuString = new OperResult($"已退出：{result.Message}");
                    IsExited = true;
                    return;
                }
                else
                {
                    var sqlSugarClient = result.Content;
                    bool LastIsSuccess = true;
                    /***创建/更新单个表***/
                    try
                    {
                        await sqlSugarClient.Queryable<HistoryValue>().FirstAsync(stoppingToken);
                        LastIsSuccess = true;
                        StatuString = OperResult.CreateSuccessResult();
                    }
                    catch (Exception)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            IsExited = true;
                            return;
                        }
                        try
                        {
                            _logger.LogWarning("连接历史数据表失败，尝试初始化表");
                            sqlSugarClient.DbMaintenance.CreateDatabase();
                            sqlSugarClient.CodeFirst.InitTables(typeof(HistoryValue));
                            LastIsSuccess = true;
                            StatuString = OperResult.CreateSuccessResult();
                        }
                        catch (Exception ex)
                        {
                            LastIsSuccess = false;
                            StatuString = new OperResult(ex);
                            _logger.LogWarning(ex, "连接历史数据库失败");
                        }
                    }
                    IsExited = false;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(500, stoppingToken);

                            if (stoppingToken.IsCancellationRequested)
                                break;

                            var collectList = DeviceVariables.ToListWithDequeue();
                            var changeList = ChangeDeviceVariables.ToListWithDequeue();

                            if (collectList.Count != 0)
                            {
                                ////Sql保存
                                var collecthis = collectList;
                                int count = 0;
                                //插入
                                try
                                {
                                    count = await sqlSugarClient.Insertable(collecthis).ExecuteCommandAsync(stoppingToken);
                                    LastIsSuccess = true;
                                }
                                catch (Exception ex)
                                {
                                    if (LastIsSuccess)
                                        _logger.LogWarning(ex, "写入历史数据失败");
                                    var cacheDatas = collecthis.ChunkTrivialBetter(500);
                                    foreach (var a in cacheDatas)
                                    {
                                        await CacheDb.AddCacheData("", a.ToJsonString(), 5000);
                                    }
                                    LastIsSuccess = false;
                                }
                            }

                            if (stoppingToken.IsCancellationRequested)
                                break;
                            if (changeList.Count != 0)
                            {
                                ////Sql保存
                                var changehis = changeList;
                                int count = 0;
                                //插入
                                try
                                {
                                    count = await sqlSugarClient.Insertable(changehis).ExecuteCommandAsync(stoppingToken);
                                    LastIsSuccess = true;
                                }
                                catch (Exception ex)
                                {
                                    if (LastIsSuccess)
                                        _logger.LogWarning(ex, "写入历史数据失败");
                                    var cacheDatas = changehis.ChunkTrivialBetter(500);
                                    foreach (var a in cacheDatas)
                                    {
                                        await CacheDb.AddCacheData("", a.ToJsonString(), 5000);
                                    }
                                    LastIsSuccess = false;
                                }
                            }

                            if (LastIsSuccess)
                            {
                                //缓存值
                                var cacheData = await CacheDb.GetCacheData();
                                var data = cacheData.SelectMany(a => a.CacheStr.FromJsonString<List<HistoryValue>>()).ToList();
                                try
                                {
                                    var count = await sqlSugarClient.Insertable(data).ExecuteCommandAsync(stoppingToken);
                                    await CacheDb.DeleteCacheData(cacheData.Select(a => a.Id).ToArray());
                                }
                                catch (Exception ex)
                                {
                                    if (LastIsSuccess)
                                        _logger.LogWarning(ex, "写入历史数据失败");
                                }
                            }

                            if (stoppingToken.IsCancellationRequested)
                                break;
                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        catch (Exception ex)
                        {
                            if (LastIsSuccess)
                                _logger?.LogWarning(ex, $"历史数据循环异常");
                            StatuString = new OperResult(ex);
                            LastIsSuccess = false;
                        }
                    }

                }
            }
            catch (TaskCanceledException)
            {

                IsExited = true;
            }
            catch (ObjectDisposedException)
            {
                IsExited = true;
            }
            catch (Exception ex)
            {
                IsExited = true;
                _logger?.LogError(ex, $"历史数据循环异常");
            }
            IsExited = true;
        }
 , TaskCreationOptions.LongRunning);
    }
    #endregion
}
/// <summary>
/// <see cref="HistoryValue"/> Master规则
/// </summary>
public class HistoryValueMapper : IRegister
{
    /// <inheritdoc/>
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<DeviceVariableRunTime, HistoryValue>()
            .Map(dest => dest.Value, (src) => ValueReturn(src))
            .Map(dest => dest.CollectTime, (src) => src.CollectTime.ToUniversalTime())//注意sqlsugar插入时无时区，直接utc时间
            .Map(dest => dest.CreateTime, (src) => DateTime.UtcNow);//注意sqlsugar插入时无时区，直接utc时间
    }

    private static object ValueReturn(DeviceVariableRunTime src)
    {
        if (src.Value?.ToString()?.IsBoolValue() == true)
        {
            if (src.Value.ToBoolean())
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return src.Value;
        }
    }
}

