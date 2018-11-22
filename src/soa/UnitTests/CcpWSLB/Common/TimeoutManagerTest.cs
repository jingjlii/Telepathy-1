﻿//-----------------------------------------------------------------------------------
// <copyright file="TimeoutManagerTest.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Test class for TimeoutManager</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest
{
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    /// <summary>
    ///This is a test class for TimeoutManagerTest and is intended
    ///to contain all TimeoutManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TimeoutManagerTest
    {
        private TimeoutManager GetTimeoutManager() => new TimeoutManager("TEST");

        /// <summary>
        /// Stores the whether the callback called
        /// </summary>
        private bool called;

        /// <summary>
        /// Unit test for timeout
        /// </summary>
        [TestMethod]
        public void SimpleTimeoutTest()
        {
            called = false;
            this.GetTimeoutManager().RegisterTimeout(3000, CallbackMethod, null);
            Thread.Sleep(3100);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ResetTimeoutTest()
        {
            called = false;
            var manager = this.GetTimeoutManager();
            manager.RegisterTimeout(3000, CallbackMethod, null);
            Thread.Sleep(2500);
            manager.ResetTimeout();
            Thread.Sleep(600);
            Assert.IsFalse(called);
            Thread.Sleep(2600);
            Assert.IsTrue(called);
        }

        /// <summary>
        /// Method to receive callback
        /// </summary>
        /// <param name="state">async state object</param>
        private void CallbackMethod(object state)
        {
            called = true;
        }
    }
}
