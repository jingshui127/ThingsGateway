#region copyright
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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 终端接口
    /// </summary>
    public interface IClient : IDependencyObject, IDisposable, ILoggerObject
    {
        /// <summary>
        /// 获取一个同步数据接收器
        /// </summary>
        /// <returns></returns>
        IReceiver CreateReceiver();

        /// <summary>
        /// 移除同步数据接收器
        /// </summary>
        void ClearReceiver();

        /// <summary>
        /// 终端协议
        /// </summary>
        Protocol Protocol { get; set; }

        /// <summary>
        /// 简单IOC容器
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// 最后一次接收到数据的时间
        /// </summary>
        DateTime LastReceivedTime { get; }

        /// <summary>
        /// 最后一次发送数据的时间
        /// </summary>
        DateTime LastSendTime { get; }
    }
}