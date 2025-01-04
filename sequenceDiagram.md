# Sequence Diagram  
    
## TCP/IP - 서버와 클라이언트 통신을 API 호출 중심으로 시퀸스 다이어그램
   
```mermaid
sequenceDiagram
    participant Server as TCP Server
    participant Client as TCP Client
    
    Note over Server: Socket()
    Note over Server: Bind()
    Note over Server: Listen()
    
    Note over Client: Socket()
    Client->>Server: Connect()
    Note over Server: Accept()
    
    rect rgb(200, 220, 255)
        Note right of Server: Data Communication
        Client->>Server: Send()
        Server-->>Client: Receive()
        Server->>Client: Send() 
        Client-->>Server: Receive()
    end
    
    Client->>Client: Close()
    Server->>Server: Close()
    
    Note over Server,Client: Connection Terminated
```
   
   
## BasicSocketClient - BasicSocketServer 시퀸스 다이어그램 
  
```mermaid   
sequenceDiagram
    participant Server
    participant Client
    
    Note over Server: Start Server
    Note over Server: Create Socket (TCP/IP)
    Note over Server: Bind to LocalEndPoint (port 11000)
    Note over Server: Listen(10)
    
    Note over Client: Start Client
    Note over Client: Create Socket (TCP/IP)
    Client->>Server: Connect to RemoteEndPoint
    
    Note over Server: Accept() Connection
    
    rect rgb(200, 220, 255)
        Note right of Server: Data Communication Loop
        Client->>Server: Send("This is a test<EOF>")
        Note over Server: Receive Data
        Note over Server: Process until <EOF>
        Server->>Client: Echo received data back
    end
    
    Note over Client: Receive echoed data
    
    Note over Client: Shutdown(Both)
    Note over Client: Close Connection
    
    Note over Server: Shutdown(Both)
    Note over Server: Close Connection
    
    Note over Server: Continue listening for new connections
    
    rect rgb(240, 240, 240)
        Note over Server,Client: Error Handling
        Note over Server: Try-Catch for socket operations
        Note over Client: Try-Catch for<br/>- ArgumentNullException<br/>- SocketException<br/>- General Exception
    end
```  
    
## BasicAsyncSocketServer 시퀸스 다이어그램
  
```mermaid
sequenceDiagram
    participant Client
    participant Main
    participant AsyncServer
    participant BeginAccept
    participant AcceptCallback
    participant ReceiveCallback
    participant SendCallback

    Main->>AsyncServer: AsyncServer()
    Note over AsyncServer: Initialize Socket Listener
    Note over AsyncServer: Bind to IP:50000
    Note over AsyncServer: Listen(2)
    
    rect rgb(200, 220, 255)
        Note over AsyncServer: User Input Loop
        AsyncServer->>BeginAccept: Press 1 (Start Accept)
        activate BeginAccept
    end

    loop While m_acceptLoop
        BeginAccept->>BeginAccept: BeginAccept with AsyncCallback
        Client->>BeginAccept: Connection Request
        BeginAccept->>AcceptCallback: Trigger AcceptCallback
        deactivate BeginAccept
        
        activate AcceptCallback
        Note over AcceptCallback: Create Session
        AcceptCallback->>ReceiveCallback: BeginReceive
        deactivate AcceptCallback
        
        activate ReceiveCallback
        Client->>ReceiveCallback: Send Data
        Note over ReceiveCallback: Process Received Data
        ReceiveCallback->>SendCallback: BeginSend Response
        deactivate ReceiveCallback
        
        activate SendCallback
        SendCallback-->>Client: Send Response
        Note over SendCallback: Log Send Complete
        deactivate SendCallback
    end

    opt Socket Exception
        Note over ReceiveCallback: Handle Client Disconnection
        ReceiveCallback-->>AsyncServer: Log Disconnection
    end

    AsyncServer->>AsyncServer: Press 2 (Stop)
    Note over AsyncServer: Close Listener
```

  
## AsyncSocketServer 시퀸스 다이어그램
  
