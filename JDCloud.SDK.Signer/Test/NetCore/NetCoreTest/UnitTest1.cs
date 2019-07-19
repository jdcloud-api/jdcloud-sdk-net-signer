using JDCloudSDK.Core.Auth;
using JDCloudSDK.Core.Extensions;
using System;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace NetCoreTest
{
    public class UnitTest1
    {

        private readonly ITestOutputHelper output;
        public UnitTest1(ITestOutputHelper tempOutput)
        {
            output = tempOutput;// new TestOutputHelper();
        }
        [Fact]
        public void Test1()
        {
            HttpClient httpClient = new HttpClient();
            string aKey = System.Environment.GetEnvironmentVariable("JDCLOUD_ACCESS_KEY");
            string sKey = System.Environment.GetEnvironmentVariable("JDCLOUD_SECRET_ACCESS_KEY");
            var credentials= new Credentials(aKey, sKey);
            HttpResponseMessage httpResponseMessage = httpClient.DoSign(credentials, "xohk7ybhwien")
                                                                .GetAsync("http://xohk7ybhwien.cn-north-1.jdcloud-api.net:8000/todo/api/v1/tasks/getAllOrUniqueTask").Result;
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                output.WriteLine(result);
            }

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);

        }
    }
}
