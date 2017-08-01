using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extensions for <see cref="EnvDTE.Project"/> instances.</summary>
    public static class ProjectExtensions
    {
        /// <summary>Checks that an assembly is referenced by the project.</summary>
        /// <param name="source">The source.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="minVersion">The optional minimum version.</param>
        /// <returns><see langword="true"/> if the assembly is referenced or <see langword="false"/> otherwise.</returns>
        public static bool CheckAssemblyReference ( this EnvDTE.Project source, string assemblyName, Version minVersion )
        {
            var dependency = new AssemblyDependency[] { new AssemblyDependency("", assemblyName, minVersion) };

            var result = CheckAssemblyReferences(source, dependency);

            return result[0];
        }

        /// <summary>Checks assemblies referenced by the project.</summary>
        /// <param name="source">The source.</param>
        /// <param name="dependencies">The list of dependencies to check.</param>        
        /// <returns>A list of boolean results, one for each dependency.</returns>
        public static List<bool> CheckAssemblyReferences ( this EnvDTE.Project source, IEnumerable<AssemblyDependency> dependencies )
        {
            var references = GetReferencedAssemblies(source, ReferencedAssemblyFlags.AssemblyReferences);
            var results = new List<bool>();

            foreach (var dependency in dependencies)
            {
                var result = false;

                foreach (var reference in references.Cast<VSLangProj.Reference>())
                {
                    if (String.Compare(reference.Name, dependency.AssemblyName, true) == 0)
                    {
                        //If a version is specified
                        if (dependency.AssemblyVersion != null)
                        {
                            var refVersion = new Version(reference.Version);
                            if (refVersion >= dependency.AssemblyVersion)
                            {
                                result = true;
                                break;
                            };
                        } else
                            result = true;
                    };
                };

                results.Add(result);
            };

            return results;
        }

        /// <summary>Finds a project item in a project.</summary>
        /// <param name="source">The source.</param>
        /// <param name="itemName">The item to find.</param>
        /// <param name="recurse"><see langword="true"/> to search recursively.</param>
        /// <returns>The item, if found.</returns>
        public static EnvDTE.ProjectItem FindItem ( this EnvDTE.Project source, string itemName, bool recurse )
        {
            var items = source.ProjectItems;

            //ProjectItems.Item() will throw if the item does not exist so do it the hard way		
            foreach (EnvDTE.ProjectItem child in items)
            {
                if (String.Compare(child.Name, itemName, true) == 0)
                    return child;
            };

            if (recurse)
            {
                foreach (EnvDTE.ProjectItem child in items)
                {
                    var item = child.FindItem(itemName, true);
                    if (item != null)
                        return item;
                };
            };

            return null;
        }

        /// <summary>Gets a project from a hierarchy instance.</summary>
        /// <param name="hierarchy">The hierarchy.</param>
        /// <returns>The project.</returns>
        /// <exception cref="Exception">Unable to get the project.</exception>
        public static EnvDTE.Project FromHierarchy ( IVsHierarchy hierarchy )
        {
            object project;

            if (hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out project) != 0)
                throw new Exception("Unable to get project from hierarchy.");

            return project as EnvDTE.Project;
        }

        /// <summary>Gets the configuration file associated with a project.</summary>
        /// <param name="source">The source value.</param>
        /// <returns>The project's configuration file, if any.</returns>
        public static EnvDTE.ProjectItem GetConfigurationFileItem ( this EnvDTE.Project source )
        {
            return source.FindItem("web.config", false) ?? source.FindItem("app.config", false);
        }

        /// <summary>Gets a hierarchy from a project.</summary>
        /// <param name="source">The source.</param>
        /// <param name="solution">The parent solution.</param>
        /// <returns>The hierarchy.</returns>
        /// <exception cref="Exception">The solution could not be found.</exception>
        public static IVsHierarchy GetHierarchyFromProject ( this EnvDTE.Project source, IVsSolution solution )
        {
            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(source.FullName, out hierarchy);
            return hierarchy;
        }

        /// <summary>Gets the file information for the project.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The project's file information..</returns>
        public static FileInfo GetProjectFileInfo ( this EnvDTE.Project source )
        {
            return new System.IO.FileInfo(source.FullName);
        }

        /// <summary>Gets the referenced assemblies of a project.</summary>
        /// <param name="source">The source.</param>
        /// <param name="flags">The assemblies to return.</param>
        /// <returns>The referenced assemblies.</returns>
        public static IEnumerable<VSLangProj.Reference> GetReferencedAssemblies ( this EnvDTE.Project source, ReferencedAssemblyFlags flags )
        {
            var references = new Dictionary<string, VSLangProj.Reference>();

            var vsProject = source.Object as VSLangProj.VSProject;
            if (vsProject != null)
            {
                var includeAll = flags == ReferencedAssemblyFlags.All;
                var includeSystem = (flags & ReferencedAssemblyFlags.SystemAssembly) == ReferencedAssemblyFlags.SystemAssembly;
                var includeUser = (flags & ReferencedAssemblyFlags.UserDefinedAssembly) == ReferencedAssemblyFlags.UserDefinedAssembly;
                var includeCom = (flags & ReferencedAssemblyFlags.ComReferences) == ReferencedAssemblyFlags.ComReferences;
                var includeProject = (flags & ReferencedAssemblyFlags.ProjectReferences) == ReferencedAssemblyFlags.ProjectReferences;

                foreach (VSLangProj.Reference reference in vsProject.References)
                {
                    if (includeAll)
                        references[reference.Identity] = reference;
                    else
                    {
                        bool add = false;

                        //Assembly vs COM
                        if (reference.Type == VSLangProj.prjReferenceType.prjReferenceTypeAssembly)
                        {
                            //Binary vs project reference
                            if (reference.SourceProject == null && (includeSystem || includeUser))
                            {
                                //System vs user
                                if (IsSystemReference(reference))
                                {
                                    if (includeSystem)
                                        add = true;
                                } else if (includeUser)
                                    add = true;
                            } else if (includeProject)  //Project reference
                                add = true;
                        } else if (reference.Type == VSLangProj.prjReferenceType.prjReferenceTypeActiveX && includeCom)
                            add = true;

                        if (add)
                            references[reference.Identity] = reference;
                    };
                };
            };

            return references.Values;
        }

        #region Private Members

        private static bool IsSystemReference ( VSLangProj.Reference reference )
        {
            var path = Path.GetFileNameWithoutExtension(reference.Path);

            return path.StartsWith("System.", StringComparison.OrdinalIgnoreCase) || path.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                    || String.Compare(path, "mscorlib", true) == 0 || String.Compare(path, "system", true) == 0;
        }
        #endregion
    }
}
