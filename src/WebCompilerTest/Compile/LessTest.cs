﻿using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebCompiler;

namespace WebCompilerTest
{
    [TestClass]
    public class LessTest
    {
        private ConfigFileProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new ConfigFileProcessor();
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete("../../artifacts/less/test.css");
            File.Delete("../../artifacts/less/test.min.css");
            File.Delete("../../artifacts/less/test2.css");
            File.Delete("../../artifacts/less/test2.min.css");
        }

        [TestMethod, TestCategory("LESS")]
        public void CompileLess()
        {
            var result = _processor.Process("../../artifacts/lessconfig.json");
            Assert.IsTrue(File.Exists("../../artifacts/less/test.css"));
        }

        [TestMethod, TestCategory("LESS")]
        public void CompileLessWithError()
        {
            var result = _processor.Process("../../artifacts/lessconfigerror.json");
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.ElementAt(0).HasErrors);
        }

        [TestMethod, TestCategory("LESS")]
        public void CompileLessWithParsingExceptionError()
        {
            var result = _processor.Process("../../artifacts/lessconfigParseerror.json");
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.ElementAt(0).HasErrors);
            Assert.AreNotEqual(0, result.ElementAt(0).Errors.ElementAt(0).LineNumber, "LineNumber is set when engine.TransformToCss generate a ParsingException");
            Assert.AreNotEqual(0, result.ElementAt(0).Errors.ElementAt(0).ColumnNumber, "ColumnNumber is set when engine.TransformToCss generate a ParsingException");
        }

        [TestMethod, TestCategory("LESS")]
        public void CompileLessWithOptions()
        {
            var result = ConfigHandler.GetConfigs("../../artifacts/lessconfig.json");
            Assert.IsTrue(result.First().Options.Count == 2);
        }

        [TestMethod, TestCategory("LESS")]
        public void CompileLessWithGlobbing()
        {
            Cleanup();
            var result = _processor.Process("../../artifacts/lessconfigglobbing.json");
            Assert.IsTrue(File.Exists("../../artifacts/less/test.css"));
            Assert.IsTrue(File.Exists("../../artifacts/less/test2.css"));
        }

        [TestMethod, TestCategory("LESS")]
        public void AssociateExtensionSourceFileChangedTest()
        {
            var result = _processor.SourceFileChanged("../../artifacts/lessconfig.json", "less/test.less");
            Assert.AreEqual(1, result.Count<CompilerResult>());
        }

        [TestMethod, TestCategory("LESS")]
        public void OtherExtensionTypeSourceFileChangedTest()
        {
            var result = _processor.SourceFileChanged("../../artifacts/lessconfig.json", "scss/test.scss");
            Assert.AreEqual(0, result.Count<CompilerResult>());
        }
    }
}
