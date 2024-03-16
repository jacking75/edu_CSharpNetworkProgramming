FreeNetLite - .Net Core 
=========
원 버전은 닷넷 3.5 지원인데 .Net Core 지원으로 바꾼 것이다.  
FreeNet은 .Net Standard 2.0 버전이고, SampleServer는 .Net Core 2.2 버전이다.  
(그리고 코드 리팩토링도 하고 있다)    
   
**현재 대규모 수정 중이다. 안정성 테스트를 하지 못했다(2017-10-25)**     
   
=========   
C# Network library. Asynchronous. TCP. GameServer.

Version
----------
* v0.1.0 Heartbeat
* v0.0.1

프로젝트 정보
----------
* C# 비동기 네트워크 라이브러리.
* 게임 서버에서 사용할 수 있는 TCP기반의 socket server.
* .Net Core 2.2 사용
* Unity 연동 가능
  
  
Sample Game
----------
![viruswar](https://github.com/sunduk/FreeNet/blob/master/viruswar/client/doc/screenshot.png?raw=true)
* FreeNet라이브러리를 활용하여 Unity로 만든 온라인 멀티플레이 보드 게임 세균전.
* The VirusWar that online multiplay board game sample developed using FreeNet and Unity.

아키텍처 및 구조   
----------
![structure](https://github.com/sunduk/FreeNet/blob/master/documents/struct.png?raw=true)
![class structure](https://github.com/sunduk/FreeNet/blob/master/documents/class_struct.png?raw=true)
![worker](https://github.com/sunduk/FreeNet/blob/master/documents/worker_thread.png?raw=true)
![logic](https://github.com/sunduk/FreeNet/blob/master/documents/logic_thread.png?raw=true)
![send](https://github.com/sunduk/FreeNet/blob/master/documents/send.png?raw=true)

Structure
----------
![structure](https://github.com/sunduk/FreeNet/blob/master/documents/struct_en.png?raw=true)
![class structure](https://github.com/sunduk/FreeNet/blob/master/documents/class_struct_en.png?raw=true)
![worker](https://github.com/sunduk/FreeNet/blob/master/documents/worker_thread_en.png?raw=true)
![logic](https://github.com/sunduk/FreeNet/blob/master/documents/logic_thread_en.png?raw=true)
![send](https://github.com/sunduk/FreeNet/blob/master/documents/send_en.png?raw=true)


라이선스
----------
* 소스코드는 상업적, 비상업적 어느 용도이든 자유롭게 사용 가능 합니다.

License
----------
* All source codes are free to use(Commercial use is possible).
