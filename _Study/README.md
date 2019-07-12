# ✍️ C# 네트워크 프로그래밍 스터디

#### 🔎 출처

##### 📚 참고서적

- 모두의 네트워크

- 시작하세요! C# 7.3 프로그래밍

  

##### 💻 참고사이트

- [예제로 배우는 C# 프로그래밍](http://www.csharpstudy.com/net/article/1-csharp-%eb%84%a4%ed%8a%b8%ec%9b%8c%ed%81%ac-%ed%94%84%eb%a1%9c%ea%b7%b8%eb%9e%98%eb%b0%8d)

- [명월 일지](https://nowonbun.tistory.com/155?category=507214)

- [Microsoft Docs](https://docs.microsoft.com/ko-kr/dotnet/api/system.net.sockets.socket?view=netframework-4.8)



## 💡 목차

### 0. [기본 개념]()

- 네트워크 모델

  - 프로토콜
  - OSI 모델과 TCP/IP 모델
  - 캡슐화와 역캡슐화

- WinSocket과 .NET 클래스

  - 소켓 API
  - WinSock API
  - .NET 네트워크 클래스

- IP 주소와 호스트

  - IP 주소 (`System.Net.IPAddress`)

  - DNS (`System.Net.Dns`)

  - IP 종단점 (`System.Net.IPEndPoint`)

    

### 1. [Socket 클래스]()

- BSD 소켓

- TCP vs. UDP

- Socket 클래스 (`System.Net.Sockets.Socket`)

  - Socket 생성자

  - UDP 소켓

  - TCP 소켓

    

### 2. [TcpClient와 TcpListener]()

TCP 클라이언트와 TCP 서버

- `TcpClient` 클래스

- `TcpListener` 클래스

  

### 3. [UdpClient]()

UDP 클라이언트와 UDP 서버

- UDP 프로토콜

  - 특징
  - 사용법 (`UdpClient` 클래스)

- UDP 클라이언트

- UDP 서버

  