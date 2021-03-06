﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    /// <summary>
    /// Define Session states
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// The session is initialized
        /// </summary>
        Creating,

        /// <summary>
        /// The session resource has been allocated
        /// </summary>
        Running,

        /// <summary>
        /// The session's requests have all been handled
        /// </summary>
        Completed,

        /// <summary>
        /// The session can't get the compute resource currently
        /// </summary>
        Queued,

        /// <summary>
        /// The session is failed to create or timeout to receive any request
        /// </summary>
        Failed,
        
        /// <summary>
        /// The session is finishing
        /// </summary>
        Closing,

        /// <summary>
        /// The session is finished and all the related resources have all been cleaned up
        /// </summary>
        Closed
    }
}
