using System;
using System.Collections.Generic;
using System.Linq;

namespace P3Net.TextTemplating
{
    /// <summary>Provides extensions for <see cref="EnvDTE.ProjectItem"/> instances.</summary>
    public static class ProjectItemExtensions
    {
        /// <summary>Finds a project item in a project item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="itemName">The item to find.</param>
        /// <param name="recurse"><see langword="true"/> to search recursively.</param>
        /// <returns>The item, if found.</returns>
        public static EnvDTE.ProjectItem FindItem ( this EnvDTE.ProjectItem source, string itemName, bool recurse )
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
                //Search the children of the item, if any			  
                foreach (EnvDTE.ProjectItem child in items)
                {
                    var item = child.FindItem(itemName, true);
                    if (item != null)
                        return item;
                };
            };

            return null;
        }
        
        /// <summary>Gets the file name, if any, of a project item.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The file name.</returns>
        public static string GetFileName ( this EnvDTE.ProjectItem source )
        {
            return (source != null && source.FileCount > 0) ? source.FileNames[0] : "";
        }
    }
}
