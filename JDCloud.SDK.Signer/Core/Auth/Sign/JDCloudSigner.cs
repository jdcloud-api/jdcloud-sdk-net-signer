﻿using JDCloudSDK.Core.Model; 
using JDCloudSDK.Core.Common;
using JDCloudSDK.Core.Utils;
using System;
using System.Collections.Generic; 
using System.Text;
#if !NET20&&!NET30
using System.Linq;
#endif
using System.Globalization;

namespace JDCloudSDK.Core.Auth.Sign
{
    /// <summary>
    /// 京东云网关签名类
    /// </summary>
    public class JDCloudSigner: IJDCloudSigner
    {
        /// <summary>
        /// 要进行忽略的签名头信息
        /// </summary>
        private static readonly string[] LIST_OF_HEADERS_TO_IGNORE_IN_LOWER_CASE = { "connection","x-jdcloud-trace-id"};
          

        /// <summary>
        /// URL 进行二次加密
        /// </summary>
        public bool DoubleUrlEncode { get; set; }=false;
        
        /// <summary>
        /// 签名注释
        /// </summary>
        public JDCloudSigner()
        {
        }

        /// <summary>
        /// 进行二次url endcode 加密的签名
        /// </summary>
        /// <param name="doubleUrlEncode"></param>
        public JDCloudSigner(bool doubleUrlEncode)
        {
             DoubleUrlEncode = doubleUrlEncode;
        }

