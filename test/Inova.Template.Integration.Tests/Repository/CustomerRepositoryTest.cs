using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Inova.Template.Core.Tests.Mocks;
using Inova.Template.Core.Tests.Mocks.Factory;
using Inova.Template.Infra.Context;
using Inova.Template.Infra.Repository;
using Inova.Template.Infra.UoW;
using Xunit;

namespace Inova.Template.Integration.Tests.Repository
{
    public class CustomerRepositoryTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly DbContextOptions<EntityContext> _entityOptions;

        public CustomerRepositoryTest()
        {
            _configurationMock = new Mock<IConfiguration>();
            _entityOptions = new DbContextOptionsBuilder<EntityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Crud_EntityTest()
        {
            var customer = CustomerMock.CustomerModelFaker.Generate();

            _configurationMock.Setup(x => x.GetSection(It.IsAny<string>()))
                .Returns(new Mock<IConfigurationSection>().Object);

            var entityContext = new EntityContext(_entityOptions);
            var unitOfWork = new UnitOfWork(entityContext);
            var dapperContext = new DapperContext(MockRepositoryBuilder.GetMockDbConnection().Object);
            var customerRepository = new CustomerRepository(entityContext, dapperContext);

            customerRepository.Add(customer);
            var IsSaveCustomer = unitOfWork.Commit();

            customerRepository.Update(customer);
            var IsUpdateCustomer = unitOfWork.Commit();

            customerRepository.Remove(customer);
            var IsRemoverCustomer = unitOfWork.Commit();

            Assert.Equal(1, IsSaveCustomer);
            Assert.Equal(1, IsUpdateCustomer);
            Assert.Equal(1, IsRemoverCustomer);
        }
    }
}
