namespace PvPGameServer.Rooms
{
    public enum UserState
    {
        IDLE = 0,
        READY = 1,
        INGAME = 2,
    }
	
    public class User
    {
        public string UserID { get; private set; }
        public string NetSessionID { get; private set; }
        public UserState State { get; set; }
        public Mok UserMok { get; set; }
        
        
        public void Init(string userID, string netSessionID)
        {
            UserID = userID;
            NetSessionID = netSessionID;
            
            State = UserState.IDLE;
            UserMok = Mok.None;
        }

        public void StartOmok(Mok mok)
        {
            State = UserState.INGAME;
            UserMok = mok;
        }
        
        public void EndOmok()
        {
            State = UserState.IDLE;
            UserMok = Mok.None;
        }
    }
}