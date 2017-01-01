using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Idioms;

namespace Flip
{
    [TestClass]
    public class CompositeStreamFilter_features
    {
        public class FakeModel : ModelBase<Guid>
        {
            public FakeModel(Guid id)
                : base(id)
            {
            }
        }

        private IFixture fixture;

        public CompositeStreamFilter_features()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [TestMethod]
        public void sut_implements_IStreamFilter()
        {
            var sut = new CompositeStreamFilter<FakeModel>();
            sut.Should().BeAssignableTo<IStreamFilter<FakeModel>>();
        }

        [TestMethod]
        public void constructor_sets_filters_correctly()
        {
            // Arrange
            var filters = new List<IStreamFilter<FakeModel>>(
                from _ in Enumerable.Range(0, 3)
                select Mock.Of<IStreamFilter<FakeModel>>());

            // Act
            var sut = new CompositeStreamFilter<FakeModel>(filters);
            IEnumerable<IStreamFilter<FakeModel>> actual = sut.Filters;

            // Assert
            actual.ShouldAllBeEquivalentTo(
                filters, opts => opts.WithStrictOrdering());
        }

        [TestMethod]
        public void constructors_have_guard_clauses()
        {
            var assertion = new GuardClauseAssertion(fixture);
            assertion.Verify(typeof(CompositeStreamFilter<>).GetConstructors());
        }

        [TestMethod]
        public void constructor_fails_if_filters_contains_null()
        {
            var filters = new IStreamFilter<FakeModel>[]
            {
                Mock.Of<IStreamFilter<FakeModel>>(),
                null,
                Mock.Of<IStreamFilter<FakeModel>>()
            };

            Action action = () => new CompositeStreamFilter<FakeModel>(filters);

            action.ShouldThrow<ArgumentException>()
                .Where(x => x.ParamName == "filters");
        }

        [TestMethod]
        public void Execute_has_guard_clauses()
        {
            var assertion = new GuardClauseAssertion(fixture);
            assertion.Verify(typeof(CompositeStreamFilter<>).GetMethod("Execute"));
        }

        [TestMethod]
        public void Execute_returns_new_value_if_no_filter()
        {
            var sut = new CompositeStreamFilter<FakeModel>();
            var newValue = fixture.Create<FakeModel>();
            var lastValue = fixture.Create<FakeModel>();

            FakeModel actual = sut.Execute(newValue, lastValue);

            actual.Should().BeSameAs(newValue);
        }

        [TestMethod]
        public void Execute_relays_arguments_to_first_filter()
        {
            var filter = Mock.Of<IStreamFilter<FakeModel>>();
            var sut = new CompositeStreamFilter<FakeModel>(filter);
            var newValue = fixture.Create<FakeModel>();
            var lastValue = fixture.Create<FakeModel>();

            sut.Execute(newValue, lastValue);

            Mock.Get(filter).Verify(
                 x => x.Execute(newValue, lastValue), Times.Once());
        }

        [TestMethod]
        public void Execute_does_not_execue_filter_if_previous_filter_returns_null()
        {
            // Arrange
            var newValue = fixture.Create<FakeModel>();
            var lastValue = fixture.Create<FakeModel>();
            FakeModel nullModel = null;

            var firstFilter = Mock.Of<IStreamFilter<FakeModel>>(
                x => x.Execute(newValue, lastValue) == nullModel);

            var sut = new CompositeStreamFilter<FakeModel>(
                firstFilter, Mock.Of<IStreamFilter<FakeModel>>());

            // Act
            sut.Execute(newValue, lastValue);

            // Assert
            Mock.Get(sut.Filters.Last()).Verify(
                x =>
                x.Execute(It.IsAny<FakeModel>(), It.IsAny<FakeModel>()),
                Times.Never());
        }

        [TestMethod]
        public void Execute_passes_filter_result_to_next_filter_as_newValue()
        {
            // Arrange
            var newValue = fixture.Create<FakeModel>();
            var lastValue = fixture.Create<FakeModel>();
            var filterResult = fixture.Create<FakeModel>();

            var firstFilter = Mock.Of<IStreamFilter<FakeModel>>(
                x => x.Execute(newValue, lastValue) == filterResult);

            var sut = new CompositeStreamFilter<FakeModel>(
                firstFilter, Mock.Of<IStreamFilter<FakeModel>>());

            // Act
            sut.Execute(newValue, lastValue);

            // Assert
            Mock.Get(sut.Filters.Last()).Verify(
                 x => x.Execute(filterResult, lastValue), Times.Once());
        }

        [TestMethod]
        public void Execute_returns_result_of_last_filter()
        {
            // Arrange
            var newValue = fixture.Create<FakeModel>();
            var lastValue = fixture.Create<FakeModel>();

            var firstResult = fixture.Create<FakeModel>();
            var firstFilter = Mock.Of<IStreamFilter<FakeModel>>(
                x => x.Execute(newValue, lastValue) == firstResult);

            var secondResult = fixture.Create<FakeModel>();
            var secondFilter = Mock.Of<IStreamFilter<FakeModel>>(
                x => x.Execute(firstResult, lastValue) == secondResult);

            var sut = new CompositeStreamFilter<FakeModel>(
                firstFilter, secondFilter);

            // Act
            FakeModel actual = sut.Execute(newValue, lastValue);

            // Assert
            actual.Should().BeSameAs(secondResult);
        }
    }
}
