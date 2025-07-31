/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels
{
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;

    /// <summary>
    /// Default <see cref="SingleThreadEventLoopBase"/> implementation which just execute all submitted task in a serial fashion.
    /// </summary>
    public class DefaultEventLoop : SingleThreadEventLoopBase
    {
        public DefaultEventLoop()
            : this(null)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent)
            : this(parent, DefaultMaxPendingTasks, TaskSchedulerType.Default)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, TaskSchedulerType taskSchedulerType)
          : this(parent, DefaultMaxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, RejectedExecutionHandlers.Reject(), maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, RejectedExecutionHandlers.Reject(), queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : this(parent, rejectedHandler, queueFactory: null, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, DefaultThreadFactory<DefaultEventLoop>.Instance, rejectedHandler, maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, DefaultThreadFactory<DefaultEventLoop>.Instance, rejectedHandler, queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), queueFactory, taskSchedulerType)
        {
        }

#if DEBUG
        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(maxPendingTasks), NewBlockingTaskQueue(maxPendingTasks), rejectedHandler,taskSchedulerType)
        {
            Start();
        }

        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(queueFactory), NewBlockingTaskQueue(queueFactory), rejectedHandler,taskSchedulerType)
        {
            Start();
        }
#else
        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(maxPendingTasks), rejectedHandler, taskSchedulerType)
        {
            Start();
        }

        public DefaultEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(queueFactory), rejectedHandler, taskSchedulerType)
        {
            Start();
        }
#endif

        private static IQueue<IRunnable> NewBlockingTaskQueue(IEventLoopTaskQueueFactory queueFactory)
        {
            if (queueFactory is null)
            {
                return NewBlockingTaskQueue(DefaultMaxPendingTasks);
            }
            return queueFactory.NewTaskQueue(DefaultMaxPendingTasks);
        }

        protected override void Run()
        {
            do
            {
                IRunnable task = TakeTask();
                if (task is object)
                {
                    task.Run();
                    UpdateLastExecutionTime();
                }
            } while (!ConfirmShutdown());
        }
    }
}