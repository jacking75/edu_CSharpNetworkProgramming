# LiteNetwork  
  
라이트 네트워크는 C#으로 제작되어 .NET Standard 2, .NET 5, .NET 6과 호환되는 간단하고 빠른 네트워킹 라이브러리이다. 주요 목표는 TCP/IP 프로토콜을 통해 기본 소켓 서버를 간단하게 생성하는 것이다.  
  
처음에는 게임 개발 네트워킹을 위해 개발되었지만 다른 용도로도 사용할 수 있다.  
    
[코드 분석 문서](https://docs.google.com/spreadsheets/d/e/2PACX-1vSv7-WSVUu7AJ0ZOgBVGLW1rZXRN4n4SFcUHLpsNfd331ZFRm5VrO1FEERG7Vg8Flw0WfdVcg8rhAX7/pubhtml) 흐름 위주로 정리  
    
## 시작하기
   
### 서버 만들기
LiteNetwork로 TCP 서버를 구축하는 방법에는 두 가지가 있습니다:
- 인스턴스 방식, LiteServer 인스턴스를 생성한 다음 수동으로 실행하는 방법
- 서비스 방식
    - 실제로 라이트네트워크는 서비스 컬렉션 객체에 대한 확장을 제공하며, .NET 제네릭 호스트 (ASP.NET Core, MAUI에서 사용)에 통합할 수 있다.

#### 공통 코드
먼저 서버에 연결된 사용자를 나타낼 사용자 클래스를 만들어야 한다. LiteServerUser 클래스를 구현하는 새 클래스를 생성하기만 하면 된다.    
```
using LiteNetwork.Server;

public class ClientUser : LiteServerUser
{
}
```  
  
이 클래스 내에서 클라이언트 프로그램이 보낸 수신 메시지를 처리할 수 있는 메서드는 HandleMessageAsync( ) 메서드이다. 또한 클라이언트가 서버에 연결하거나 연결이 끊어질 때 알림을 받을 수도 있다.  
```
using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Server;

public class TcpUser : LiteServerUser
{
    public override Task HandleMessageAsync(byte[] packetBuffer)
    {
        // Handle incoming messages using a BinaryReader or any other solution for reading a byte[].
    }

    protected override void OnConnected()
    {
        // When the client connects.
    }

    protected override void OnDisconnected()
    {
        // When the client disconnects.
    }
}
```  
   
   
서버 사용자가 준비되면, 이 타입의 사용자를 처리할 서버 자체를 생성할 수 있다. 다른 새 클래스를 생성하고, 여기서 T가 이전에 생성한 TcpUser인 LiteServer<T> 클래스를 구현한다.  
```
public class MyTcpServer : LiteServer<TcpUser>
{
    public MyTcpServer(LiteServerOptions options, IServiceProvider serviceProvider = null)
        : base(options, serviceProvider)
    {
    }
}
```  
  
서버에는 아래와 같이 서버의 수명을 제어할 수 있는 몇 가지 후크가 있다:  
방법	             설명
OnBeforeStart()	서버가 시작되기 전에 호출된다.
OnAfterStart()	서버가 시작된 후 호출된다.
OnBeforeStop()	서버가 중지되기 전에 호출된다.
OnAfterStop()	서버가 중지된 후 호출된다.
OnError(ILiteConnection, Exception)	지정된 ILiteConnection에 처리되지 않은 오류가 있을 때 호출된다.  
  
  
#### 인스턴스를 통해 서버 생성
이제 서버와 사용자 클래스가 빌드 되었으므로 이제 서버를 인스턴스화하고 Start( ) 메서드를 호출하여 서버를 시작할 수 있다.  
```
// Using top-level statement
using LiteNetwork.Server;
using System;

// Create the server configuration, to listen on "127.0.0.1" and port "4444"
var configuration = new LiteServerOptions()
{
    Host = "127.0.0.1",
    Port = 4444
};

// Create the server instance by givin the server options and start it.
using var server = new MyTcpServer(configuration);
server.Start();

// Just for the example, otherwise the console will just shutdown.
// Do not use in production environment.
Console.ReadKey(); 
```  
  
  
#### 서비스를 통해 서버 만들기
For this example, you will need to install the [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/ ) package from nuget in order to build a [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host ).  
```
// Using top-level statement
using Microsoft.Extensions.Hosting;
using System;

var host = new HostBuilder()
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
```  
 
그런 다음 호스트가 설정되어 실행 중이면 LiteNetwork.Hosting 네임스페이스에 있는 `ConfigureLiteNetwork()` 메서드를 사용하여 LiteServer 서비스를 구성할 수 있다:
```
// Using top-level statement
using LiteNetwork.Hosting;
using LiteNetwork.Server.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

var host = new HostBuilder()
    // Configures the LiteNetwork context.
    .ConfigureLiteNetwork((context, builder) =>
    {
        // Adds a LiteServer instance for the MyTcpServer class.
        builder.AddLiteServer<MyTcpServer>(options =>
        {
            // This configures the server's LiteServerOptions instance.
            options.Host = "127.0.0.1";
            options.Port = 4444;
        });
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
```  
   
이제 서버가 "127.0.0.1" 및 포트 "4444"에서 수신 대기 중이다. 또한 .NET 제너릭 호스트를 사용하고 있으므로 서버 및 클라이언트 클래스에 종속성 주입 기능도 제공한다. 따라서 services, configuration( 구성된 경우 IOptions<T> 등)을 주입할 수 있다.  
  
>>> 참고: 다른 매개 변수를 사용하여 `builder.AddLiteServer<>()` 메서드를 호출하여 단일 .NET 제너릭 호스트에 원하는 만큼의 서버를 추가할 수도 있다.  
  
  
### 클라이언트 만들기  
LiteNetwork로 TCP 클라이언트를 구축하는 방법은 두 가지가 있다:  
- 인스턴스 방식: LiteClient 인스턴스를 생성한 다음 원격 서버에 수동으로 연결한다.
- 서비스 방식
    - 실제로 라이트네트워크는 서비스 컬렉션 객체에 대한 확장을 제공하며, .NET 제네릭 호스트 (ASP.NET Core, MAUI에서 사용)에 통합할 수 있다.  

#### 공통 코드
우선, LiteClient 클래스를 상속하는 새 클래스를 만들어야 한다.  
```
using LiteNetwork.Client;

public class MyTcpClient : LiteClient
{
    public EchoClient(LiteClientOptions options, IServiceProvider serviceProvider = null) 
        : base(options, serviceProvider)
    {
    }
}
```  

라이트 네트워크 서버와 마찬가지로 클라이언트에는 아래와 같이 클라이언트 수명을 제어할 수 있는 몇 가지 후크가 있다:  
  
방법							설명
HandleMessageAsync()	클라이언트가 서버로부터 메시지를 수신할 때 호출된다.
OnConnected()				클라이언트가 원격 서버에 연결되면 호출된다.
OnDisconnected()			클라이언트가 원격 서버에서 연결이 끊어졌을 때 호출된다.
OnError(예외)				클라이언트 프로세스 내에 처리되지 않은 오류가 있을 때 호출된다.
  
  
#### 인스턴스를 통해 클라이언트 만들기
이제 이전에 생성한 클라이언트를 사용하여 MyTcpClient 클래스의 새 인스턴스를 생성하고 원격 서버에 연결하기 위한 올바른 옵션을 설정한 다음 ConnectAsync( ) 메서드를 호출할 수 있다.  
```
// Using top-level statement
using LiteNetwork.Client;
using System;

var options = new LiteClientOptions()
{
    Host = "127.0.0.1",
    Port = 4444
};
var client = new CustomClient(options);
Console.WriteLine("Press any key to connect to server.");
Console.ReadKey();

await client.ConnectAsync();

// Do something while client is connected.
```  
  
  
#### 서비스를 통해 클라이언트 만들기
이 예제에서는 .NET 일반 호스트를 빌드하기 위해 nuget에서 Microsoft.Extensions.Hosting 패키지를 설치해야 한다.  
```
// Using top-level statement
using LiteNetwork.Client.Hosting;
using LiteNetwork.Hosting;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureLiteNetwork((context, builder) =>
    {
        builder.AddLiteClient<MyTcpClient>(options =>
        {
            options.Host = "127.0.0.1";
            options.Port = 4444;
        });
    })
    .UseConsoleLifetime()
    .Build();

// At this point, the client will connect automatically once the host starts running.
await host.RunAsync(); 
```  
  
프로그램이 시작되면 MyTcpClient는 원격 서버("127.0.0.1" 및 포트 4444)에 연결을 시도한다. 또한 .NET 제너릭 호스트를 사용하기 때문에 클라이언트에 종속성 주입 메커니즘도 제공한다. 따라서 services, configuration(구성된 경우 IOptions<T> ), 로거 등을 주입할 수 있다.  
  
>>> 참고: 다른 매개 변수를 사용하여 `builder.AddLiteClient<>()` 메서드를 호출하여 단일 .NET 일반 호스트에 원하는 만큼의 클라이언트를 추가할 수도 있다. 
  
<br>  

## LiteServerOptions  
```
public class LiteServerOptions
{
    /// <summary>
    /// Gets the default maximum of connections in accept queue.
    /// </summary>
    public const int DefaultBacklog = 50;

    /// <summary>
    /// Gets the default client buffer allocated size.
    /// </summary>
    public const int DefaultClientBufferSize = 128;

    /// <summary>
    /// Gets or sets the server's listening host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server's listening port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 대기 중인 연결 대기열의 최대값을 가져오거나 설정한다.
    /// </summary>
    public int Backlog { get; set; } = DefaultBacklog;

    /// <summary>
    /// 처리된 클라이언트 버퍼 크기를 가져오거나 설정한다.
    /// </summary>
    public int ClientBufferSize { get; set; } = DefaultClientBufferSize;

    /// <summary>
    /// 수신 전략 유형을 가져오거나 설정한다.
    /// </summary>
    public ReceiveStrategyType ReceiveStrategy { get; set; }

    /// <summary>
    /// 기본 서버 패킷 프로세서를 가져온다.
    /// </summary>
    public ILitePacketProcessor PacketProcessor { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="LiteServerOptions"/> instance
    /// with a default <see cref="LitePacketProcessor"/>.
    /// </summary>
    public LiteServerOptions()
    {
        PacketProcessor = new LitePacketProcessor();
    }
}
``` 
    

## 수신 전략 유형:    
```
public enum ReceiveStrategyType
{
    /// <summary>
    /// 기본 전략이다. 수신 패킷이 수신된 후 바로 처리한다.
    /// </summary>
    Default,

    /// <summary>
    /// 수신된 패킷을 수신 대기열에 넣는다. 패킷은 수신된 순서와 동일한 순서로 처리된다.
    /// </summary>
    Queued
}
```
  
`Default`은 패킷이 오면 바로 Task.Run으로 비동기로 `HandleMessageAsync`가 호출하도록 하고, `Queued`는 일단 Queue에 넣고 동기로 `HandleMessageAsync` 호출해서 처리하도록 한다.   
`Default` 전략을 사용하면 동일 세션이 빠르게 패킷을 보내면 동시에 패킷이 처리될 수 있다(만약 패킷 처리하는 로직이 멀티스레드 이고, 각 세션이 특정 스레드에 묶이지 않는다면).   
  
### Default
`internal class LiteDefaultConnectionToken : ILiteConnectionToken`    
```
public void ProcessReceivedMessages(IEnumerable<byte[]> messages)
{
    Task.Run(async () =>
    {
        foreach (var messageBuffer in messages)
        {
            await _handlerAction(Connection, messageBuffer).ConfigureAwait(false);
        }
    });
}
```
   
### Queued
`internal class LiteQueuedConnectionToken : ILiteConnectionToken`  
```
public LiteQueuedConnectionToken(LiteConnection connection, Func<LiteConnection, byte[], Task> handlerAction)
{
    Connection = connection;
    _handlerAction = handlerAction;
    DataToken = new LiteDataToken(Connection);
    _receiveMessageQueue = new BlockingCollection<byte[]>();
    _receiveCancellationTokenSource = new CancellationTokenSource();
    _receiveCancellationToken = _receiveCancellationTokenSource.Token;
    Task.Factory.StartNew(OnProcessMessageQueue,
        _receiveCancellationToken,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default);
}

private async Task OnProcessMessageQueue()
{
    while (!_receiveCancellationToken.IsCancellationRequested)
    {
        try
        {
            byte[] message = _receiveMessageQueue.Take(_receiveCancellationToken);
            await _handlerAction(Connection, message).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // The operation has been cancelled: nothing to do
        }
    }
}

public void ProcessReceivedMessages(IEnumerable<byte[]> messages)
{
    foreach (byte[] message in messages)
    {
        _receiveMessageQueue.Add(message);
    }
}
```  
   
<br>  
  

## TODO
- [ ] LitePacketParser 에서 ParseIncomingData() 에서 패킷을 처리하는 부분을 수정해야 한다. 패킷 헤더 정보가 잘보되면 무한으로 패킷 데이터 파싱을 할 수도 있다.
- [ ] LiteDataToken 에서 헤더와 보디 데이터 담는 부분은 패킷 Receive할 때마다 할당과 해제를 할 필요는 없다고 본다. Reset()에서 이 두개의 버퍼를 null로 해서 매번 할당하게 만듬
- [ ] LiteSender 에서 패킷을 보내는 스레드를 만듬. LiteConnection 마다 LiteSender을 가지고 있으므로 결과적으로 1세션당 1개의 Sender용 스레가 만들어진다.  



<br>  
<br>  

아래는 원본에 있는 글이다.  
--  
  
# LiteNetwork

[![Build](https://github.com/Eastrall/LiteNetwork/actions/workflows/build.yml/badge.svg)](https://github.com/Eastrall/LiteNetwork/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/LiteNetwork.svg)](https://www.nuget.org/packages/LiteNetwork/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/LiteNetwork)](https://www.nuget.org/packages/LiteNetwork/)

`LiteNetwork` is a simple and fast networking library built with C# and compatible with .NET Standard 2, .NET 5 and .NET 6. Its main goal is to simply the creation of basic socket servers over the TCP/IP protocol.

Initially, LiteNetwork has been initialy developed for game development networking, but can also be used for other purposes.

## How to install

`LiteNetwork` is shiped as a single package, you can install it through the Visual Studio project package manager or using the following command in the Package Manager Console:

```sh
$> Install-Package LiteNetwork
```

Or you can use the dotnet command:

```sh
$> dotnet add package LiteNetwork
```

## Getting started

### Create a server

There is two ways of building a TCP server with `LiteNetwork`:
* The instance way, by creating a `LiteServer` instance and then run it manually
* The service way
    * In fact, `LiteNetwork` provides an extension to the `ServiceCollection` object, and can be integrated in a [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) (used by ASP.NET Core, MAUI).

#### Common code

First of all, you will need to create the user class that will represent a connected user on your server. Simple create a new `class` that implements the `LiteServerUser` class.

```csharp
using LiteNetwork.Server;

public class ClientUser : LiteServerUser
{
}
```

Within this class, you will be able to handle this client's incoming message sent by a client program thanks to the `HandleMessageAsync()` method.
You can also be notified when the client connects to the server or disconnects.

```csharp
using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Server;

public class TcpUser : LiteServerUser
{
    public override Task HandleMessageAsync(byte[] packetBuffer)
    {
        // Handle incoming messages using a BinaryReader or any other solution for reading a byte[].
    }

    protected override void OnConnected()
    {
        // When the client connects.
    }

    protected override void OnDisconnected()
    {
        // When the client disconnects.
    }
}
```

Once the server user is ready, you can create the server itself that will handle this `TcpUser` type of users.
Create another new `class`, and implement the `LiteServer<T>` class where `T` is the previously created `TcpUser`.

```csharp
public class MyTcpServer : LiteServer<TcpUser>
{
    public MyTcpServer(LiteServerOptions options, IServiceProvider serviceProvider = null)
        : base(options, serviceProvider)
    {
    }
}
```
The server has some hooks that allows you to control its life time, such as:

| Method | Description |
|--------|-------------|
| `OnBeforeStart()` | Called before the server starts. |
| `OnAfterStart()` | Called after the server starts.  |
| `OnBeforeStop()` | Called before the server stops. |
| `OnAfterStop()` | Called after the server stops. |
| `OnError(ILiteConnection, Exception)` | Called when there is an unhandled error witht the given `ILiteConnection`. |


#### Create the server via instance

Now that the server and user classes are built, you can now instanciate your server and call the `Start()` method to start the server.

```csharp
// Using top-level statement
using LiteNetwork.Server;
using System;

// Create the server configuration, to listen on "127.0.0.1" and port "4444"
var configuration = new LiteServerOptions()
{
    Host = "127.0.0.1",
    Port = 4444
};

// Create the server instance by givin the server options and start it.
using var server = new MyTcpServer(configuration);
server.Start();

// Just for the example, otherwise the console will just shutdown.
// Do not use in production environment.
Console.ReadKey(); 
```

#### Create the server via service

For this example, you will need to install the [`Microsoft.Extensions.Hosting`](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) package from nuget in order to build a [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

```csharp
// Using top-level statement
using Microsoft.Extensions.Hosting;
using System;

var host = new HostBuilder()
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
```

Then, once your host is setup and running, you can configure the `LiteServer` service using the `ConfigureLiteNetwork()` method located in the `LiteNetwork.Hosting` namespace:

```csharp
// Using top-level statement
using LiteNetwork.Hosting;
using LiteNetwork.Server.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

var host = new HostBuilder()
    // Configures the LiteNetwork context.
    .ConfigureLiteNetwork((context, builder) =>
    {
        // Adds a LiteServer instance for the MyTcpServer class.
        builder.AddLiteServer<MyTcpServer>(options =>
        {
            // This configures the server's LiteServerOptions instance.
            options.Host = "127.0.0.1";
            options.Port = 4444;
        });
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
```

Your server is now listening on "127.0.0.1" and port "4444".
Also, since you are using a .NET generic host, it also provides dependency injection into the server and client classes. Hence, you can inject services, configuration (`IOptions<T>` if configured, etc..).

> Note: You can also add as many servers you want into a single .NET generic host by calling the `builder.AddLiteServer<>()` method with different parameters.

### Create a client

There is two ways of building a TCP client with `LiteNetwork`:
* The instance way: by creating a `LiteClient` instance and then connect to the remote server manually.
* The service way
    * In fact, `LiteNetwork` provides an extension to the `ServiceCollection` object, and can be integrated in a [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) (used by ASP.NET Core, MAUI).

#### Common code

First of all, you will ned to create a new `class` that inherit from the `LiteClient` class.

```csharp
using LiteNetwork.Client;

public class MyTcpClient : LiteClient
{
    public EchoClient(LiteClientOptions options, IServiceProvider serviceProvider = null) 
        : base(options, serviceProvider)
    {
    }
}
```
Just like a LiteNetwork server, the client has some hooks that allows you to control the client lifetime, such as:

| Method | Description |
|--------|-------------|
| `HandleMessageAsync()` | Called when the client receives a message from the server. |
| `OnConnected()` | Called when the client is connected to the remote server. |
| `OnDisconnected()` | Called when the client is disconnected from the remote server. |
| `OnError(Exception)` | Called when there is an unhandled error within the client process. |

#### Create the client via instance

Using the previously created client, you can now create a new instance of the `MyTcpClient` class, set the correct options to connect to the remote server and then, call the `ConnectAsync()` method.

```csharp
// Using top-level statement
using LiteNetwork.Client;
using System;

var options = new LiteClientOptions()
{
    Host = "127.0.0.1",
    Port = 4444
};
var client = new CustomClient(options);
Console.WriteLine("Press any key to connect to server.");
Console.ReadKey();

await client.ConnectAsync();

// Do something while client is connected.
```

#### Create the client via service

For this example, you will need to install the [`Microsoft.Extensions.Hosting`](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) package from nuget in order to build a [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

```csharp
// Using top-level statement
using LiteNetwork.Client.Hosting;
using LiteNetwork.Hosting;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureLiteNetwork((context, builder) =>
    {
        builder.AddLiteClient<MyTcpClient>(options =>
        {
            options.Host = "127.0.0.1";
            options.Port = 4444;
        });
    })
    .UseConsoleLifetime()
    .Build();

// At this point, the client will connect automatically once the host starts running.
await host.RunAsync(); 
```

Once your program starts, your `MyTcpClient` will try to connect to the remote server ("127.0.0.1" and port 4444). Also, since you are using the .NET generic host, it also provides the dependency injection mechanism into the client. Hence, you can inject services, configuration (`IOptions<T>` if configured), logger, etc...

> Note: You can also add as many clients you want into a single .NET generic host by calling the `builder.AddLiteClient<>() method with different parameters.

## Protocol

### Packet Processor

TBA.

## Thanks

I would like to thank everyone that contributed to this library directly by fixing bugs or add new features, but also the people with who I had the chance to discuss about networking problematics which helped me to improve this library.

## Credits

Package Icon : from [Icons8](https://icons8.com/)
