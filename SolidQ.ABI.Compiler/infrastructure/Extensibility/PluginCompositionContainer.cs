using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace SolidQ.ABI.Compiler.Infrastructure.Extensibility.Compiler
{
    internal class PluginCompositionContainer<T> : IDisposable where T : class, new()
    {
        private AggregateCatalog _aggreagateCatalog;
        private bool _disposed;

        public PluginCompositionContainer()
        {
            _aggreagateCatalog = new AggregateCatalog();
        }

        public T Compose()
        {
            foreach(var path in Directory.GetDirectories(CompilerEngine.PluginsPath))
                _aggreagateCatalog.Catalogs.Add(item: new DirectoryCatalog(path));            

            var bootstrapper = new T();

            using (var container = new CompositionContainer(_aggreagateCatalog))
                container.ComposeParts(bootstrapper);

            return bootstrapper;
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
                    foreach (var catalog in _aggreagateCatalog.Catalogs)
                        catalog.Dispose();                    

                    _aggreagateCatalog.Dispose();
                }

                _disposed = true;   
            }
        }

        #endregion
    }
}
