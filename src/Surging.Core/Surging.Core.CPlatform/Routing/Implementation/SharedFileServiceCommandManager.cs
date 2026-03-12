using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Support;
using Surging.Core.CPlatform.Support.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing.Implementation
{
    public class SharedFileServiceCommandManager : ServiceCommandManagerBase, IDisposable
    {
        #region Field

        private readonly string _filePath;
        private readonly ISerializer<string> _serializer;
        private readonly ILogger<SharedFileServiceCommandManager> _logger;
        private ServiceCommandDescriptor[] _serviceCommands;
        private readonly FileSystemWatcher _fileSystemWatcher;

        #endregion Field

        #region Constructor

        public SharedFileServiceCommandManager(string filePath, ISerializer<string> serializer,
            IServiceEntryManager serviceEntryManager,ILogger<SharedFileServiceCommandManager> logger) : base(serializer, serviceEntryManager)
        {
            _filePath = filePath;
            _serializer = serializer;
            _logger = logger;

            var directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            if (!File.Exists(filePath)) File.Create(filePath).Close();
            _fileSystemWatcher = new FileSystemWatcher(directoryName, Path.GetFileName(filePath));

            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        #endregion Constructor

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _fileSystemWatcher?.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Overrides of ServiceRouteManagerBase

        /// <summary>
        ///     获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommandsAsync()
        {
            if (_serviceCommands == null)
                await EntryRoutes(_filePath);
            return _serviceCommands;
        }

 
        /// <summary>
        ///     清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override Task ClearAsync()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
            return Task.FromResult(0);
        }

        /// <summary>
        ///     设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        public override async Task SetServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> commands)
        {
            using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                fileStream.SetLength(0);
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(_serializer.Serialize(commands));
                }
            }
        }
 

        #endregion Overrides of ServiceRouteManagerBase

        #region Private Method

        private async Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommands(string file)
        {
            ServiceCommandDescriptor[] commands;
            if (File.Exists(file))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从文件：{file}中获取服务路由。");
                string content;
                while (true)
                {
                    try
                    {
                        using (
                            var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var reader = new StreamReader(fileStream, Encoding.UTF8);
                            content = await reader.ReadToEndAsync();
                        }
                        break;
                    }
                    catch (IOException)
                    {
                    }
                }
                try
                {
                    var serializer = _serializer;
                    commands = serializer.Deserialize<string, ServiceCommandDescriptor[]>(content);
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation(
                            $"成功获取到以下路由信息：{string.Join(",", commands.Select(i => i.ServiceId))}。");
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError(exception, "获取路由信息时发生了错误。");
                    commands = new ServiceCommandDescriptor[0];
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为文件：{file}不存在。");
                commands = new ServiceCommandDescriptor[0];
            }
            return commands;
        }

        private async Task EntryRoutes(string file)
        {
            var oldCommands = _serviceCommands?.ToArray();
            var newCommands = (await GetServiceCommands(file)).ToArray();
            _serviceCommands = newCommands;
            if (oldCommands == null)
            {
                //触发服务路由创建事件。
                OnCreated(newCommands.Select(command => new ServiceCommandEventArgs(command)).ToArray());
            }
            else
            {
                //旧的服务Id集合。
                var oldServiceIds = oldCommands.Select(i => i.ServiceId).ToArray();
                //新的服务Id集合。
                var newServiceIds = newCommands.Select(i => i.ServiceId).ToArray();

                //被删除的服务Id集合
                var removeServiceIds = oldServiceIds.Except(newServiceIds).ToArray();
                //新增的服务Id集合。
                var addServiceIds = newServiceIds.Except(oldServiceIds).ToArray();
                //可能被修改的服务Id集合。
                var mayModifyServiceIds = newServiceIds.Except(removeServiceIds).ToArray();

                //触发服务路由创建事件。
                OnCreated(
                    newCommands.Where(i => addServiceIds.Contains(i.ServiceId))
                        .Select(command => new ServiceCommandEventArgs(command))
                        .ToArray());

                //触发服务路由删除事件。
                OnRemoved(
                    oldCommands.Where(i => removeServiceIds.Contains(i.ServiceId))
                        .Select(command => new ServiceCommandEventArgs(command))
                        .ToArray());

                //触发服务路由变更事件。
                var currentMayModifyCommands =
                    newCommands.Where(i => mayModifyServiceIds.Contains(i.ServiceId)).ToArray();
                var oldMayModifyCommands =
                    oldCommands.Where(i => mayModifyServiceIds.Contains(i.ServiceId)).ToArray();

                foreach (var oldMayModifyRoute in oldMayModifyCommands)
                {
                    if (!currentMayModifyCommands.Contains(oldMayModifyRoute))
                        OnChanged(
                            new ServiceCommandChangedEventArgs(
                                currentMayModifyCommands.First(
                                    i => i.ServiceId == oldMayModifyRoute.ServiceId),
                                oldMayModifyRoute));
                }
            }
        }

        private async void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"文件{_filePath}发生了变更，将重新获取路由信息。");

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                string content;
                try
                {
                    content = File.ReadAllText(_filePath, Encoding.UTF8);
                }
                catch (IOException) //还没有操作完，忽略本次修改
                {
                    return;
                }
                if (!string.IsNullOrWhiteSpace(content))
                {
                    await EntryRoutes(_filePath);
                }
                else
                {
                    return;
                }
            }

            await EntryRoutes(_filePath);
        }

        public override ValueTask AddNodeMonitorWatcher(string serviceId)
        {
            return ValueTask.CompletedTask;
        }

        protected override async Task InitServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> serviceCommands)
        {
            await SetServiceCommandsAsync(serviceCommands);
        }

        #endregion Private Method
    }
}

