using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Framework.ServiceModel
{
    /// <summary>Provides a base class for wrapping WCF channels.</summary>
    /// <typeparam name="T">The type of the channel.</typeparam>
    public abstract class ServiceChannelClient<T> : ISupportsPersistentChannel where T: class
    {
        /// <summary>Determines if the channel is already open.</summary>
        protected bool HasOpenChannel
        {
            get { return m_channel != null; }
        }

        /// <summary>Closes the persistent channel if it is open.</summary>
        protected virtual void CloseChannel ()
        {
            var channel = Interlocked.Exchange(ref m_channel, null);
            if (channel != null)
                channel.Close();
        }

        /// <summary>Creates an instance of the client wrapper.</summary>
        /// <returns>The client wrapper.</returns>
        protected virtual ServiceClientWrapper<T> CreateInstance ()
        {
            return ServiceClientFactory.CreateAndWrap<T>();
        }                

        /// <summary>Invokes a method on the channel.</summary>
        /// <param name="action">The action to perform.</param>
        protected virtual void InvokeMethod ( Action<T> action )
        {
            if (HasOpenChannel)
                action(m_channel.Client);
            else
                ServiceClientFactory.InvokeMethod<T>(action, CreateInstance);
        }

        /// <summary>Invokes a method on the channel.</summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action to perform.</param>
        protected virtual TResult InvokeMethod<TResult> ( Func<T, TResult> action )
        {
            return HasOpenChannel ? action(m_channel.Client) : ServiceClientFactory.InvokeMethod<T, TResult>(action, CreateInstance);
        }

        /// <summary>Opens the persistent channel if it is not open.</summary>
        protected virtual void OpenChannel ()
        {
            if (m_channel == null)
            {
                var channel = CreateInstance();
                var oldChannel = Interlocked.CompareExchange(ref m_channel, channel, null);
                if (oldChannel != null)
                    channel.Close();
            };
        }

        #region ISupportsPersistentChannel
        
        void ISupportsPersistentChannel.Close ()
        {
            CloseChannel();
        }
        
        void ISupportsPersistentChannel.Open ()
        {
            OpenChannel();
        }
        #endregion

        #region Private Members
        
        private ServiceClientWrapper<T> m_channel;
        #endregion
    }
}
