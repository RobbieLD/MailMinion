using System.Collections.Generic;

namespace MailMinion
{
    public interface IMailBoxCreator
    {
        void Run(string path, IList<string> folders);
    }
}