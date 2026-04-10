using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.EventExecutor.Implementation
{
    /// <summary>
    /// Since ChannelHandlerContext is attached to the Executor, if Libuv is used, a copy of the shared Executor will be generated
    /// If you are using network components, please choose MultithreadExecutor
    /// </summary>
    public class DefaultEventExecutorProvider : IEventExecutorProvider
    {
        private readonly IEventLoopGroup _workEventExecutor;

        private readonly IEventLoopGroup _bossEventExecutor;

        private readonly IEventLoopGroup _workMultithreadExecutor;

        private readonly IEventLoopGroup _bossMultithreadExecutor;


        public DefaultEventExecutorProvider()
        {
            if (!AppConfig.ServerOptions.Libuv)
            {
                _bossEventExecutor = new MultithreadEventLoopGroup(1, TaskSchedulerType.Alone);
                _workEventExecutor = new MultithreadEventLoopGroup(AppConfig.ServerOptions.EventLoopCount, TaskSchedulerType.Alone);
                _bossMultithreadExecutor = _bossEventExecutor;
                _workMultithreadExecutor = _workEventExecutor;
            }
            else
            {
                var dispatcher = new DispatcherEventLoopGroup(TaskSchedulerType.Alone);
                _bossEventExecutor = dispatcher;
                _workEventExecutor = new WorkerEventLoopGroup(dispatcher, AppConfig.ServerOptions.EventLoopCount, TaskSchedulerType.Alone);
                _bossMultithreadExecutor = new MultithreadEventLoopGroup(1, TaskSchedulerType.Alone);
                _workMultithreadExecutor = new MultithreadEventLoopGroup(AppConfig.ServerOptions.EventLoopCount, TaskSchedulerType.Alone);
            }
        }

        public IEventLoopGroup GetBossMultithreadExecutor()
        {
            return _bossMultithreadExecutor;
        }

        public IEventLoopGroup GetBossEventExecutor()
        {
            return _bossEventExecutor;
        }

        public IEventLoopGroup GetWorkMultithreadExecutor()
        {
            return _workMultithreadExecutor;
        }

        public IEventLoopGroup GetWorkEventExecutor()
        {
            return _workEventExecutor;
        }
    }
}
