using ChristmasLightServer;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;

namespace ChristmasLightServerTests
{
    [TestClass]
    public class LightServiceTests
    {
        private LightService _lightService;
        private ILogger<LightService> _logger;
        private ICommandSender _commandSender;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger<LightService>>();
            _commandSender = Substitute.For<ICommandSender>();
            _lightService = new LightService(_logger);            
        }

        [TestMethod]
        public void RegisterCommandTest()
        {
            _lightService.RegisterCommand(_commandSender);
            _logger.Received().LogInformation("RegisterCommandSender");
        }

        [TestMethod]
        public void QueueStripChange_NoCommand()
        {
            var commands = new List<SetLightColorCommand>();
            _lightService.QueueStripChange(commands);
            _logger.Received().LogError("CommandSenderOffline");
        }

        [TestMethod]
        public void QueueStripChange_ThenRegisterCommand()
        {
            var commands = new List<SetLightColorCommand>();
            _lightService.QueueStripChange(commands);
            _logger.Received().LogError("CommandSenderOffline");
        }


    }
}