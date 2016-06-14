using Newtonsoft.Json.Linq;
using NLog;
using SolidQ.ABI.Extensibility.Compiler;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;

namespace SolidQ.ABI.Plugin.Common.MetadataCompiler
{
    [Export(typeof(IMetadataCompilerPlugin))]
    public class FooMetadataCompiler : IMetadataCompilerPlugin
    {
        private static Logger _logger;

        #region Properties

        public string Name
        {
            get
            {
                return "Foo";
            }
        }

        public string Author
        {
            get
            {
                return "SolidQ";
            }
        }

        public string Version
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            }
        }

        public string Description
        {
            get 
            {
                return "Foo plugin example";
            }
        }

        public string Help
        {
            get
            {
                return "A sample plugin. It will just print all the argument passed.";
            }
        }

        #endregion

        public void Initialize(LogFactory log)
        {
            _logger = log.GetCurrentClassLogger();
            _logger.Debug("Initialize");
        }

        public void Shutdown()
        {
            _logger.Debug("Shutdown");
        }
        
        /// <summary>
        /// *** Don't modify, used for unit testing ***
        /// </summary>
        public dynamic Compile(params string[] args)
        {
            _logger.Debug("Compile");

            dynamic arguments = new JArray();

            foreach (var arg in args)
                arguments.Add(new JValue(arg));

            return arguments;
        }
    }
}
