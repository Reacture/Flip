using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Idioms;

namespace Flip
{
    [TestClass]
    public class StreamExtensions_features
    {
        [TestMethod]
        public void class_has_guard_clauses()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var assertion = new GuardClauseAssertion(fixture);
            assertion.Verify(typeof(StreamExtensions));
        }

        public class FakeModel : ModelBase<Guid>
        {
            public FakeModel()
                : base(Guid.NewGuid())
            {
            }
        }

        public interface IFunctor
        {
            void Action<T>(T arg);
        }

        [TestMethod]
        public void Emit_relays_with_observable_with_model()
        {
            var model = new FakeModel();
            var store = new StreamStore<Guid, FakeModel>();
            var functor = Mock.Of<IFunctor>();
            store.Connect(model.Id).Stream.Subscribe(functor.Action);
            IConnection<Guid, FakeModel> connection = store.Connect(model.Id);

            connection.Emit(model);

            Mock.Get(functor).Verify(x => x.Action(model), Times.Once());
        }

        [TestMethod]
        public void Emit_relays_observable_with_future()
        {
            var model = new FakeModel();
            var store = new StreamStore<Guid, FakeModel>();
            var functor = Mock.Of<IFunctor>();
            store.Connect(model.Id).Stream.Subscribe(functor.Action);
            IConnection<Guid, FakeModel> connection = store.Connect(model.Id);

            connection.Emit(Task.FromResult(model));

            Mock.Get(functor).Verify(x => x.Action(model), Times.Once());
        }
    }
}