        /// <summary>
        /// 进行请求签名
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public SignedRequestModel Sign(RequestModel requestModel, Credential credentials) {
            string nonceId = "";
            if (!requestModel.NonceId.IsNullOrWhiteSpace())
            {
                nonceId = requestModel.NonceId;
            }
            else if (requestModel.Header != null &&
                 requestModel.Header.Count > 0 &&
                 requestModel.Header.ContainsKey(ParameterConstant.X_JDCLOUD_NONCE))
            {
                List<string> headValues = requestModel.Header[ParameterConstant.X_JDCLOUD_NONCE];
                if (headValues != null && headValues.Count > 0)
                {
                    nonceId = headValues[0];
                }
                else
                {
                    nonceId = Guid.NewGuid().ToString().ToLower();
                }
            }
            else
            {
                nonceId = Guid.NewGuid().ToString().ToLower();
            }

            var signDate = requestModel.OverrddenDate == null ? DateTime.Now:requestModel.OverrddenDate.Value;
            string formattedSigningDateTime = signDate.ToString(ParameterConstant.DATA_TIME_FORMAT);
            string formattedSigningDate = signDate.ToString(ParameterConstant.HEADER_DATA_FORMAT);
            string scope = SignUtil.GenerateScope(formattedSigningDate, requestModel.ServiceName, requestModel.RegionName,ParameterConstant.JDCLOUD_TERMINATOR);
            var requestHeader = requestModel.Header;
            requestHeader.Add(ParameterConstant.X_JDCLOUD_DATE,
                              new List<string> { formattedSigningDateTime } );
            if (!requestModel.Header.ContainsKey(ParameterConstant.X_JDCLOUD_NONCE)) {
                requestHeader.Add(ParameterConstant.X_JDCLOUD_NONCE,
                                new List<string> { nonceId });
            }
          
            var contentSHA256 = "";
            if (requestHeader.ContainsKey(ParameterConstant.X_JDCLOUD_CONTENT_SHA256))
            {
                List<string> contentSha256Value = requestHeader[ParameterConstant.X_JDCLOUD_CONTENT_SHA256];
                if (contentSha256Value != null && contentSha256Value.Count > 0)
                {
                    contentSHA256 = contentSha256Value[0];
                } 
            }
            if (contentSHA256.IsNullOrWhiteSpace())
            {
                contentSHA256 = SignUtil.CalculateContentHash(requestModel.Content);
            }
            var requestParameters = OrderRequestParameters(requestModel.QueryParameters);
            string path = "";
            StringBuilder stringBuilder = new StringBuilder();
            if (!requestModel.ResourcePath.TrimStart().StartsWith("/")) {
                stringBuilder.Append("/");
            }
            stringBuilder.Append(requestModel.ResourcePath);
            path = stringBuilder.ToString();
            var canonicalRequest = SignUtil.CreateCanonicalRequest(requestParameters,
                GetCanonicalizedResourcePath(path,false),
                requestModel.HttpMethod.ToUpper()
                ,GetCanonicalizedHeaderString(requestModel),
                GetSignedHeadersString(requestModel), contentSHA256);
            var stringToSign = SignUtil.CreateStringToSign(canonicalRequest, formattedSigningDateTime, scope, ParameterConstant.JDCLOUD2_SIGNING_ALGORITHM);

            byte[] kSecret = System.Text.Encoding.UTF8.GetBytes($"JDCLOUD2{credentials.SecretAccessKey}");
            byte[] kDate =SignUtil.Sign(formattedSigningDate, kSecret, ParameterConstant.SIGN_SHA256);
            byte[] kRegion = SignUtil.Sign(requestModel.RegionName, kDate, ParameterConstant.SIGN_SHA256);
            byte[] kService = SignUtil.Sign(requestModel.ServiceName, kRegion, ParameterConstant.SIGN_SHA256);
            byte[] signingKey = SignUtil.Sign(ParameterConstant.JDCLOUD_TERMINATOR, kService, ParameterConstant.SIGN_SHA256);
            byte[] signature = SignUtil.ComputeSignature(stringToSign, signingKey);
           // Console.WriteLine($" kSecret={ BitConverter.ToString(kSecret).Replace("-", "")}");
           // Console.WriteLine($" kDate={ BitConverter.ToString(kDate).Replace("-", "")}");
           // Console.WriteLine($" kRegion={ BitConverter.ToString(kRegion).Replace("-", "")}");
           // Console.WriteLine($" kService={ BitConverter.ToString(kService).Replace("-", "")}");
           // Console.WriteLine($" signingKey={ BitConverter.ToString(signingKey).Replace("-", "")}");
           // Console.WriteLine($" signature={ BitConverter.ToString(signature).Replace("-", "")}");

            string signingCredentials = credentials.AccessKeyId + "/" + scope;
            string credential = "Credential=" + signingCredentials;
            string signerHeaders = "SignedHeaders=" + GetSignedHeadersString(requestModel);
            string signatureHeader = "Signature=" + StringUtils.ByteToHex(signature, true);

            var signHeader = new StringBuilder().Append(ParameterConstant.JDCLOUD2_SIGNING_ALGORITHM)
                    .Append(" ")
                    .Append(credential)
                    .Append(", ")
                    .Append(signerHeaders)
                    .Append(", ")
                    .Append(signatureHeader)
                    .ToString();
           
            requestModel.AddHeader(ParameterConstant.AUTHORIZATION, signHeader);
            SignedRequestModel signedRequestModel = new SignedRequestModel();
            signedRequestModel.CanonicalRequest = canonicalRequest;
            signedRequestModel.ContentSHA256 = contentSHA256;
            foreach (var header in requestModel.Header) {
                signedRequestModel.RequestHead.Add(header.Key, string.Join(",", header.Value.ToArray()));
            }
            signedRequestModel.RequestNonceId = nonceId; 
            signedRequestModel.SignedHeaders = signHeader;
            signedRequestModel.StringSignature = stringToSign;
            signedRequestModel.StringToSign = stringToSign;
            
            return signedRequestModel;
        }

       

        




       
        /// <summary>
        /// order request parameters
        /// </summary>
        /// <param name="requestQueryParameters"></param>
        /// <returns></returns>
        private static string OrderRequestParameters(string requestQueryParameters) {
            if (requestQueryParameters.IsNullOrWhiteSpace()) {
                return string.Empty;
            }
            if (!requestQueryParameters.IsNullOrWhiteSpace())
            {
                if (requestQueryParameters.StartsWith("?"))
                {
                    requestQueryParameters = requestQueryParameters.Substring(1);
                }
            }
            Dictionary<string, string> paramDic = new Dictionary<string, string>();
            var paramArray = requestQueryParameters.Split('&');
            if (paramArray != null && paramArray.Length > 0) {
                foreach (var paramKeyValue in paramArray) {
                    var keyValue = paramKeyValue.Split('=');
                    if (keyValue != null && keyValue.Length > 0) {
                        if (keyValue.Length == 1)
                        {
                            paramDic.Add(keyValue[0], string.Empty);
                        }
                        else if (keyValue.Length == 2)
                        {
                            paramDic.Add(keyValue[0], keyValue[1]);
                        }
                        else {
                            StringBuilder stringBuilder = new StringBuilder();
                            for (int i = 1; i < keyValue.Length; i++) {
                                if (i == keyValue.Length - 1)
                                {
                                    stringBuilder.Append(keyValue[i]);
                                }
                                else {
                                    stringBuilder.Append(keyValue[i]).Append("=");
                                }
                            } 
                            paramDic.Add(keyValue[0], stringBuilder.ToString());
                        }
                    }
                }
            }
            if (paramDic != null && paramDic.Count > 0) {
                StringBuilder resultBuilder = new StringBuilder();
#if !NET20 && !NET30
                var orderParamDic = paramDic.OrderBy(p => p.Key);
#else


                var orderParamDic = new SortedDictionary<string, string>(); ;
                foreach (var item in paramDic)
                {
                    orderParamDic.Add(item.Key, item.Value);
                }
#endif
                foreach (var param in orderParamDic) {
                    resultBuilder.Append(param.Key).Append("=").Append(param.Value);
                    resultBuilder.Append("&");
                }
                return resultBuilder.ToString().TrimEnd('&');
            } 
            return string.Empty;
        }

        


