# ✍️ 1. Socket 클래스

## 💡 목차

- BSD 소켓

- TCP vs. UDP

- Socket 클래스 (`System.Net.Sockets.Socket`)

  - Socket 생성자

  - UDP 소켓

  - TCP 소켓

    

## 📝 BSD 소켓

![BSD 소켓](./images/BSD소켓.png)

## 📝 TCP vs. UDP

| 기준       | TCP                                                          | UDP                                                          |
| ---------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| **연결성** | 통신 전에 반드시 서버로 연결(연결 지향성 : connection-oriented) | 연결되지 않고 동작 가능(비연결 지향성 : connectionless)      |
| **신뢰성** | 데이터를 보냈을 떄 반드시 상대방은 받았다는 신호를 보내줌(신뢰성 보장) | 데이터를 보낸 측은 상대방이 정상적으로 데이터를 받았는지 알 수없음 |
| **순서**   | 데이터를 보낸 순서대로 상대방은 받게 됨                      | 데이터를 보낸 순서와 상관없이 먼저 도착한 데이터를 받을 수 있음 |
| **속도**   | 신뢰성 및 순서를 확보하기 위해 부가적인 통신이 필요하므로 UDP에 비해 다소 느림 | 부가적인 작업을 하지 않으므로 TCP보다 빠름                   |

​    

## 📝 Socket 클래스 (`System.Net.Sockets.Socket`)

- 운영체제는 TCP/IP 통신을 위해 소켓(socket)이라는 기능을 만들어 두고 있으며, 닷넷 응용 프로그램도 소켓을 이용해 **다른 컴퓨터와 TCP/IP 통신을 할 수 있다**.

- `Socket` 클래스는 가장 Low 레벨의 클래스로서, **`TcpClient`, `TcpListener`, `UdpClient` 들은 모두 `Socket` 클래스를 이용하여 작성**되었다.

- `TcpClient`, `TcpListener`, `UdpClient` 들이 모두 TCP/IP와 UDP/IP 프로토콜만을 지원하는 반면, `Socket` 클래스는 IP 뿐만아니라 AppleTalk, IPX, Netbios, SNA 등 다양한 네트워크들에 대해 사용될 수 있다.

- 여기서는  `Socket` 클래스를 사용해 TCP, UDP 네트워크를 사용하는 부분에 대해 살펴본다! 

    

🙋🏼 잠깐! : 바인딩(binding) 이란?

- 클라이언트 소켓은 데이터가 전송돼야 할 대상을 지정하기 위해 접점 정보(종단점, IP 주소와 포트)가 필요하다.

- 따라서, **서버 소켓은 컴퓨터에 할당된 IP 주소 중에서 어떤 것과 연결될지 결정해야하고 이 과정을 바인딩(binding)**이라 한다.

- 간단히 말해서 소켓이 특정 IP와 포트에 묶이면 바인딩이 성공했다고 볼 수 있다. 일단 이렇게 바인딩되고 나면 다른 소켓에서는 절대로 동일한 접점 정보로 바인딩할 수 없다.

- 소켓은 컴퓨터에 할당된 모든 IP에 대해 바인딩할 수 있는 방법을 제공하며, 이때 사용하는 특별한 주소가 "0.0.0.0" 이며, IPAddress.Any를 사용해 코드를 줄일 수 있다.

  ```c#
  Socket socket = ...;
  
  IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
  IPEndPoint endPoint = new IPEndPoint(ipAddress, 10200)l
  
  socket.Bind(endPoint);
  ```

  ```c#
  Socket socket = ...;
  
  IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 10200);
  
  socket.Bind(endPoint);
  ```

  

### ✏️ Socket 생성자

Socket 생성자는 3개의 인자를 받는다.

```c#
public Socket {
    AddressFamily addressFamily,
    SocketType socketType,
    ProtocolType protocolType
}
```

- 모든 인자가 enum 형식

  - AddressFamily는 31개, SocketType은 6개, ProtocolType은 25개의 값을 갖는다....!

- 현실적으로 사용하는 방법은 딱 두가지 조합!

  - "SocketType.Stream + Protocol.Tcp"로 생성된 소켓 : 스트림 소켓 또는 TCP 소켓
  - "SocketType.Dgram + Protocol.Udp"로 생성된 소켓 : 데이터그램 소켓 또는 UDP 소켓
  - TCP와 UDP가 모두 IP 프로토콜을 기반으로 동작한다.

  ```c#
  Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
  Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
  // IPv6용 소켓을 생성하려면 첫 번째 인자에 AddressFamily.InterNetworkV6 값을 주면된다.
  ```

