# C# 네트워크 프로그래밍 학습 저장소  
  
  
## 학습
- [C# 네트워크 API 설명](https://github.com/jacking75/com2usStudy_CSharpNetworkProgramming/tree/hellowoori/_Study )
- [C# 네트워크 프로그래밍 기초](https://docs.google.com/document/d/e/2PACX-1vSQHI4OAHL_zOa1DjJRiW7arDLy160tE2uo1TWvoe8PtPKct8bR0VP84iYQnLhjYoix0-HoJkdvoHNC/pub )  
- [C# 네트워크 프로그래밍 비동기I/O](https://docs.google.com/document/d/e/2PACX-1vRhA1jfWuZs8mUHHN9Cv0VyesDCD7exPbgy6ZjdCGMHNNu4O_gyhysyzwpVfJmmmcCOG--JCgL8htxW/pub )  
- [(인프런) C# 네트워크 프로그래밍](https://inf.run/2yN5 )    
- [(유튜브) C# 네트워크 프로그래밍 공부하기](https://www.youtube.com/watch?v=lMVdPDvElKg )  
   
    
   
## 고성능 네트워크 관련 글
  
### .NET Framework의 비동기 네트워크 라이브러리 성능 개선
- 닷넷은 현재 버전 4.5가 나왔다.(2013.04.05) 
    - 성장한 만큼 많은 개선과 기능 추가가 있었다. 그러나 프로그래머들이 닷넷의 성장을 따라가지 못하는 경우가 많다.
    - 닷넷을 메인으로 사용하지 않는 경우는 당연하고, 주위의 닷넷 프로그래머들의 이야기를 들어보면 닷넷 프로그래머들도 최신 기술을 모르던가 사용하지 않는 경우가 많다고한다. 
    - 대부분 2.0 대의 기술을 자주 사용한다고 한다.
- 닷넷으로 고성능 네트웍 프로그래밍을 하기 위해 자료를 찾아보면 이전 방식을 사용한 경우가 많다.
    - 혹시 아래 소개한 것을 본적이 없다면 꼭 이것을 보고 닷넷으로 서버 프로그램을 개발하기 바란다(물론 궂이 고성능을 필요로 하지 않으면 기존 방식대로 해도 좋다)
  
  
### SocketAsyncEventArgs
- 닷넷에서 고성능 네트웍 프로그래밍을 하기 위해서는 필연적으로 3.5에서 추가된 SocketAsyncEventArgs 클래스에 대해서 알아야 한다.
    - http://msdn.microsoft.com/ko-kr/library/system.net.sockets.socketasynceventargs.aspx
- 이것을 사용한 샘플은
    - http://archive.msdn.microsoft.com/nclsamples/Wiki/View.aspx?title=Socket%20Performance&referringTitle=Home
- 샘플을 보면 알겠지만 고성능을 내기 위해서 우리가 지금까지 일반적으로 해왔던 동적 메모리 사용 대신 
    - 메모리풀 및 정적 메모리 사용을 닷넷 네트워크 프로그래밍에서 어떻게 적용하는지 나타내고 있다. 
    - 닷넷은 기본적으로 동적 메모리 할당이 네이티브 보다는 좋겠지만 그래도 메모리 재사용 보다는 성능 측면에서 좋을 수가 없고, 
    - 또 가비지 컬렉션에 대한 부담이 있다.
- CodeProject의 'C# SocketAsyncEventArgs High Performance Socket Code'강좌
    - 위의 예제보다 더 자세하고 부족한 부분을 추가해서 잘 설명하고 있다. 강추 
    - http://www.codeproject.com/Articles/83102/C-SocketAsyncEventArgs-High-Performance-Socket-Cod
- [SocketAsyncEventArgs - 'SocketAsyncEventArgs'의 이해](https://blog.danggun.net/3596 )  
- [C# 채팅 프로그램 #01 IOCP - EAP 패턴을 이용한 비동기 TCP/IP 서버 구현 (SocketAsyncEventArgs)](https://jeongkyun-it.tistory.com/87 )
  
### High performance asynchronous awaiting sockets
- http://stackoverflow.com/questions/17093206/high-performance-asynchronous-awaiting-sockets
- [Converting Microsoft's Asynchronous Server Socket Sample to Async CTP](http://social.msdn.microsoft.com/Forums/en-US/aac79f64-5886-40f5-a8f1-a4a6f2460c85/converting-microsofts-asynchronous-server-socket-sample-to-async-ctp)
    
  
  
## Tips
  
### 소켓 타임 아웃 설정
```
// 접속 대기
Socket client = tcplistener.AcceptSocket();

// 2초 동안 수신하지 못하면 타임아웃으로 설정
client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000 );
```
  
```
TcpClient client = new TcpClient("127.0.0.1", 12345);
client.ReceiveTimeout = 2000;
client.SendTimeout = 2000;
```
  
### 자신의 IP 얻기
```
IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());
```
  
### IP, 맥어드레스 얻기 - NetworkInformation 방식
```
var adapters = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            
foreach (var adapter in adapters)
{
	if (adapter.OperationalStatus.Equals(System.Net.NetworkInformation.OperationalStatus.Up))
	{
		var properties = adapter.GetIPProperties();
		foreach (var ipInfo in properties.UnicastAddresses)
		{
			var ip = ipInfo.Address;
			if (!System.Net.IPAddress.IsLoopback(ip))
			{
				Console.WriteLine("IP = " + ip);
				Console.WriteLine("MAC = " + adapter.GetPhysicalAddress());
			}
		 }
	 }
}
```  
  
### 맥어드레스 얻기 - SendARP 방식
``` 
using System;
using System.Net;
using System.Runtime.InteropServices;

[DllImport("iphlpapi.dll", ExactSpelling=true)]
private static extern int SendARP( int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen );

private byte[] getMacAddress(string val)
{
    return getMacAddress(IPAddress.Parse(val));
}

private byte[] getMacAddress(IPAddress addr)
{
    byte[] mac = new byte[6];
    int len = mac.Length;
    int r = SendARP( BitConverter.ToInt32(addr.GetAddressBytes(), 0), 0, mac, ref len );

    return mac;
}
```
  
### UDP 브로드캐스트  
```
IPEndPoint remoteIP = new IPEndPoint(IPAddress.Broadcast, 10002);

byte[] data = new byte[16];
Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, 
                               ProtocolType.Udp );

s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 16);

// 브로드캐스트는 옵션으로 사용 가능하도록 한다 
s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

s.SendTo(data, data.Length, SocketFlags.None, remoteIP);
```
  
### UDP TTL 지정
```
// 보낼 곳
IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse("192.168.11.2"), 80);

// 보낼 데이터
byte[] data = new byte[16];

// UDP 소켓 만들기
Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.U에 );

// TTL를 설정
// TTL라는 것은…→　http://e-words.jp/w/TTL-2.html
s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 255);

// 데이터를 보낸다
s.SendTo(data, 0, data.Length, SocketFlags.None, remoteIP);
```  
  
### 컴퓨터의 네트워크 카드 리스트
```
using System.Management; // 참조 설정에 System.Management 를 추가

ManagementClass mc = new ManagementClass("Win32_PerfRawData_Tcpip_NetworkInterface");
ManagementObjectCollection moc = mc.GetInstances();

foreach (ManagementObject mo in moc)
{
    // 정보를 표시
    Console.WriteLine("이름     = {0}", mo["Name"]);
    Console.WriteLine("접속 속도 = {0} Mbps", 
                  Convert.ToInt32(mo["CurrentBandwidth"]) / 1000 / 1000);

    Console.WriteLine("------");
}
``` 
  
  