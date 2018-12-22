using System;
using System.Collections.Generic;

namespace Framework.ServiceModel
{
    /// <summary>Represents an object that can open and close persistent channels.</summary>
    public interface ISupportsPersistentChannel
    {
        /// <summary>Closes the channel.</summary>
        void Close ();

        /// <summary>Opens the channel.</summary>
        void Open ();
    }
}
