using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using T4Toolbox;

namespace P3Net.TextTemplating
{
    /// <summary>Base class for T4 templates.</summary>
    public abstract class MyTemplate : CSharpTemplate
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

        /// <summary>Gets the template's file information.</summary>
        public FileInfo TemplateFileInfo
        {
            get { return new FileInfo(TransformationContext.Current.Host.TemplateFile); }
        }

        /// <summary>Gets the name of the base template.</summary>
        public string TemplateName
        {
            get { return Path.GetFileNameWithoutExtension(TemplateFileInfo.Name); }
        }

        /// <summary>Gets the directory containing to the template.</summary>
        public string TemplatePath
        {
            get { return Path.GetDirectoryName(TransformationContext.Current.Host.TemplateFile); }
        }

        /// <summary>Gets the VS solution, if any.</summary>
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

        /// <summary>Writes attributes for the generated interfaces.</summary>        
        /// <param name="endLine"><see langword="true"/> to generate an end of line after the attributes.</param>
        /// <remarks>
        /// The default implementation applies code generation attributes on the type.
        /// <para/>
        /// When the generated attributes are followed by an inline template then set <paramref name="endLine"/> to <see langword="false"/>
        /// so that a blank line will not appear in the output.
        /// </remarks>
        public void WriteInterfaceAttributes ( bool endLine = false )
        {
            var builder = new StringBuilder();

            WriteTypeAttributesCore(builder, true);

            //Remove any ending if needed
            if (endLine)
            {
                if (!builder.EndsWith(Environment.NewLine))
                    builder.AppendLine();
            } else
            {
                if (builder.EndsWith(Environment.NewLine))
                    builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            };

            Write(builder.ToString());
        }

        /// <summary>Writes type attributes for the generated type.</summary>        
        /// <param name="endLine"><see langword="true"/> to generate an end of line after the attributes.</param>
        /// <remarks>
        /// The default implementation applies code generation attributes on the type.
        /// <para/>
        /// When the generated attributes are followed by an inline template then set <paramref name="endLine"/> to <see langword="false"/>
        /// so that a blank line will not appear in the output.
        /// </remarks>
        public void WriteTypeAttributes ( bool endLine = false )
        {
            var builder = new StringBuilder();

            WriteTypeAttributesCore(builder, false);

            //Remove any ending if needed
            if (endLine)
            {
                if (!builder.EndsWith(Environment.NewLine))
                    builder.AppendLine();
            } else
            {
                if (builder.EndsWith(Environment.NewLine))
                    builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            };

            Write(builder.ToString());
        }
        #endregion

        #region Compiler Issues - Shouldn't be necessary

        /// <summary>Indents subsequent generated code.</summary>
        /// <param name="count">The number of times to indent.</param>
        public MyTemplate Indent ( int count = 1 )
        {
            TextTransformationExtensions.Indent(this, count);

            return this;
        }

        /// <summary>Makes a unique identifier.</summary>
        /// <param name="prefix">The optional prefix.</param>
        /// <param name="suffix">The optional suffix.</param>
        /// <returns>The unique identifier.</returns>
        public string MakeUniqueIdentifier ( string prefix = null, string suffix = null )
        {
            return TextTransformationExtensions.MakeUniqueIdentifier(this, prefix, suffix);
        }

        /// <summary>Unindents subsequent generated code.</summary>
        /// <param name="count">The number of times to unindent.</param>
        public MyTemplate Unindent ( int count = 1 )
        {
            TextTransformationExtensions.Unindent(this, count);

            return this;
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

        /// <summary>Writes type attributes for the generated type.</summary>        
        /// <remarks>
        /// The default implementation applies code generation attributes on the type.
        /// </remarks>
        protected virtual void WriteTypeAttributesCore ( StringBuilder builder, bool isinterface )
        {
            var templateName = Path.GetFileNameWithoutExtension(TransformationContext.Current.Host.TemplateFile);
            builder.AppendLine("[System.CodeDom.Compiler.GeneratedCode(\"{0}\", \"\")]", templateName);

            if (!isinterface)
            {
                builder.AppendLine("[System.Diagnostics.DebuggerNonUserCode]");
                builder.AppendLine("[System.Diagnostics.DebuggerStepThrough]");
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
