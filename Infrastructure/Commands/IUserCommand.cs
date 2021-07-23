using System;

namespace DAL.Commands
{
    public interface IUserCommand
    {
        void UnSubscribe(string userId, int courseId);

        void Subscribe(string userId, int courseId, DateTime startStudyDate);
    }
}