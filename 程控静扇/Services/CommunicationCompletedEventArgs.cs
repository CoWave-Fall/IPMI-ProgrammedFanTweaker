using System;

namespace 程控静扇.Services
{
    public class CommunicationCompletedEventArgs : EventArgs
    {
        public bool IsSuccess { get; } 

        public CommunicationCompletedEventArgs(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}