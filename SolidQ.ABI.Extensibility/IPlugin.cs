using System;
using NLog;

namespace SolidQ.ABI.Extensibility
{
    public interface IPlugin
    {
        string Name { get; }
        string Author { get; }
        string Version { get; }
        string Description { get; }
        string Help { get; }
        void Initialize(LogFactory log);
        void Shutdown();
    }
}
