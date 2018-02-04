using System.Collections.Generic;
using System.IO;
using MimeKit;

namespace MailMinion
{
    public interface IFileService
    {
        IList<string> IgnoreList { get; set; }
        string Template { get; }

        Stream GetMailStream(string path);
        bool IsImage(string fileName);
        string SaveAttachment(MimePart attachment, int messageCount, int attachmentCount, string folderName);
        void SaveEmail(string fileName, string content, int messageCount);
        void SaveMailBox(MailBox mailBox, string fileName);
        string GetAttachmentPath(string attachmentName, string folderName);
        string GetAttachmentName(string path);
    }
}