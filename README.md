# com2usStudy_CSharpNetworkProgramming
컴투스 C# 네트워크 프로그래밍 학습
  
  
이 저장소에 있는 것을 다 공부했다면 [FreeNetLite ](https://github.com/jacking75/FreeNetLite)에 있는 것을 분석 후 리팩토링하고, 예제 서버를 만들어본다.  
  
  
## 고성능 네트워크 관련 글
  
###.NET Framework의 비동기 네트워크 라이브러리 성능 개선
- 닷넷은 현재 버전 4.5가 나왔다.(2013.04.05) 
    - 성장한 만큼 많은 개선과 기능 추가가 있었다. 그러나 프로그래머들이 닷넷의 성장을 따라가지 못하는 경우가 많다.
    - 닷넷을 메인으로 사용하지 않는 경우는 당연하고, 주위의 닷넷 프로그래머들의 이야기를 들어보면 닷넷 프로그래머들도 최신 기술을 모르던가 사용하지 않는 경우가 많다고한다. 
    - 대부분 2.0 대의 기술을 자주 사용한다고 한다.
- 닷넷으로 고성능 네트웍 프로그래밍을 하기 위해 자료를 찾아보면 이전 방식을 사용한 경우가 많다.
    - 혹시 아래 소개한 것을 본적이 없다면 꼭 이것을 보고 닷넷으로 서버 프로그램을 개발하기 바란다(물론 궂이 고성능을 필요로 하지 않으면 기존 방식대로 해도 좋다)
  
  
###SocketAsyncEventArgs
- 닷넷에서 고성능 네트웍 프로그래밍을 하기 위해서는 필연적으로 3.5에서 추가된 SocketAsyncEventArgs 클래스에 대해서 알아야 한다.
    - http://msdn.microsoft.com/ko-kr/library/system.net.sockets.socketasynceventargs.aspx
- 이것을 사용한 샘플은
    - http://archive.msdn.microsoft.com/nclsamples/Wiki/View.aspx?title=Socket%20Performance&referringTitle=Home
- 샘플을 보면 알겠지만 고성능을 내기 위해서 우리가 지금까지 일반적으로 해왔던 동적 메모리 사용 대신 
    - 메모리풀 및 정적 메모리 사용을 닷넷 네트워크 프로그래밍에서 어떻게 적용하는지 나타내고 있다. 
    - 닷넷은 기본적으로 동적 메모리 할당이 네이티브 보다는 좋겠지만 그래도 메모리 재사용 보다는 성능 측면에서 좋을 수가 없고, 
    - 또 가비지 컬렉션에 대한 부담이 있다.
- CodeProject읭 'C# SocketAsyncEventArgs High Performance Socket Code'강좌
    - 위의 예제보다 더 자세하고 부족한 부분을 추가해서 잘 설명하고 있다. 강추 
    - http://www.codeproject.com/Articles/83102/C-SocketAsyncEventArgs-High-Performance-Socket-Cod
  
  
###High performance asynchronous awaiting sockets
- http://stackoverflow.com/questions/17093206/high-performance-asynchronous-awaiting-sockets
- Converting Microsoft's Asynchronous Server Socket Sample to Async CTP
    - http://social.msdn.microsoft.com/Forums/en-US/aac79f64-5886-40f5-a8f1-a4a6f2460c85/converting-microsofts-asynchronous-server-socket-sample-to-async-ctp


  