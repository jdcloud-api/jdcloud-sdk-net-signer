# JDCloud Open Api DotNet Signer

[![Build status](https://ci.appveyor.com/api/projects/status/sy68ga8pvt9vi4vs/branch/master?svg=true)](https://ci.appveyor.com/project/lishjun01/jdcloud-sdk-net-signer/branch/master)

## 简介 

&emsp;&emsp;此包为京东云 dotnet SDK 的签名包，使用签名包可以通过open Api 或者 APIM 网关调用后端的服务。  
&emsp;&emsp;为了方便您理解SDK中的一些概念和参数的含义，使用SDK前建议您先查看OpenAPI使用入门。要了解每个API的具体参数和含义，请参考程序注释或参考[OpenAPI&SDK](https://www.jdcloud.com/help/faq?act=3)下具体产品线的API文档。

## 环境准备 & 编译

* 此项目使用visual studio 2019 开发，如果需要进行代码编辑、调试，推荐使用visual studio 2019 以上版本打开，没有尝试使用visual stuido 2017 打开，不过可以修改解决方案的sln版本号打开

* 本项目使用最新的 dotnet standard 多目标框架的方法进行编译，项目使用了 .NET 3.5 、 .NET 4.0 、 .NET 4.5  、 .NET 46 、.NET47 和 .net standard 2.0 版本进行编译，如果使用.NET 48 推荐引用.net standard 2.0 版本。在编译前需要安装 .NET Framework 3.5 、4.0 、4.5 、4.6 、4.7 的开发sdk和dotnet core 2.0 以上版本的sdk，在windows 10 操作系统下 .NET Framework 3.5 请在 `启用和关闭windows功能` 的控制面板勾选应用以后再安装visual studio 2017 和 .net framework 4.7 ，.dotnet core 的安装方法请查看[微软官网文档](https://www.microsoft.com/net/learn/get-started/windows)。其它目标框架请在安装visual studio时候勾选或微软官方网站下载。

* 因目前Http调用工具类使用`HttpClient`，如果使用`.Net Framework 4.5` 需要引用框架包`System.Net.Http`。因`HttpClient`不支持`.Net Framework 3.5`且对`.NetFramework 4.0` 的异步支持不是很完善，所以在项目中使用了`HttpWebRequest`进行了替换。

* 因项目使用了`Newtonsoft.Json` 作为Json 对象转换的工具包，因此也需要引用`Newtonsoft.Json`，请在使用的时候选择与您使用的框架对应的版本引用。

* 如果需要使用其他版本的SDK，请在项目中增加编译版本，同时修改编译判断条件 ，具体编译目标框架编译条件信息参见[微软官网文档（Target frameworks页面）](https://docs.microsoft.com/en-us/dotnet/standard/frameworks)。

* 在开始调用京东云open API之前，需提前在京东云用户中心账户管理下的AccessKey管理页面申请accesskey和secretKey密钥对（简称AK/SK）。AK/SK信息请妥善保管，如果遗失可能会造成非法用户使用此信息操作您在云上的资源，给你造成数据和财产损失。

## 签名工具使用方法

&emsp;&emsp;在您的项目中引用需要调用的相关模块的dll 与 此dll所依赖的相关类库文件。然后按照下方调用签名的Demo使用  

```csharp

    // .net framework 2.0 3.0 3.5 or 4.0 HttpWebRequest

    var credentials = new Credentials("accesskey", "secretKey");
    string url = "http://xohk7ybhwien.cn-north-1.jdcloud-api.net:8000/todo/api/v1/tasks/getAllOrUniqueTask";
    HttpWebRequest httpWebRequest= (HttpWebRequest)WebRequest.Create(url);
    httpWebRequest.Method = "get";
    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.DoSign(credentials,"xohk7ybhwien").GetResponse();

    // .net framework 4.5 and later

    HttpClient httpClient = new HttpClient();
    var credentials= new Credentials("accesskey", "secretKey");
    HttpResponseMessage httpResponseMessage = httpClient.DoSign(credentials, "xohk7ybhwien")
                                                                .GetAsync("http://xohk7ybhwien.cn-north-1.jdcloud-api.net:8000/todo/api/v1/tasks/getAllOrUniqueTask").Result;
    if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
    {
        var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
        Console.WriteLine(result);
    }

```

注意，如果使用京东云Dotnet Core Open API SDK 的 JDCloud.SDK.Core 1.1.0 以上版本 包含此签名功能，不需要单独使用此工具
