using System;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Idioms;

namespace Flip
{
    [TestClass]
    public class StreamStore_features
    {
        public class FakeModel : IModel<string>
        {
            public string Id { get; set; }
        }

        private IFixture fixture;
        private IStreamFilter<FakeModel> filter;
        private StreamStore<string, FakeModel> sut;

        [TestInitialize]
        public void TestInitialize()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            filter = Mock.Of<IStreamFilter<FakeModel>>();
            Mock.Get(filter)
                .Setup(x => x.Execute(It.IsAny<FakeModel>(), It.IsAny<FakeModel>()))
                .Returns<FakeModel, FakeModel>((newValue, lastValue) => newValue);

            sut = new StreamStore<string, FakeModel>(filter);
        }

        [TestMethod]
        public void sut_implements_IStreamStore()
        {
            sut.Should().BeAssignableTo<IStreamStore<string, FakeModel>>();
        }

        [TestMethod]
        public void class_has_guard_clauses()
        {
            var assertion = new GuardClauseAssertion(fixture);
            assertion.Verify(typeof(StreamStore<string, FakeModel>));
        }

        [TestMethod]
        public void Connect_returns_connection()
        {
            string modelId = fixture.Create("modelId");
            IConnection<string, FakeModel> actual = sut.Connect(modelId);
            actual.Should().NotBeNull();
            actual.ModelId.Should().Be(modelId);
        }

        public interface IFunctor
        {
            void Action<T>(T arg);
        }

        [TestMethod]
        public void Connection_Emit_propagates_model_correctly()
        {
            // Arrange
            string modelId = fixture.Create("modelId");

            IConnection<FakeModel> connection1 = sut.Connect(modelId);
            var subscriber1 = Mock.Of<IFunctor>();
            connection1.Stream.Subscribe(subscriber1.Action);

            IConnection<FakeModel> connection2 = sut.Connect(modelId);
            var subscriber2 = Mock.Of<IFunctor>();
            connection2.Stream.Subscribe(subscriber2.Action);

            var model = new FakeModel { Id = modelId };

            // Act
            connection1.Emit(Observable.Return(model));

            // Assert
            Mock.Get(subscriber1).Verify(x => x.Action(model), Times.Once());
            Mock.Get(subscriber2).Verify(x => x.Action(model), Times.Once());
        }

        [TestMethod]
        public void stream_uses_filter()
        {
            // Arrange
            string modelId = fixture.Create("modelId");

            var lastModel = new FakeModel { Id = modelId };

            IConnection<FakeModel> connection = sut.Connect(modelId);
            var subscriber = Mock.Of<IFunctor>();
            connection.Stream.Subscribe(subscriber.Action);
            connection.Emit(Observable.Return(lastModel));

            var newModel = new FakeModel { Id = modelId };
            var filtered = new FakeModel { Id = modelId };

            Mock.Get(filter)
                .Setup(x => x.Execute(newModel, lastModel))
                .Returns(filtered)
                .Verifiable();

            // Act
            connection.Emit(Observable.Return(newModel));

            // Assert
            Mock.Get(filter).Verify();
            Mock.Get(subscriber).Verify(x => x.Action(filtered), Times.Once());
        }
    }
}
