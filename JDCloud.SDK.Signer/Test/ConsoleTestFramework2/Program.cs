using JDCloudSDK.Core.Auth;
using JDCloudSDK.Core.Common;
using JDCloudSDK.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text; 

namespace ConsoleTestFramework2
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string aKey = System.Environment.GetEnvironmentVariable("JDCLOUD_ACCESS_KEY");
            string sKey = System.Environment.GetEnvironmentVariable("JDCLOUD_SECRET_ACCESS_KEY");
           
            var credentials = new Credentials(aKey, sKey);
            string url = "http://w6p38c2w0fwy.cn-east-2.jdcloud-api.net:8000/todo/api/v1/tasks/getAllOrUniqueTask?title=luan_ma_bu_xing";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.DoSign(credentials, "xohk7ybhwien").GetResponse();

                Console.WriteLine(httpWebResponse.StatusCode);
            }
            catch (WebException webException) {
                using (HttpWebResponse exceptionResponce = (HttpWebResponse)webException.Response)
                {
                    if (exceptionResponce != null)
                    {
                       
                        using (StreamReader streamReader = new StreamReader(exceptionResponce.GetResponseStream()))
                        {
                            string responseContent = streamReader.ReadToEnd();
                            Console.WriteLine(responseContent);
                        }
                    }
                }
            }
            string postUrl = "http://w6p38c2w0fwy.cn-east-2.jdcloud-api.net:8000/todo/api/v1/tasks/createTask/json";
            HttpWebRequest postHttpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            postHttpWebRequest.Method = "POST";
            postHttpWebRequest.ContentType = "application/json";
           // postHttpWebRequest.Headers.Add(ParameterConstant.X_JDCLOUD_NONCE, "c31604bd-37e3-4297-98ad-5973b7e388a7");
            try
            {
                var outDate = new DateTime();

                if (!DateTime.TryParse("2020-01-12 18:44:52", out outDate)) {
                    outDate = new DateTime();
                } 
                HttpWebResponse httpWebResponse2 = (HttpWebResponse)postHttpWebRequest.DoSign(credentials,
                    "w6p38c2w0fwy", "{\"description\": \"1234567890\", \"title\": \"luan_ma_bu_xing\"}",
                    true).GetResponse();

                Console.WriteLine(httpWebResponse2.StatusCode);
                if (httpWebResponse2 != null)
                {
                    using (StreamReader streamReader = new StreamReader(httpWebResponse2.GetResponseStream()))
                    {
                        string responseContent = streamReader.ReadToEnd();
                        Console.WriteLine(responseContent);
                    }
                }
            }
            catch (WebException webException)
            {
                using (HttpWebResponse exceptionResponce = (HttpWebResponse)webException.Response)
                {
                    if (exceptionResponce != null)
                    { 
                        using (StreamReader streamReader = new StreamReader(exceptionResponce.GetResponseStream()))
                        {
                            string responseContent = streamReader.ReadToEnd();
                            Console.WriteLine(responseContent);
                        }
                    }
                }
            }


                Console.ReadLine();
        }
    }
}
