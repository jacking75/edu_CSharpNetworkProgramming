using System;
using FreeNet;

namespace SampleServer
{	
	/// <summary>
	/// 하나의 session객체를 나타낸다.
	/// </summary>
	class GameUser
	{
		Session Session;

		public GameUser(Session session)
		{
			Session = session;
		}		
	}
}
