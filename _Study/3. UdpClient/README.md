# ✍️ 3. UdpClient

## 💡 목차

UDP 클라이언트와 UDP 서버

- UDP 프로토콜

  - 특징
  - 사용법 (`UdpClient` 클래스)

- UDP 클라이언트

- UDP 서버

  

## 📝 UDP 클라이언트와 UDP 서버

- [이미지 출처](https://www.cs.dartmouth.edu/~campbell/cs60/socketprogramming.html)

![UDP소켓](./images/UDP%EC%86%8C%EC%BC%93.jpg)

  

### ✏️ UDP 프로토콜

#### ◼️ 특징

- UDP(User Datagram Protocol)는 TCP와 같이 IP에 기반한 전송 계층(Transport Layer) 프로토콜

- UDP는 단 2가지 기능만을 수행

  - IP 위에 포트를 더한다.
  - 데이타 Corruption을 감지해 불량 데이타를 폐기한다.

- TCP는 송수신 전에 반드시 연결(Connection)이 전제되어야 하는 반면, **UDP는 별도의 연결이 필요없다**.

  - 비유하자면
  - TCP는 전화와 같이 통신 전에 미리 연결이 되어있어야하고,
  - UDP는 메일과 같이 주소만 알면 그냥 보낼 수 있다.

- ❤️ UDP 단점

  - 데이타가 중간에 유실될 수 있다
  - 데이타가 도달하는 순서가 뒤바뀔 수 있다

- 🖤 UDP 장점

  - TCP와 달리 연결이 필요없고, 통신 절차가 단순하므로 더 효율적일 수 있다!
  - 데이타의 신뢰성이 중요하지 않는 경우 유용하게 사용될 수 있다.
  - 비디오 스트리밍, 주식 시세 등과 같이 계속 데이타가 들어오기 때문에 중간에 데이타 하나가 유실되더라도 크게 문제가 없는 경우 UDP가 많이 사용된다.
  - Broadcast와 Multicast에 유용하게 사용된다. (이 부분은 다시 정리)

    

#### ◼️ 사용법 (`UdpClient` 클래스)

- UDP를 사용하기 위해서는 `System.Net.Sockets` 네임스페이스 안의 `UdpClient` 클래스나 `Socket` 클래스를 사용한다.

- TCP와 달리 UDP는 **별도의 UDP 서버 클래스가 없으며, 서버도 `UdpClient` 클래스를 사용**한다.

    

### ✏️ UDP 클라이언트

> 아래의 예제는 간단한 메시지를 UDP 서버에 보내고 응답을 읽어 화면에 표시하는 간단한 프로그램이다.

```c#
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UdpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient client = new UdpClient();

            string msg = "안녕하세용!";
            byte[] datagram = Encoding.UTF8.GetBytes(msg);
            
            // 2. 데이타 송신
            client.Send(datagram, datagram.Length, "127.0.0.1", 7777);
            Console.WriteLine("[Send] 127.0.0.1 로 {0} 바이트 전송", datagram.Length);
            
            // 3. 데이타 수신
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = client.Receive(ref epRemote);
            Console.WriteLine("[Receive] {0} 로 부터 {1} 바이트 수신", epRemote, bytes.Length);
            
            // 4. UdpClient 객체 닫기
            client.Close();
        }
    }
}
```

1. UDP 통신을 위해 `System.Net.Sockets` 네임스페이스의 `UdpClient` 객체를 생성한다.

   - `UdpClient` 생성자에서 서버와 포트를 줄 수도 있고,

     ```c#
     IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9999);
     UdpClient server = new UdpClient(ipep);
     ```

   - 만약 하나의 `UdpClient` 객체로 여러 서버에 데이타를 보낼 경우는 위 예제처럼 `Send()` 메서드에서 서버와 포트를 지정한다.

2. `UdpClient` 객체의 `Send()` 메서드를 사용해 데이타(UDP에서는 **datagram**이라고 함)를 서버에 보낸다. 

   - 네트워크 데이타 송수신은 기본적으로 바이트 데이타를 사용하는데, 따라서 문자열을 보낼 경우 먼저 바이트로 인코딩한 후 보내게 된다. 
   - 보통 영문은 ASCII로 인코딩하고, 한글 등 비영문 문자열은 UTF 인코딩을 사용한다.
   - UDP datagram은 최대 65,527 바이트(헤더 8 바이트 , 총 65,535)까지 전송할 수 있다.

3. UDP 에서 데이타를 수신할 경우는 `UdpClient` 객체의 `Receive()` 메서드를 사용한다.

   - `Receive()` 메서드는 수신 데이타와 함께 상대 컴퓨터의 종단점(IP 주소와 포트) 정보도 같이 전달받는데, 이를 위해 `IPEndPoitn` 객체를 ref 파라미터로 전달한다. 이를 통해 데이타가 수신되면 누가 그 데이타를 전송했는지 알 수 있다.
   - TCP와 달리 UDP는 Connectionless 프로토콜이기 때문에 이렇게 **누가 보낸 데이타인지를 알 필요가 있다**.

4. 마지막으로 `UdpClient` 객체를 닫는다.

​    

### ✏️ UDP 서버

- UDP 서버는 포트를 열고 클라이언트로부터 들어오는 datagram을 수신하게 된다. 즉, UDP 서버는 통상 UDP 포트를 Listening 하고 있으면서 루프 안에서 계속 데이타 송수신을 처리하는 형태로 구현된다.
- UDP는 별도의 Connection 과정이 필요없으며, UDP 클라이언트로부터 datagram을 직접 받아 처리하면 된다.
- 따라서, UDP 서버는 UDP 클라이언트와 거의 동일한 기능을 갖기 때문에 **별도의 UDP 서버 클래스가 없고 `UdpClient` 클래스를 사용**한다.

```c#
using System;
using System.Net;
using System.Net.Sockets;

namespace UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. UdpClient 객체 생성. 포트 7777에서 Listening
            UdpClient server = new UdpClient(7777);

            try
            {
                while (true)
                {
                    Console.WriteLine("수신 대기...");
                    
                    // 클라이언트 IP를 담을 변수
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                    // 2. 데이타 수신
                    byte[] datagram = server.Receive(ref remoteEP);
                    Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신", remoteEP, datagram.Length);

                    // 3. 데이타 송신
                    server.Send(datagram, datagram.Length, remoteEP);
                    Console.WriteLine("[Send] {0} 로 {1} 바이트 송신", remoteEP, datagram.Length);
                }
            }
            catch (Exception e)
            {    
                Console.WriteLine(e.ToString());
            }
            // 4. UdpClient 객체 닫음
            server.Close();
        }
    }
}
```

1. UDP 클라이언트로부터 데이타를 받아들이기 위해 먼저 Listening할 포트를 지정하며, `UdpClient` 객체를 생성한다.
2. UDP에서 데이타를 수신하기 위해 `UdpClient`의 `Receive()` 메서드를 사용한다.
   - `Receive()` 메서드는 수신 데이타와 함께 상대 UDP 클라이언트의 종단점(IP 주소와 포트) 정보도 같이 전달받는데, 이를 위해 `IPEndPoint`의 객체를 ref 파라미터로 전달한다.
   - 데이타 수신 후, ref 파라미터를 체크하면 데이타를 보낸 UDP 클라이언트의 IP 주소와 포트를 알 수 있다.
3. `UdpClient` 객체의 `Send()` 메서드를 사용하여 데이타를 UDP 클라이언트로 전달한다. 이때 클라이언트 IP는 위의 `Recevie()` 메서드에서 받아온 IP 주소를 사용한다.
4. 마지막으로 `UdpClient` 객체를 닫는다.