- `Socket` 클래스는 `IDisposable`을 상속받았다!

  - 따라서, 소켓을 생성한 후 필요가 없어지면 반드시 자원을 해제해야 한다.

  ```c#
  // 소켓 자원 생성
  Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
  
  // --- [소켓을 사용해 통신] ---
  
  // 반드시 소켓 자원 해제
  socket.Close();
  ```
  

  

### ✏️ UDP 소켓

#### 특징

- **비연결성**

  - UDP 클라이언트 측에서 명시적인 Connection 설정 과정이 필요 없다.

- **신뢰성 결여**

  - 전달된 데이터가 상대방에게 반드시 도착한다는 보장이 없다.

- **순서 없음**

  - 송신자가 보낸 순서와 다르게 데이터를 받을 수도 있다.

- **최대 65,535 바이트 라는 한계**

  - SendTo 메서드에 전달하는 바이트의 크기는 65535를 넘을 수 없다.(각종 데이터 패킷의 헤더로 인해 그 크기는 다소 줄어든다.) 
  - 또한 UDP 데이터가 거쳐가는 네트워크 장비 중에는 32KB 정도만을 허용하도록 제약하는 경우도 있으므로 SendTo 메서드에 많은 데이터를 보내는 것은 권장하지 않는다.

- **파편화(fragmentation)**

  - UDP를 이용해 많은 데이터를 보내는 것은 좋지 않은 선택이다.
  - 이론상 최대 64KB의 데이터를 SendTO로 보낸다고해도 TCP/IP 통신에서는 64KB가 약 1000바이트 정도로 분할되어 전송될 수 있다. 그렇게 되면 64번의 데이터를 전송하게 되는데, 이 중하나라도 중간에 패킷이 유실되면 수신측의 네트워크 장치가 받은 63개의 패킷은 폐기 되어버린다.
  - 즉, 한번에 보내는 UDP 데이터의 양이 많을수록 데이터가 폐기될 확률이 더 높아진다.

- **메시지 중심(message-oriented)**

  - 송신 측에서 한 번의 SendTo() 메서드 호출에 1000바이트의 데이터를 전송했다면 수신 측에서도 ReceiveFrom 메서드를 한 번 호출했을 때 1000바이트를 받는다.

  - 즉, SendTo에 전달된 64KB 이내의 바이트 배열은 상대방에게 정상적으로 보내는 데 성공하기만 한다면 ReceiveFrom 메서드에서는 그 바이트 배열 데이터를 그대로 한번에 받을 수 있다.

  - 메시지 중심의 통신이란 이런 식으로 보내고 받는 메시지의 경계(message boundary)가 지켜짐을 의미한다.

    

#### ⬛️ BasicUDP

##### 예제1)

```c#
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BasicUDP
{
    class Program
    {
        static void Main(string[] args)
        {
            // 서버 소켓이 동작하는 스레드
            Thread serverThread = new Thread(serverFunc);
            serverThread.IsBackground = true;
            serverThread.Start();
            
            Thread.Sleep(500); // 서버 소켓용 스레드가 실행될 시간을 주기 위함
            
            Thread clientThread = new Thread(clientFunc);
            clientThread.IsBackground = true;
            clientThread.Start();
            
            Console.WriteLine("종료하려면 아무 키나 누르세요....");
            Console.ReadLine();
        }

        private static void serverFunc(object obj)
        {

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 10200);

            socket.Bind(endPoint);

            byte[] recvBytes = new byte[1024];
            EndPoint clientEP = new IPEndPoint(IPAddress.None, 0);

            while (true)
            {
                int nRecv = socket.ReceiveFrom(recvBytes, ref clientEP);
                string txt = Encoding.UTF8.GetString(recvBytes, 0, nRecv);

                byte[] sendBytes = Encoding.UTF8.GetBytes("Hello:" + txt + " from." + clientEP);
                socket.SendTo(sendBytes, clientEP);
            }
        }

        private static void clientFunc(object obj)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 10200);
            EndPoint senderEP = new IPEndPoint(IPAddress.None, 0);

            int times = 5;

            while (times-- > 0)
            {
                byte[] buf = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                socket.SendTo(buf, serverEP);

                byte[] recvBytes = new byte[1024];
                int nRecv = socket.ReceiveFrom(recvBytes, ref senderEP);

                string txt = Encoding.UTF8.GetString(recvBytes, 0, nRecv);
                
                Console.WriteLine(txt);
                Thread.Sleep(1000);
            }

            socket.Close();
            Console.WriteLine("UDP Client socket: Closed");
        }
        
    }
}
```

