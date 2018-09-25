﻿//------------------------------------------------------------------------------
// <copyright file="BrokerQueueEventArgs.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>define the event class for broker queue.</summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// the event args class.
    /// </summary>
    internal class BrokerQueueEventArgs : EventArgs
    {
        /// <summary>
        /// the broker queue.
        /// </summary>
        private BrokerQueue queueField;

        /// <summary>
        /// Initializes a new instance of the BrokerQueueEventArgs class.
        /// </summary>
        /// <param name="queue">the broker queue.</param>
        public BrokerQueueEventArgs(BrokerQueue queue)
        {
            this.queueField = queue;
        }

        /// <summary>
        /// Gets the broker queue.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public BrokerQueue BrokerQueue
        {
            get
            {
                return this.queueField;
            }
        }
    }
}