        /// <summary>
        /// 生成签名头信息字符串
        /// </summary>
        /// <param name="requestModel">http 请求信息</param>
        /// <returns>签名头信息字符串</returns>
        private string GetSignedHeadersString(RequestModel requestModel)
        {
            var headers = requestModel.Header; 
#if !NET20 && !NET30
            var keys = headers.Keys;
            List<string> keysList = keys.ToList().OrderBy(p => p.ToLower(CultureInfo.GetCultureInfo("en-US"))).ToList();
#else
            List<string> keysList = new List<string>();
            foreach (var item in headers.Keys)
            {
                keysList.Add(item);
            }
            keysList.Sort();
#endif
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string header in keysList)
            {
                if (ShouldExcludeHeaderFromSigning(header))
                {
                    continue;
                }
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(";");
                }
                stringBuilder.Append(header.ToLower(CultureInfo.GetCultureInfo("en-US")));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取规范化的头信息
        /// </summary>
        /// <param name="requestModel">签名请求信息</param>
        /// <returns>规范化头信息字符串</returns>
        private string GetCanonicalizedHeaderString(RequestModel requestModel)
        {
            var headers = requestModel.Header; 
#if !NET20 && !NET30
            var keys = headers.Keys;
            List<string> keysList = keys.ToList().OrderBy(p => p.ToLower(CultureInfo.GetCultureInfo("en-US"))).ToList();
#else
            List<string> keysList = new List<string>();
            foreach (var item in headers.Keys)
            {
                keysList.Add(item);
            }
            keysList.Sort();
#endif
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in keysList)
            {
                if (ShouldExcludeHeaderFromSigning(item))
                {
                    continue;
                }
                string key = item.ToLower(CultureInfo.GetCultureInfo("en-US"));

                foreach (var headerValue in headers[item])
                {
                    string headerCompactedValue = StringUtils.AppendCompactedString(key);
                    stringBuilder.Append(key).Append(":");
                    if (headerValue != null)
                    {
                        string headerValueCompactedValue = StringUtils.AppendCompactedString(headerValue);
                        stringBuilder.Append(headerValueCompactedValue);
                    }
                    stringBuilder.Append("\n");
                }
            }


            return stringBuilder.ToString();
        }

        /// <summary>
        /// 是否属于被排除的报文头
        /// </summary>
        /// <param name="header">头信息字符串</param>
        /// <returns>是否是需要排除的头信息</returns>
        private bool ShouldExcludeHeaderFromSigning(string header)
        {
#if !NET20 && !NET30
            return LIST_OF_HEADERS_TO_IGNORE_IN_LOWER_CASE.Contains(header.ToLower());
#else
            var result = false;
            foreach (var item in LIST_OF_HEADERS_TO_IGNORE_IN_LOWER_CASE)
            {
                if (header.ToLower() == item.ToLower())
                {
                    result = true;

                }
            }
            return result;
#endif 
        }


        /// <summary>
        /// 进行请求path 转换
        /// </summary>
        /// <param name="path">请求path</param>
        /// <param name="doubleUrlEncode">是否进行二次编码</param>
        /// <returns>转换后的path</returns>
        protected string GetCanonicalizedResourcePath(string path, bool doubleUrlEncode)
        {
 
            if (path.IsNullOrWhiteSpace()) 
            {
                return "/";
            }
            else
            {
                string value = doubleUrlEncode ?Uri.EscapeDataString(path).Replace("%2F","/"): path;
                if (value.StartsWith("/"))
                {
                    return value;
                }
                else
                {
                    return $"/{value}";
                }
            }
        }

        



       


    

        /// <summary>
        /// 添加时间请求头信息
        /// </summary>
        /// <param name="requestModel">请求信息</param>
        /// <returns></returns>
        private RequestModel AddRequestDateHeader(RequestModel requestModel)
        {
            string formatDateTime = requestModel.OverrddenDate == null || !requestModel.OverrddenDate.HasValue ?
                DateTime.UtcNow.ToString(ParameterConstant.DATA_TIME_FORMAT) : requestModel.OverrddenDate.Value.ToString(ParameterConstant.DATA_TIME_FORMAT);
            requestModel.AddHeader(ParameterConstant.X_JDCLOUD_DATE, formatDateTime); 
            return requestModel;
        }

        /// <summary>
        /// 添加ContentType头信息
        /// </summary>
        /// <param name="requestModel">请求信息</param>
        /// <returns></returns>
        private RequestModel AddContentTypeHeader(RequestModel requestModel)
        {
            if (requestModel.ContentType.IsNullOrWhiteSpace())
            {
                requestModel.ContentType = ParameterConstant.MIME_JSON;
            }
            requestModel.AddHeader(ParameterConstant.CONTENT_TYPE, requestModel.ContentType);

            return requestModel;
        }

        /// <summary>
        /// 添加host 请求信息
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        private RequestModel AddHostHeader(RequestModel requestModel)
        {
            string url = requestModel.Uri.Host;
            if (url.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("the request url is not set,please check the client url config");
            }
            if ( requestModel.Uri.IsDefaultPort)
            {
                requestModel.AddHeader(ParameterConstant.HOST, requestModel.Uri.Host);
                
            }else
            {
                StringBuilder signHostBuilder = new StringBuilder();
                signHostBuilder.Append(requestModel.Uri.Host).Append(":").Append(requestModel.Uri.Port);
                requestModel.AddHeader(ParameterConstant.HOST, signHostBuilder.ToString());
            }

            return requestModel;
        }

         
    }

}
