using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using T4Toolbox;

namespace P3Net.TextTemplating
{
    /// <summary>Provides a base class for template generators.</summary>
    public abstract class MyGenerator : Generator
    {
        #region Attributes

        /// <summary>Gets the active project, if any.</summary>
        public EnvDTE.Project ActiveProject
        {
            get
            {
                if (m_activeProject == null && DteInstance != null)
                    m_activeProject = DteInstance.GetCurrentProject();

                return m_activeProject;
            }
        }

        /// <summary>Gets the DTE instance.</summary>
        public EnvDTE.DTE DteInstance
        {
            get
            {
                if (m_dte == null)
                    m_dte = (EnvDTE.DTE)TransformationContext.Current.GetService(typeof(EnvDTE.DTE));

                return m_dte;
            }
        }

        /// <summary>Gets or sets whether dependency checking is enabled.</summary>
        /// <remarks>The default is <see langword="false"/>.</remarks>
        public bool SkipDependencyChecks { get; set; }

        public IVsSolution VsSolution
        {
            get
            {
                if (m_solution == null)
                    m_solution = GetService<SVsSolution>() as IVsSolution;

                return m_solution;
            }
        }
        #endregion

        #region Methods

        /// <summary>Finds a project in the solution.</summary>
        /// <param name="projectName">The name of the project.</param>        
        /// <returns>The project or <see langword="null"/> if not found.</returns>              
        public EnvDTE.Project FindProject ( string projectName )
        {
            return DteInstance.FindProject(projectName);
        }

        /// <summary>Gets a project from a hierarchy instance.</summary>
        /// <param name="hierarchy">The hierarchy.</param>
        /// <returns>The project.</returns>
        /// <exception cref="TransformationException">Unable to get the project.</exception>
        public EnvDTE.Project GetProjectFromHierarchy ( IVsHierarchy hierarchy )
        {
            try
            {
                return ProjectExtensions.FromHierarchy(hierarchy);
            } catch (TransformationException)
            {
                throw;
            } catch (Exception e)
            {
                throw new TransformationException("Unable to get project.", e);
            };
        }

        /// <summary>Gets a service.</summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service instance.</returns>
        public T GetService<T>() where T : class
        {
            return (T)TransformationContext.Current.GetService(typeof(T));
        }
        #endregion

        #region Protected Members

        /// <summary>Gets the dependencies of the template.</summary>
        /// <returns>A list of dependencies.</returns>
        /// <remarks>The default implementation has no dependencies.</remarks>
        protected virtual IEnumerable<AssemblyDependency> GetDependencies ()
        {
            return null;
        }

        /// <summary>Initializes the child template.</summary>
        /// <param name="template">The template.</param>
        protected virtual T InitializeTemplate<T>( T template ) where T : MyTemplate
        {
            template.SkipDependencyChecks = this.SkipDependencyChecks;

            return template;
        }

        /// <summary>Validates the template.</summary>
        /// <remarks>
        /// The default implementation uses <see cref="Validator"/> to perform the validation.
        /// </remarks>
        protected override void Validate ()
        {
            base.Validate();

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            foreach (var result in results)
            {
                Error(result.ErrorMessage);
            };

            if (!SkipDependencyChecks)
            {
                var dependencies = GetDependencies() ?? Enumerable.Empty<AssemblyDependency>();
                if (dependencies.Any())
                {
                    var checks = ActiveProject.CheckAssemblyReferences(dependencies);
                    int index = 0;
                    foreach (var dependency in dependencies)
                    {
                        if (!checks[index++])
                            Error("A reference to '{0}' version '{1}' or higher is required.", dependency.DisplayName, dependency.AssemblyVersion);
                    };
                };
            };
        }
        #endregion

        #region Private Members

        //Cache
        private EnvDTE.DTE m_dte;
        private EnvDTE.Project m_activeProject;
        private IVsSolution m_solution;
        #endregion
    }
}