​    

### ✏️ TCP 소켓

#### 특징

- **연결성(connection-oriented)**
  
  - TCP 통신은 서버 측의 Listen/Accept와 클라이언트 측의 Connect를 이용해 반드시 연결이 이뤄진 다음 통신할 수 있다.
- **신뢰성**
  - Send 메서드를 통해 수신 측에 데이터가 전달되면 수신 측은 내부적으러 그에 대한 확인(ACK) 신호를 송신 측에 전달한다.
  - 따라서 TCP 통신은 데이터가 수신 측에 정상적으로 전달됐는지 알 수 있고, 이 과정에서 ACK 신호가 오지 않으면 자동적으로 데이터를 재전송함으로써 신뢰성을 확보한다.
- **순서 보장**
  
  - 데이터를 보낸 순서대로 수신 측에 전달된다. 예를 들어, 3번의 Send 메서드가 호출돼 각각 100, 105, 102 바이트가 전송되는 경우, 수신 측의 첫 번째 Receive 메서드는 100 바이트에 해당하는 데이터를 먼저 스트림 방식으로 수신하게 된다.
- **스트림 중심(stream-oriented)**
  - 메시지 간의 경계가 없다.
  - 예를 들어, 10,000 바이트의 데이터를 Send 메서드를 이용해 송신하는 경우 내부적인 통신 상황에 따라 2048, 2048, 5904 바이트 단위로 잘라서 전송될 수 있다. 따라서 1번의 Send 메서드가 실행됐음에도 수신하는 측은 여러 번 Receive 메서드를 호출해야만 모든 데이터를 받을 수 있다.
  - 이렇게 메시지 경계를 가지지 않고 전달되는 것을 **스트림 방식** 이라고 한다.

  

🙋🏼 잠깐! :  TCP는 뭉쳐서 오지만, UDP는 뭉치지 않는다!

**TCP**

![뭉쳐서온다](./images/%EB%AD%89%EC%B3%90%EC%84%9C%EC%98%A8%EB%8B%A4.png)

**UDP**

![뭉치지않는다](./images/%EB%AD%89%EC%B9%98%EC%A7%80%EC%95%8A%EB%8A%94%EB%8B%A4.png)

  

#### ⬛️ BasicSocketClient

##### 예제 1)

```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BasicSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Connect();
        }

        static void Connect()
        {
            // 1. 서버에 접속
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(ipep);
            
            Console.WriteLine("Socket connect");

            // 2. 서버로부터 데이터 받기
            Byte[] _data = new Byte[1024];
            client.Receive(_data);
            
            String _buf = Encoding.Default.GetString(_data);
            Console.WriteLine(_buf);
            
            // 3. 서버에 데이터 보내기
            _buf = "소켓 접속 확인 됐습니다";
            _data = Encoding.Default.GetBytes(_buf);
            client.Send(_data);
            
            // 4. 서버와 접속 끊기
            client.Close();
            
            Console.WriteLine("Press and key...");
            Console.ReadLine();
        }
    }
}
```

  

##### 예제 2)

```C#
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BasicSocketClient
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Basic TCP Client!");
            StartClient();
            return 0;
        }

        public static void StartClient()
        {
            // Data buffer for incoming data
            byte[] bytes = new byte[1024];
            
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // 1. 소켓 객체 생성 (TCP 소켓)
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 2. 서버에 연결
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint);

                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // 3. 소켓에 데이터 전송
                    int bytesSent = sender.Send(msg);

                    // 4. 서버에서 데이터 수신
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // 5. 소켓 닫기
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
```

1. 먼저 `Socket` 객체를 생성하는데

   - 첫번째 파라미터는 IP를 사용한다는 것이고,
   - 두번째 파라미터는 스트림 소켓을 사용한다는 것이며,
   - 마지막 파라미터는 TCP 프로토콜을 사용한다는 것을 지정한 것이다.
   - **TCP 프로토콜은 스트림 소켓을 사용하고, UDP는 데이터그램 소켓을 사용한다**.

2. `Socket` 객체의 `Connect()` 메서드를 호출하여 서버 종단점(EndPoint)에 연결한다.

3. 소켓을 통해 데이터를 보내기 위해 `Socket` 객체의 `Send()` 메서드를 사용하였다.

   - 데이터 전송을 첫번째 파라미터에 바이트 배열을 넣으면 되고,

   - 두번째 파라미터는 옵션으로 `SocketFlags`를 지정할 수 있다.

     (이 플래그는 소켓에 보다 고급 옵션들을 지정하기 위해 사용된다.)

