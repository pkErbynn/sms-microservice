using Microsoft.Extensions.Configuration;
using Moq;
using SmsMicroservice.Models;
using SmsMicroservice.SyncDataService;

namespace SmsMicroservice.Tests.SyncDataService.Tests
{
    public class HttpSmsDataClientTests
    {
        [Fact]
        public async Task SendSmsToThirdPartyAsync_FailedRequest_RetriesAndReturnsFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var configMock = new Mock<IConfiguration>();
            var httpSmsDataClient = new HttpSmsDataClient(httpClient, configMock.Object);
            var nullSmsMessage = ((SmsTextMessage)null);

            // Act
            var result = await httpSmsDataClient.SendSmsToThirdPartyAsync(nullSmsMessage);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendSmsToThirdPartyAsync_ApiDown_RetriesAndReturnsFalse()
        {
            // Arrange
            var apiUrl = "https://localhost:7194/api/Sms";
            var httpClient = new HttpClient();
            var configMock = Mock.Of<IConfiguration>(x =>
                x["ThirdPartyAPI:Url"] == apiUrl &&
                x["ThirdPartyAPI:ApiKey"] == "test-api-key" &&
                x["ThirdPartyAPI:DelayBetweenRetriesMilliseconds"] == "1000" &&
                x["ThirdPartyAPI:MaxRetryAttempts"] == "2"
            );
            var _httpSmsDataClient = new HttpSmsDataClient(httpClient, configMock);

            var smsTextMessage = new SmsTextMessage
            {
                PhoneNumber = "1234",
                SmsText = "Test message"
            };

            var smsService = new HttpSmsDataClient(httpClient, configMock);

            // Act
            var result = await smsService.SendSmsToThirdPartyAsync(smsTextMessage);

            // Assert
            Assert.False(result);
        }
      
    }
}
