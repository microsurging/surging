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

namespace DotNetty.Common.Concurrency
{
    using DotNetty.Common.Internal;

    /// <summary>
    /// Default <see cref="SingleThreadEventExecutor"/> implementation which just execute all submitted task in a serial fashion.
    /// </summary>
    public sealed class DefaultEventExecutor : SingleThreadEventExecutor
    {
        public DefaultEventExecutor()
            : this(DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(int maxPendingTasks)
            : this(RejectedExecutionHandlers.Reject(), maxPendingTasks, TaskSchedulerType.Default)
        {
        }

        public DefaultEventExecutor(TaskSchedulerType taskSchedulerType)
          : this(RejectedExecutionHandlers.Reject(), DefaultMaxPendingExecutorTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler)
            : this(rejectedHandler, DefaultMaxPendingExecutorTasks, TaskSchedulerType.Default)
        {
        }

        public DefaultEventExecutor(IEventExecutorTaskQueueFactory queueFactory)
            : this(RejectedExecutionHandlers.Reject(), queueFactory, TaskSchedulerType.Default)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(null, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(null, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, queueFactory, taskSchedulerType)
        {
        }


        public DefaultEventExecutor(IThreadFactory threadFactory)
            : this(threadFactory, DefaultMaxPendingExecutorTasks, TaskSchedulerType.Default)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(threadFactory, RejectedExecutionHandlers.Reject(), maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(threadFactory, RejectedExecutionHandlers.Reject(), queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : this(threadFactory, rejectedHandler, DefaultMaxPendingExecutorTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(null, threadFactory, rejectedHandler, maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(null, threadFactory, rejectedHandler, queueFactory, taskSchedulerType)
        {
        }


        public DefaultEventExecutor(IEventExecutorGroup parent)
            : this(parent, DefaultMaxPendingExecutorTasks, TaskSchedulerType.Default)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, RejectedExecutionHandlers.Reject(), maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, RejectedExecutionHandlers.Reject(), queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler, TaskSchedulerType taskSchedulerType)
            : this(parent, rejectedHandler, queueFactory: null, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, int maxPendingTasks, TaskSchedulerType taskSchedulerType)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), maxPendingTasks, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), queueFactory, taskSchedulerType)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks,TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(maxPendingTasks), rejectedHandler, taskSchedulerType)
        {
            Start();
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory, TaskSchedulerType taskSchedulerType)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(queueFactory), rejectedHandler, taskSchedulerType)
        {
            Start();
        }

        private static IQueue<IRunnable> NewBlockingTaskQueue(IEventExecutorTaskQueueFactory queueFactory)
        {
            if (queueFactory is null)
            {
                return NewBlockingTaskQueue(DefaultMaxPendingExecutorTasks);
            }
            return queueFactory.NewTaskQueue(DefaultMaxPendingExecutorTasks);
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