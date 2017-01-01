using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flip
{
    [TestClass]
    public class ModelBase_features
    {
        public class FakeModel : ModelBase<Guid>
        {
            public FakeModel(Guid id)
                : base(id)
            {
            }
        }

        [TestMethod]
        public void sut_implements_IModel()
        {
            var sut = new FakeModel(Guid.NewGuid());
            sut.Should().BeAssignableTo<IModel<Guid>>();
        }
    }
}
