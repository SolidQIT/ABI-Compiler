using SolidQ.ABI.Compiler.Infrastructure.Extensibility.Compiler;
using SolidQ.ABI.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SolidQ.ABI.Compiler.Infrastructure.Extensibility
{
    public class PluginCollector : IDisposable
    {
        private ReadOnlyCollection<IPlugin> _plugins;
        private bool _disposed;

        public ReadOnlyCollection<IPlugin> Plugins
        {
            get
            {
                if (_plugins == null)
                    _plugins = Collect();

                return _plugins;
            }
        }

        private ReadOnlyCollection<IPlugin> Collect()
        {
            var plugins = new List<IPlugin>();

            using (var container = new PluginCompositionContainer<MetadataCompilerPluginBootstrapper>())
            {
                var bootstrapper = container.Compose();

                plugins.AddRange(bootstrapper.MetadataCompilers);               
            }

            var duplicatedPlugins = plugins.GroupBy((p) => p.Name)
                .Where((g) => g.Count() > 1)
                .Select((g) => g.Key)
                .ToArray();

            if (duplicatedPlugins.Length > 0)
                throw new ApplicationException(string.Format("Duplicated plugin names found [{0}]", string.Join(";", duplicatedPlugins)));

            return plugins.AsReadOnly();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // ???
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
