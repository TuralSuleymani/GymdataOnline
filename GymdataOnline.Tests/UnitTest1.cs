using AccreditationMS.Areas.Admin.Controllers;
using AccreditationMS.Areas.Admin.Models;
using AccreditationMS.Core.Repositories.Interfaces;
using AccreditationMS.Core.UnitOfWork;
using AccreditationMS.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using AccreditationMS.Infrastructure.Helpers;

namespace AccreditationMS.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Add_returnsCurrencyAsync()
        {
            //Arrange
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<IUnitOfWork> mock = mockRepository.Create<IUnitOfWork>();
            Mock<ICurrencyRepository> cur = mockRepository.Create<ICurrencyRepository>();


            cur.Setup(x => x.GetAllAsync())
                .Returns
                  (Task.FromResult<IEnumerable<Currency>>
                        (new List<Currency>
                            {
                                   new Currency
                                   {
                                       Id=1,
                                       Unit="AZN"
                                   },
                                   new Currency
                                   {
                                       Id=2,
                                       Unit="USD"
                                   }
                            }
                        )
                  );

            EventController eventController = new EventController(mock.Object);
            //Act
            List<Currency> currencies =
                  (((await eventController.Add()) as ViewResult).Model as EventModel).Currencies.ToList<Currency>();

            //Assert
            Assert.Equal(expected: 2, actual: currencies.Count());
            Assert.Equal(expected: "AZN", actual: currencies[0].Unit);


        }

        [Fact]
        public void GenerateInvoice_LastInvoice_GetCorrectSevenDigit()
        {
            //Arrange
            Mock<IInvoiceGenerator> invoiceGenerator = new Mock<IInvoiceGenerator>();
            invoiceGenerator.Setup(x => x.GenerateInvoice()).Returns("4567");
            //Act
            string invoice = invoiceGenerator.Object.GenerateInvoice();
            //Assert
            Assert.Equal("0004567", invoice);
        }

    }
}
