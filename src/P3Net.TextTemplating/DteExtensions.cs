using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extensions for <see cref="EnvDTE.DTE"/> instances.</summary>
    public static class DteExtensions
    {        
        /// <summary>Finds a project given its name.</summary>
        /// <param name="source">The source.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The project, if found.</returns>
        public static EnvDTE.Project FindProject ( this EnvDTE.DTE source, string projectName )
        {
            if (String.IsNullOrEmpty(projectName))
                return null;

            foreach (var project in GetAllProjects(source, false))
            {
                if (String.Compare(project.Name, projectName, true) == 0)
                    return project;
            };

            return null;
        }

        /// <summary>Gets all the projects in the solution.</summary>
        /// <param name="source">The source.</param>
        /// <param name="includeFolders"><see langword="true"/> to return folders as well as projects.</param>
        /// <returns>The list of projects.</returns>
        public static IEnumerable<EnvDTE.Project> GetAllProjects ( this EnvDTE.DTE source, bool includeFolders )
        {
            foreach (EnvDTE.Project project in source.Solution.Projects)
            {
                var isFolder = IsProjectFolder(project);

                if (isFolder)
                {
                    foreach (var item in GetProjectsCore(project, includeFolders))
                        yield return item;
                };

                if (!isFolder || includeFolders)
                    yield return project;
            };
        }

        /// <summary>Gets the currently selected project, if any.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The current project.</returns>
        public static EnvDTE.Project GetCurrentProject ( this EnvDTE.DTE source )
        {
            var projects = (Array)((source.ActiveSolutionProjects != null) ? source.ActiveSolutionProjects : null);

            if (projects != null && projects.GetLength(0) > 0)
                return (EnvDTE.Project)projects.GetValue(0);

            return null;
        }

        #region Private Members

        private static IEnumerable<EnvDTE.Project> GetProjectsCore ( EnvDTE.Project project, bool includeFolders )
        {
            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
            {
                if (item.SubProject != null)
                {
                    var isFolder = IsProjectFolder(item.SubProject);
                    if (isFolder)
                    {
                        foreach (var child in GetProjectsCore(item.SubProject, includeFolders))
                            yield return child;
                    };

                    if (!isFolder || includeFolders)
                        yield return item.SubProject;
                };
            };
        }

        private static bool IsProjectFolder ( EnvDTE.Project project )
        {
            return project.Kind == EnvDTE80.ProjectKinds.vsProjectKindSolutionFolder;
        }
        #endregion
    }
}
