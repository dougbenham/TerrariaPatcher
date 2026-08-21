using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace TerrariaPatcher
{
    class MyAssemblyResolver : BaseAssemblyResolver
    {
        private readonly string _extraDirectory;

        public MyAssemblyResolver(string extraDirectory)
        {
            this._extraDirectory = extraDirectory;
        }

        protected override AssemblyDefinition SearchDirectory(AssemblyNameReference name, IEnumerable<string> directories, ReaderParameters parameters)
        {
            return base.SearchDirectory(name, directories.Concat(new[] {_extraDirectory}), parameters);
        }
    }
}
