namespace OmokAPIServer.Enums
{
    public enum ResultCode
    {
        Success = 0,
        SessionNotExist = 1,
        SessionTimeout = 2,
        InvalidAuthToken = 3,
        InvalidNickname = 4,
        UserNotExist = 5,
        MatchingFail = 6,
        
        DBCommonError = 101,
        DBInsertError = 102,
        DBUpdateError = 103,
        
        CacheSetError = 201,
        CacheAddListError = 202
    }
}