using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF;
using LNF.Repository;
using LNF.Impl;
using LNF.Impl.DependencyInjection;
using LNF.DependencyInjection;
using LNF.DataAccess;

namespace Data.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        protected IContainerContext Context { get; private set; }
        protected IProvider Provider => Context.GetInstance<IProvider>();
        protected ISession DataSession => Provider.DataAccess.Session;

        private IUnitOfWork _uow;

        [TestInitialize]
        public void Setup()
        {
            ContainerContextFactory.Current.NewThreadScopedContext();
            Context = ContainerContextFactory.Current.GetContext();
            var cfg = new ThreadStaticContainerConfiguration(Context);
            cfg.RegisterAllTypes();
            ServiceProvider.Setup(Provider);
            _uow = Provider.DataAccess.StartUnitOfWork();
        }

        [TestCleanup]
        public void Complete()
        {
            _uow.Dispose();
        }
    }
}
