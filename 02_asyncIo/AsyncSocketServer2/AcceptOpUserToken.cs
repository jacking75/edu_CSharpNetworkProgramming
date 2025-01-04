using System;


namespace AsyncSocketServer2;

class AcceptOpUserToken
{
    // 앱에서 이 UserToken을 사용하는 유일한 이유는 식별자를 부여하여 프로그램 흐름에서 확인할 수 있게 하기 위함입니다.
    // 그렇지 않으면 필요하지 않을 것입니다.


    private Int32 id; // 테스트용으로만 사용됩니다.
    internal Int32 socketHandleNumber; // 테스트용으로만 사용됩니다.

    public AcceptOpUserToken(Int32 identifier)
    {
        id = identifier;


        //if (Program.watchProgramFlow == true)   // 테스트용
        //{
        //    Program.testWriter.WriteLine("AcceptOpUserToken constructor, idOfThisObject " + id);
        //}
    }

    public Int32 TokenId
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }
}