4. 소켓에 데이터를 수신하기 위해 `Socket` 객체의 `Receive()` 메서드를 사용하였다.

   - `Receive()` 메서드는 첫번째 파라미터에 수신된 데이터를 넣게 되고,
   - `Send()` 와 마찬가지로 `SocketFlags` 옵션을 지정할 수도 있다.
   - `Receive()` 메서드는 실제 수신된 바이트수를 정수로 리턴한다.

5. 마지막으로 소켓을 닫는다

  

#### ⬛️ BasicSocketServer

##### 예제 1)

```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BasicSocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 서버 소켓 초기화
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9999);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(20);

            Console.WriteLine("Server Start....Listen port 9999...");

            // 2. 클라이언트가 접속 할 때까지 대기...
            Socket client = server.Accept();
            
            // 3. 새로운 클라이언트 접속
            IPEndPoint ip = (IPEndPoint) client.RemoteEndPoint;
            
            Console.WriteLine("주소 {0} 에서 접속", ip.Address);

            // 4. 클라이언트에 데이터를 보낸다
            String _buf = "Woori 서버에 오신 걸 환영합니다.";
            Byte[] _data = Encoding.Default.GetBytes(_buf);
            client.Send(_data);

            // 5. 클라이언트로부터 데이터를 받는다
            _data = new Byte[1024];
            client.Receive(_data);
            _buf = Encoding.Default.GetString(_data);

            Console.WriteLine(_buf);

            // 클라이언트/서버 소켓 종료
            client.Close();
            server.Close();
        }
    }
}
```

  

##### 예제 2)

```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BasicSocketServer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Basic TCP Server");
            StartListening();
            return 0;
        }

        // Incoming data from the client
        public static string data = null;

        public static void StartListening()
        {
            // Data buffer for incoming data
            byte[] bytes = new Byte[1024];

            // Dns.Resolve(...) is obsoleted for this type
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            
            // 1, 소켓 객체 생성 (TCP 소켓)
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 2. 포트에 Bind
                listener.Bind(localEndPoint);
                
                // 3. 포트 Listening 시작
                listener.Listen(10);
                
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    
					// 4. 연결을 받아들여 새 소켓 생성
                    Socket handler = listener.Accept();
                    data = null;

                    while (true)
                    {
                        // 5. 소켓 수신
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Text received : {0}", data);
                    
                    byte[] msg = Encoding.ASCII.GetBytes(data);
					
                    // 6. 소켓 송신
                    handler.Send(msg);
                    
                    // 7. 소켓 닫기
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
```

1. 먼저 `Socket` 객체를 생성하는데, 이는 Socket 클라이언트에서 `Socket` 객체를 생성하는 것과 동일하다.
2. 서버는 포트를 열고 클라이언트 접속을 기다리는데, 먼저 어떤 포트를 사용할지 Bind 해주게 된다.
3. 서버에서 포트를 열고 클라이언트 접속을 실제 기다리기 위해 `Socket` 객체의 `Listen()` 메서드를 사용한다.
   - `Listen()` 메서드는 동시에 여러 클라이언트가 접속되었을 때, 큐에 몇 개의 클라이언트가 대기할 수 있는지 지정할 수 있다. 위의 경우는 예시를 위해 10을 넣었다.
4. `Socket` 객체의 `Accept()` 메서드를 사용하여 클라이언트 접속을 받아들이고 새 소켓 객체를 리턴한다. 이후 클라이언트는 이 새로 생성된 소켓 객체와 통신하게 된다.
   - 이 부분은 UDP와 TCP의 가장 큰 차이점이다. TCP 서버용 소켓 인스턴스는 클라이언트와 직접 통신할 수 없고 오직 새로운 연결을 맺는 역할만 한다. 클라이언트와의 직접적인 통신은 서버 소켓의 Accept에서 반환된 소켓 인스턴스로만 할 수 있다.
     ![새로운_소켓](./images/%EC%83%88%EB%A1%9C%EC%9A%B4_%EC%86%8C%EC%BC%93.png)

5. 소켓에서 데이터를 수신하기 위해 `Socket` 객체의 `Receive() ` 메서드를 사용하였다. 이는 소켓 클라이언트에서 수신하는 것과 동일하다.

6. 소켓에서 데이터를 전달하기 위해 `Socket` 객체의 `Send()` 메서드를 사용하였다. 이는 소켓 클라이언트에서 송신하는 것과 동일하다.

7. 마지막으로 소켓을 닫는다.

