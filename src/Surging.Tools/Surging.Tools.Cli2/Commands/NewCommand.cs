using McMaster.Extensions.CommandLineUtils;
using Surging.Tools.Cli2.Commands.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Tools.Cli2.Commands
{
    [Command("New", Description = "Command-line  service see tool")]
    public class NewCommand
    {
        private async Task OnExecute(CommandLineApplication app, IConsole console)
        { 
        }
    }
}
