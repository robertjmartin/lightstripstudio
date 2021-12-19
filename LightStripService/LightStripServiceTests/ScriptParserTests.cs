using ChristmasLightServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristmasLightServerTests
{
    [TestClass]
    public class ScriptParserTests
    {
        [TestMethod]
        public void Test()
        {
            ScriptParser parser = new ScriptParser();

            parser.StartScript("setColor(0, rgbColor(1,2,3))");
            var result = parser.GetCommandsFromScript();
        }

        [TestMethod]
        public void Test2()
        {
            ScriptParser parser = new ScriptParser();

            parser.StartScript("setColor(0, hsvColor(315,100,64))");
            var result = parser.GetCommandsFromScript();
        }
    }
}
