/*
 * Copyright © 2018 Michael Taylor
 * https://www.michaeltaylorp3.net
 *
 *  * All Rights Reserved
 */
using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Web.XmlTransform;

namespace P3Net.BuildExtensions.TransformConfigs
{
    /// <summary>Handles transformations on XML files.</summary>
    public class TransformXmlFiles : Task
    {
        /// <summary>Gets or sets the source filename to use as the base for the transforms.</summary>
        [Required]
        public string SourceFile { get; set; }

        /// <summary>Gets or sets the target filename.</summary>
        [Required]
        public string TargetFile { get; set; }

        /// <summary>Gets or sets the list of files to be transformed.</summary>
        [Required]
        public ITaskItem[] TransformFiles { get; set; }

        /// <summary>Gets or sets the base output directory. The environment information is concatenated to this.</summary>
        [Required]
        public string OutputDirectory { get; set; }

        /// <summary>Gets or sets the optional project name to include in the output path.</summary>
        public string ProjectName { get; set; }

        /// <summary>Not used anymore.</summary>
        public string ToolsDirectory { get; set; }

        #region Methods

        public override bool Execute ()
        {
            Log.LogMessage("Begin TransformXmlFiles");

            if (TransformFiles?.Any() ?? false)
            {
                Log.LogMessage("Creating Microsoft.Web.Publishing.Tasks.TransformXml");
               
                foreach (var inputFile in TransformFiles)
                {
                    Log.LogMessage($"Preparing to transform '{inputFile.ItemSpec}'");

                    //Get the env name
                    var fileParts = Path.GetFileNameWithoutExtension(inputFile.ItemSpec).Split('.');
                    var envName = fileParts.LastOrDefault();

                    //Build output directory as base output directory plus environment plus project (if supplied)                        
                    var outDir = Path.Combine(OutputDirectory, envName);
                    if (!String.IsNullOrEmpty(ProjectName))
                        outDir = Path.Combine(outDir, ProjectName);

                    //Build the output path
                    var outFile = Path.Combine(outDir, TargetFile);

                    //Make sure the directory exists
                    if (!Directory.Exists(outDir))
                    {
                        Log.LogMessage($"Creating directory '{outDir}'");
                        Directory.CreateDirectory(outDir);
                    };

                    //Transform the config                                    
                    var transform = new XmlTransformation(inputFile.ItemSpec);
                    var sourceDocument = new XmlTransformableDocument() { PreserveWhitespace = true };
                    sourceDocument.Load(SourceFile);

                    Log.LogMessage($"Transforming file");
                    if (transform.Apply(sourceDocument))
                    {
                        sourceDocument.Save(outFile);
                    } else
                    {
                        Log.LogError($"Error transforming file");
                        return false;
                    };
                };
            };

            Log.LogMessage("End TransformXmlFiles");
            return true;
        }
        #endregion
    }
}