```mermaid
sequenceDiagram
    participant Main
    participant AsyncServer
    participant Server
    participant BufferManager
    participant SocketAsyncEventArgsPool
    participant Socket

    Main->>AsyncServer: Call AsyncServer(args)
    AsyncServer->>AsyncServer: Parse command line args
    AsyncServer->>Server: new Server(numConnections, receiveSize)
    Server->>BufferManager: new BufferManager(receiveSize * numConnections * 2)
    Server->>SocketAsyncEventArgsPool: new SocketAsyncEventArgsPool(numConnections)
    AsyncServer->>Server: Init()
    Server->>BufferManager: InitBuffer()
    loop For each connection
        Server->>SocketAsyncEventArgsPool: Push new SocketAsyncEventArgs
    end
    AsyncServer->>Server: Start(localEndPoint)
    Server->>Socket: Create listen socket
    Server->>Socket: Bind(localEndPoint)
    Server->>Socket: Listen(100)
    Server->>Server: StartAccept(null)
    Server->>Socket: AcceptAsync
    
    alt AcceptAsync completed synchronously
        Server->>Server: ProcessAccept
    else AcceptAsync completed asynchronously  
        Socket-->>Server: AcceptEventArg_Completed callback
        Server->>Server: ProcessAccept
    end

    Server->>SocketAsyncEventArgsPool: Pop SocketAsyncEventArgs
    Server->>Socket: ReceiveAsync
    
    alt ReceiveAsync completed synchronously
        Server->>Server: ProcessReceive
    else ReceiveAsync completed asynchronously
        Socket-->>Server: IO_Completed callback (Receive)
        Server->>Server: ProcessReceive
    end
    
    Server->>Socket: SendAsync (Echo)
    
    alt SendAsync completed synchronously 
        Server->>Server: ProcessSend
    else SendAsync completed asynchronously
        Socket-->>Server: IO_Completed callback (Send)
        Server->>Server: ProcessSend
    end

    Server->>Server: StartAccept (Next client)
```     

### Server 클래스의 주요 역할은 다음과 같습니다:
  
1. 초기화 및 설정    
```mermaid
sequenceDiagram
    participant Client
    participant Server
    participant BufferManager
    participant SocketAsyncEventArgsPool

    Server->>BufferManager: 1. 버퍼 초기화
    Note over BufferManager: 모든 소켓 연산을 위한<br/>재사용 가능한 버퍼 풀 생성
    Server->>SocketAsyncEventArgsPool: 2. 이벤트 아규먼트 풀 초기화
    Note over SocketAsyncEventArgsPool: read/write 작업을 위한<br/>SocketAsyncEventArgs 객체 풀 생성
    Server->>Server: 3. 연결 제한 설정
    Note over Server: Semaphore를 사용하여<br/>최대 연결 수 제어
```
  
2. 클라이언트 연결 처리  
```mermaid
sequenceDiagram
    participant Client
    participant Server
    participant Socket
    
    Client->>Server: 연결 요청
    Server->>Socket: AcceptAsync 호출
    
    alt 동기 완료
        Server->>Server: ProcessAccept 직접 호출
    else 비동기 완료
        Socket-->>Server: AcceptEventArg_Completed 콜백
        Server->>Server: ProcessAccept 호출
    end
    
    Note over Server: 1. 연결된 클라이언트 수 증가<br/>2. SocketAsyncEventArgs 풀에서 객체 가져옴<br/>3. 다음 수신 대기 시작
```
  
3. 데이터 송수신 처리  
```mermaid
sequenceDiagram
    participant Client
    participant Server
    participant Socket

    Socket-->>Server: IO_Completed 이벤트
    
    alt Receive 완료
        Server->>Server: ProcessReceive
        Note over Server: 1. 수신된 바이트 카운트<br/>2. 에코백을 위한 Send 요청
    else Send 완료
        Server->>Server: ProcessSend
        Note over Server: 1. 송신 결과 확인<br/>2. 다음 수신 대기
    end
```
  
#### 주요 특징:
1. 비동기 이벤트 기반 처리
   - AcceptAsync, ReceiveAsync, SendAsync 메소드를 사용
   - 콜백을 통한 비동기 완료 처리

2. 리소스 관리
   - BufferManager: 메모리 단편화 방지를 위한 버퍼 풀 관리
   - SocketAsyncEventArgsPool: 이벤트 아규먼트 객체 재사용
   - Semaphore: 동시 연결 수 제어

3. 에코 서버 기능
   - 수신된 데이터를 그대로 클라이언트에게 재전송
   - 연속적인 송수신 처리

4. 연결 관리
   - 클라이언트 연결 수 추적
   - 비정상 연결 해제 처리
   - 연결 해제 시 리소스 정리 및 재사용
  
이러한 구조를 통해 Server 클래스는 확장성이 높고 효율적인 비동기 소켓 서버를 구현합니다.
  





     
