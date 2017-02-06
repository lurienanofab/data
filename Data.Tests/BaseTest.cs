using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF;
using LNF.Repository;

namespace Data.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        IUnitOfWork _uow;

        [TestInitialize]
        public void Setup()
        {
            _uow = Providers.DataAccess.StartUnitOfWork();
        }

        [TestCleanup]
        public void Complete()
        {
            _uow.Dispose();
        }
    }
}
