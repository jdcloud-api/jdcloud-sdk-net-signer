﻿using JDCloudSDK.Core.Auth;
using JDCloudSDK.Core.Extensions;
using NUnit.Framework;
using System;
using System.Net;

namespace NetFrameworkTest
{ 
    public class UnitTest1
    { 
        [Test]
        public void TestMethod1()
        {
            string aKey = System.Environment.GetEnvironmentVariable("JDCLOUD_ACCESS_KEY");
            string sKey = System.Environment.GetEnvironmentVariable("JDCLOUD_SECRET_ACCESS_KEY");
            var credentials = new Credentials(aKey, sKey);
            string url = "http://xohk7ybhwien.cn-north-1.jdcloud-api.net:8000/todo/api/v1/tasks/getAllOrUniqueTask";
            HttpWebRequest httpWebRequest= (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "get";
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.DoSign(credentials, "xohk7ybhwien").GetResponse();


            Assert.AreEqual(HttpStatusCode.OK, httpWebResponse.StatusCode);
        }
    }
}
