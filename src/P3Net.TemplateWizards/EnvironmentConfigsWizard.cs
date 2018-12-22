using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.TemplateWizard;

using P3Net.TextTemplating;

namespace P3Net.TemplateWizards
{
    public class EnvironmentConfigsWizard : IWizard
    {
        #region IWizard Members

        void IWizard.BeforeOpeningFile ( EnvDTE.ProjectItem projectItem )
        {
        }

        void IWizard.ProjectFinishedGenerating ( EnvDTE.Project project )
        {
        }

        void IWizard.ProjectItemFinishedGenerating ( EnvDTE.ProjectItem projectItem )
        {
        }

        void IWizard.RunFinished ()
        {
        }

        void IWizard.RunStarted ( object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams )
        {
            try
            {
                var dte = automationObject as EnvDTE.DTE;

                RunStartedCore(dte, replacementsDictionary, runKind, customParams);
            } catch (WizardCancelledException)
            {
                throw;
            } catch (Exception e)
            {
                ReportErrorAndCancel("Error processing template.", e);
            };
        }

        bool IWizard.ShouldAddProjectItem ( string filePath )
        {
            return true;
        }
        #endregion

        #region Private Members

        #region Types

        private enum ProjectType { Web, Windows };

        private sealed class ConfigurationFileItem
        {
            public EnvDTE.ProjectItem ConfigurationItem { get; set; }
            public ProjectType ProjectType { get; set; }

            public string BaseConfigFileName { get; set; }
        }

        private const string SharedTargetsFileName = "P3Net.transformconfig.props";
        private const string PackageName = "P3Net.BuildExtensions.TransformConfigs";
        #endregion

        private void RunStartedCore ( EnvDTE.DTE dte, 
                        Dictionary<string, string> replacementsDictionary, 
                        WizardRunKind runKind, 
                        object[] customParams )
        {
            //Get the current project                
            var project = dte.GetCurrentProject();

            //Get the configuration file
            var configFile = GetConfigurationFile(project);
            if (configFile == null)
                ReportErrorAndCancel("No configuration file could be found.");

            //Verify the standard target import is defined
            EnsureStandardTargetIsImported(project);

            //Set the template parameters
            replacementsDictionary.Add("$IsWebProject$", configFile.ProjectType == ProjectType.Web ? "1" : "0");
            replacementsDictionary.Add("$BaseConfigFileName$", configFile.BaseConfigFileName);

            //Remove the default config transforms if they exist            
            RemoveProjectItem(configFile.ConfigurationItem, configFile.BaseConfigFileName + ".Debug.config");
            RemoveProjectItem(configFile.ConfigurationItem, configFile.BaseConfigFileName + ".Release.config");
        }

        private void EnsureStandardTargetIsImported ( EnvDTE.Project project )
        {
            var buildProject = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(project.FullName).First();

            //Check for the import
            var hasImport = (from i in buildProject.Xml.Imports
                                where String.Compare(Path.GetFileName(i.Project), SharedTargetsFileName, true) == 0
                                select i).Any();
            if (hasImport)
                return;

            ReportErrorAndConfirm($"This item requires that the NuGet package {PackageName} be installed in the project. Do you want to add the template anyway?");
        }
        
        private ConfigurationFileItem GetConfigurationFile ( EnvDTE.Project project )
        {
            var item = project.GetConfigurationFileItem();
            if (item == null)
                return null;

            var filename = Path.GetFileNameWithoutExtension(item.GetFileName());
            var projectType = String.Compare(filename, "web", true) == 0 ? ProjectType.Web : ProjectType.Windows;
            return new ConfigurationFileItem() { ConfigurationItem = item, ProjectType = projectType, BaseConfigFileName = filename };
        }

        private void RemoveProjectItem ( EnvDTE.ProjectItem parentItem, string itemName )
        {
            try
            {
                var item = parentItem.FindItem(itemName, false);
                if (item != null)
                    item.Delete();
            } catch
            {
                ReportError(String.Format("Unable to remove default transform '{0}'.  You should delete this manually.", itemName));
            };
        }

        private void ReportError ( string message, Exception error = null )
        {
            var msg = (error != null) ? String.Format("{0}\r\n{1}", message, error.Message) : message;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ReportErrorAndCancel ( string message, Exception error = null )
        {
            ReportError(message, error);

            throw new WizardCancelledException(message, error);
        }

        private void ReportErrorAndConfirm ( string message, Exception error = null )
        {
            var msg = (error != null) ? String.Format("{0}\r\n{1}", message, error.Message) : message;
            if (MessageBox.Show(msg, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                throw new WizardCancelledException(message, error);
        }
        #endregion
    }
}