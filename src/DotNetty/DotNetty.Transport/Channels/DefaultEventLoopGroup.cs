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
    using System;
    using DotNetty.Common.Concurrency;

    /// <summary>
    /// <see cref="MultithreadEventLoopGroup{T1, T2}"/> which must be used for the local transport.
    /// </summary>
    public class DefaultEventLoopGroup : MultithreadEventLoopGroup<DefaultEventLoopGroup, DefaultEventLoop>
    {
        private static readonly Func<DefaultEventLoopGroup, DefaultEventLoop> DefaultEventLoopFactory;

        static DefaultEventLoopGroup()
        {
            DefaultEventLoopFactory = group => new DefaultEventLoop(group);
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup()
            : base(0, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads)
            : base(nThreads, DefaultEventLoopFactory)
        {
        }

        public DefaultEventLoopGroup(TaskSchedulerType taskSchedulerType)
     : this(0, taskSchedulerType)
        {
        }

        public DefaultEventLoopGroup(int nThreads, TaskSchedulerType taskSchedulerType)
         : base(nThreads, group => new DefaultEventLoop(group,  taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IRejectedExecutionHandler rejectedHandler)
            : this(0, rejectedHandler, TaskSchedulerType.Default)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IEventLoopTaskQueueFactory queueFactory)
            : this(0, queueFactory, TaskSchedulerType.Default)
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, queueFactory, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, rejectedHandler, queueFactory, taskSchedulerType))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IThreadFactory threadFactory)
            : this(0, threadFactory, TaskSchedulerType.Default)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, queueFactory: null, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory: null, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, queueFactory, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory, taskSchedulerType))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory)
            : base(0, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory)
            : base(nThreads, chooserFactory, DefaultEventLoopFactory)
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, queueFactory: null, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, queueFactory, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, rejectedHandler, queueFactory, taskSchedulerType))
        {
        }


        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, maxPendingTasks, taskSchedulerType))
        {
        }

        /// <summary>Creates a new instance of <see cref="DefaultEventLoopGroup"/>.</summary>
        public DefaultEventLoopGroup(int nThreads, IThreadFactory threadFactory, IEventExecutorChooserFactory<DefaultEventLoop> chooserFactory,
            IRejectedExecutionHandler rejectedHandler, IEventLoopTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(nThreads, chooserFactory, group => new DefaultEventLoop(group, threadFactory, rejectedHandler, queueFactory, taskSchedulerType))
        {
        }
    }
}