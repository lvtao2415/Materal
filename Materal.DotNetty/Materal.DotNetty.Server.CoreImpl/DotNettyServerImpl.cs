﻿using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Materal.DotNetty.Server.Core;
using Materal.StringHelper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Materal.DotNetty.Server.CoreImpl
{
    public class DotNettyServerImpl : IDotNettyServer
    {
        protected readonly IServiceProvider Service;
        protected ServerConfig ServerConfig;

        public DotNettyServerImpl(IServiceProvider service)
        {
            Service = service;
        }
        public event Action<IServerChannelHandler> OnConfigHandler;
        public event Action<string> OnMessage;
        public event Action<string, string> OnSubMessage;
        public event Action<Exception> OnException;
        public event Func<string> OnGetCommand;
        private IChannel _bootstrapChannel;
        public virtual async Task RunAsync(ServerConfig serverConfig)
        {
            ServerConfig = serverConfig;
            OnSubMessage?.Invoke("服务启动中......", "重要");
            //第一步：创建ServerBootstrap实例
            var bootstrap = new ServerBootstrap();
            //第二步：绑定事件组
            IEventLoopGroup mainGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workGroup = new MultithreadEventLoopGroup();
            bootstrap.Group(mainGroup, workGroup);
            //第三步：绑定服务端的通道
            bootstrap.Channel<TcpServerSocketChannel>();
            //第四步：配置处理器
            bootstrap.Option(ChannelOption.SoBacklog, 8192);
            bootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                try
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new HttpServerCodec());
                    pipeline.AddLast(new HttpObjectAggregator(655300000));
                    var channelHandler = Service.GetService<ServerChannelHandler>();
                    if (channelHandler == null) return;
                    if (OnException != null)
                    {
                        channelHandler.OnException += OnException;
                    }
                    channelHandler.OnMessage += OnMessage;
                    OnConfigHandler?.Invoke(channelHandler);
                    pipeline.AddLast(channelHandler);
                }
                catch (Exception exception)
                {
                    OnException?.Invoke(exception);
                }
            }));
            //第五步：配置主机和端口号
            IPAddress ipAddress = GetTrueIPAddress();
            _bootstrapChannel = await bootstrap.BindAsync(ipAddress, ServerConfig.Port);
            OnSubMessage?.Invoke("服务启动成功", "重要");
        }
        /// <summary>
        /// 等待服务停止
        /// </summary>
        public virtual async Task StopAsync()
        {
            OnSubMessage?.Invoke("正在停止服务......", "重要");
            await _bootstrapChannel.CloseAsync();
            OnSubMessage?.Invoke("服务已停止", "重要");
        }
        #region 私有方法
        /// <summary>
        /// 获得真实IP地址
        /// </summary>
        /// <returns></returns>
        protected virtual IPAddress GetTrueIPAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            ipAddresses = ipAddresses.Where(m => m.ToString().IsIPv4()).ToArray();
            bool trueAddress = ipAddresses.Any(m => ServerConfig.Host.Equals(m.ToString()));
            IPAddress result = trueAddress ? IPAddress.Parse(ServerConfig.Host) : ipAddresses[0];
            return result;
        }
        #endregion
    }
}
