using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laster40Net
{
    //TODO 커맨드 라인 인수로 설정 정보를 받는 기능 추가 구현하기

    /// <summary>
    /// TCP Service 설정
    /// 
    /// history
    /// 2012-12-17 : 커넥션 옵션 추가 ( 재 접속, 재 시도 )
    /// 2012-12-20 : bugfix
    ///              Recevie 시도전에 이미 접속 종료 발생시 접속 종료 처리가 되지 않는 버그 수정
    /// </summary>
    /// 
    public class TcpServiceConfig
    {
        /// <summary>
        /// 패킷 받을 버퍼의 사이즈
        /// </summary>
        public int ReceviceBuffer;
        
        /// <summary>
        /// 보낼 패킷 버퍼의 사이즈
        /// </summary>
        public int SendBuffer;
        /// <summary>
        
        /// 세션당 보낼수 있는 버퍼의 갯수
        /// </summary>
        public int SendCount;
        
        /// <summary>
        /// 최대 허용 접속자수
        /// </summary>
        public int MaxConnectionCount;
       
        /// <summary>    
         /// 세션 업데이트 Intval
        /// </summary>
        public int UpdateSessionIntval;

        /// <summary>
        /// 세션 Receive Timeout
        /// </summary>
        public int SessionReceiveTimeout;

        /// <summary>
        /// 메세지 팩토리 어셈블리 이름
        /// </summary>
        public string MessageFactoryAssemblyName;

        /// <summary>
        /// 메세지 팩토리 타입이름
        /// </summary>
        public string MessageFactoryTypeName;

        /// <summary>
        /// 로그를 어떤식으로 남길것인지
        /// </summary>
        public string Log;

        /// <summary>
        /// 로그를 어떤식으로 남길것인지
        /// </summary>
        public string LogLevel;

        /// <summary>
        /// 접속을 받을 Listener 설정
        /// </summary>
         


        public struct ListenerConfig
        {
            /// <summary>
            /// ip
            /// </summary>
            public string ip;
            /// <summary>
            /// port
            /// </summary>
            public int port;
            /// <summary>
            /// backlog
            /// </summary>
            public int backlog;
            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="backlog"></param>
            public ListenerConfig(string ip, int port, int backlog)
            {
                this.ip = ip;
                this.port = port;
                this.backlog = backlog;
            }
        }


        /// <summary>
        /// 리스너 설정
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute("Listener", IsNullable = false)]
        public ListenerConfig[] Listeners;


        /// <summary>
        /// 접속할 클라이언트 설정
        /// </summary>
        public struct ClientConfig
        {
            /// <summary>
            /// ip
            /// </summary>
            public string ip;
            /// <summary>
            /// port
            /// </summary>
            public int port;
            /// <summary>
            /// 접속 초과 시간( 이후에 실패! )
            /// </summary>
            public int timeout;
            /// <summary>
            /// 재시도 횟수, 0이면 무한
            /// </summary>
            public int retry;
            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="timeout"></param>
            /// <param name="retry"></param>
            public ClientConfig(string ip, int port, int timeout, int retry)
            {
                this.ip = ip;
                this.port = port;
                this.timeout = timeout;
                this.retry = retry;
            }
        }

        /// <summary>
        /// 클라이언트(원격에 접속을 시도할)
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute("Client", IsNullable = false)]
        public ClientConfig[] Clients;
    }    
}
