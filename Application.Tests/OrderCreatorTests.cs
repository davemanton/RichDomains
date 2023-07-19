using Application.Tests.Infrastructure;
using Client.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests
{
    public class OrderCreatorTests
    {
        IServiceProvider _serviceProvider;

        private CreateOrderRequestDto _request;

        public OrderCreatorTests()
        {
            // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
            _serviceProvider = TestDependencyResolver.Resolve();

            // happy path request
            _request = new CreateOrderRequestDto
            {
                FirstName = "FIRSTNAME",
                LastName ="LASTNAME",
                Address = "ADDRESS"
            };
        }

        public ICreateOrders GetContractUnderTest()
        {
            return _serviceProvider.GetRequiredService<ICreateOrders>();
        }

        [Fact]
        public void Create_ReturnsCustomerDetails_InDto()
        {
            var contractUnderTest = GetContractUnderTest();

            var responseDto = contractUnderTest.Create(_request);

            Assert.Equal(_request.FirstName, responseDto.FirstName);
            Assert.Equal(_request.LastName, responseDto.LastName);
            Assert.Equal(_request.Address, responseDto.Address);
        }
    }
}