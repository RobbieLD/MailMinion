using System.Collections.Generic;
using MailMinion.Models;

namespace MailMinion
{
    public interface IMailBoxCreator
    {
        MailBox Create(string path, IList<Tab> tabs);
    }
}