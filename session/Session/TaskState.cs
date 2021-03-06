﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    /// <summary>
    /// Define Session states
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// The task is initialized
        /// </summary>
        Creating,

        /// <summary>
        /// The task is running
        /// </summary>
        Running,

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
        Finishing,

        /// <summary>
        /// The session is finished and all the related resources have all been cleaned up
        /// </summary>
        Finished,

        /// <summary>
        /// The session is canceling due to client's cancellation
        /// </summary>
        Canceling,

        /// <summary>
        /// The session is canceled and all the related resources have all been cleaned up
        /// </summary>
        Canceled
    }
}
