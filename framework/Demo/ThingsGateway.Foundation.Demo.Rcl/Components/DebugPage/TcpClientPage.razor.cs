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

namespace ThingsGateway.Foundation.Demo;

/// <inheritdoc/>
public partial class TcpClientPage : IDisposable
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private TouchSocketConfig config;
    /// <summary>
    /// IP
    /// </summary>
    private string IP = "127.0.0.1";
    /// <summary>
    /// Port
    /// </summary>
    public int Port { get; set; } = 502;

    private TcpClient TcpClient { get; set; } = new();

    private async Task ConnectAsync()
    {
        try
        {
            TcpClient.Close();
            await GetTcpClient().ConnectAsync();
        }
        catch (Exception ex)
        {

            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }

    }
    private void DisConnect()
    {
        try
        {
            TcpClient.Close();
        }
        catch (Exception ex)
        {

            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public TcpClient GetTcpClient()
    {
        config ??= new TouchSocketConfig();
        var LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetRemoteIPHost(new IPHost(IP + ":" + Port));
        //载入配置
        TcpClient.Setup(config);
        return TcpClient;
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        config ??= new TouchSocketConfig();

        base.OnInitialized();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            var LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
            LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
            config.SetRemoteIPHost(new IPHost(IP + ":" + Port));
            TcpClient.Setup(config);
        }
        base.OnAfterRender(firstRender);
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        TcpClient.SafeDispose();
    }

    internal void StateHasChangedAsync()
    {
        StateHasChanged();
    }
}