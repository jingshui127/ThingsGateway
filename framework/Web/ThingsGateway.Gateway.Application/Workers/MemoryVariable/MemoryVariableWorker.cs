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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 实时数据库后台服务
/// </summary>
public class MemoryVariableWorker : BackgroundService
{
    private readonly ILogger<MemoryVariableWorker> _logger;
    private GlobalDeviceData _globalDeviceData;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="MemoryVariableWorker"/>
    public MemoryVariableWorker(ILogger<MemoryVariableWorker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");

    #region worker服务

    private EasyLock easyLock = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("中间变量服务启动");
        await easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { easyLock.Release(); easyLock = null; });
        await base.StartAsync(cancellationToken);
    }
    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("中间变量服务停止");
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await easyLock.WaitAsync();
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

    private Task MemoryWorkerTask;

    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    private ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    /// <summary>
    /// 初始化
    /// </summary>
    public async Task InitAsync()
    {
        var stoppingToken = StoppingTokens.Last().Token;
        MemoryWorkerTask = await Task.Factory.StartNew(async () =>
        {
            _logger?.LogInformation($"中间变量计算线程开始");
            try
            {
                var variableService = App.GetService<IVariableService>();
                var data = await variableService.GetMemoryVariableRuntimeAsync();
                _globalDeviceData.MemoryVariables = new(data);
                StatuString = OperResult.CreateSuccessResult();
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(500, stoppingToken);

                        if (stoppingToken.IsCancellationRequested)
                            break;
                        var isSuccess = true;
                        foreach (var item in _globalDeviceData.MemoryVariables)
                        {
                            if (!string.IsNullOrEmpty(item.ReadExpressions) && item.ProtectTypeEnum != ProtectTypeEnum.WriteOnly)
                            {
                                //变量内部已经做了表达式转换，直接赋值
                                var operResult = item.SetValue(item.Value);
                                if (!operResult.IsSuccess)
                                {
                                    if (StatuString.IsSuccess)
                                        _logger?.LogWarning(operResult.Message);
                                    isSuccess = false;
                                    StatuString = operResult;
                                }
                            }
                            else
                            {

                            }

                        }
                        if (isSuccess)
                            StatuString = OperResult.CreateSuccessResult();
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, $"历史数据循环异常");
                        StatuString = new OperResult(ex);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"中间变量计算线程循环异常");
            }
        }
 , TaskCreationOptions.LongRunning);
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

            StoppingTokens.Add(new());
            //初始化线程
            await InitAsync();
            if (MemoryWorkerTask.Status == TaskStatus.Created)
                MemoryWorkerTask.Start();
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
            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.Cancel();
            }
            if (MemoryWorkerTask != null)
            {

                try
                {
                    _logger?.LogInformation($"中间变量计算线程停止中");
                    await MemoryWorkerTask.WaitAsync(TimeSpan.FromSeconds(10));
                    _logger?.LogInformation($"中间变量计算线程已停止");
                }
                catch (ObjectDisposedException)
                {

                }
                catch (TimeoutException)
                {
                    _logger?.LogInformation($"中间变量计算线程停止超时，已强制取消");
                }
                catch (Exception ex)
                {
                    _logger?.LogInformation(ex, "等待线程停止错误");
                }
            }

            MemoryWorkerTask?.SafeDispose();
            foreach (var cancellationToken in StoppingTokens)
            {
                cancellationToken.SafeDispose();
            }
            MemoryWorkerTask = null;
            StoppingTokens.Clear();
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



    #endregion
}


