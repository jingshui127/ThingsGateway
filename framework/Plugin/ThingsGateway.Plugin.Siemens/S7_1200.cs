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

namespace ThingsGateway.Plugin.Siemens
{
    /// <inheritdoc/>
    public class S7_1200 : Siemens
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Type DriverDebugUIType => typeof(S7_1200DebugPage);
        /// <inheritdoc/>
        public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

        /// <inheritdoc/>
        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                FoundataionConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                    ;
                client = new TcpClient();
                ((TcpClient)client).Setup(FoundataionConfig);
            }
            //载入配置
            _plc = new((TcpClient)client, SiemensEnum.S1200)
            {
                DataFormat = driverPropertys.DataFormat,
                ConnectTimeOut = driverPropertys.ConnectTimeOut,
                TimeOut = driverPropertys.TimeOut
            };
            if (driverPropertys.LocalTSAP != 0)
            {
                _plc.LocalTSAP = driverPropertys.LocalTSAP;
            }
            if (driverPropertys.Rack != 0)
            {
                _plc.Rack = driverPropertys.Rack;
            }
            if (driverPropertys.Slot != 0)
            {
                _plc.Slot = driverPropertys.Slot;
            }
        }


    }
}
