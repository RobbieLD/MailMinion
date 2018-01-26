using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MailMinion.Models;

namespace MailMinion.Tests
{
    [TestClass]
    public class MailBoxCreatorTests
    {
        [TestMethod]
        public void Test()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestRun()
        {
            // Arrange
            var mockFileService = new Mock<IFileService>();

            mockFileService.Setup(x => x.IsImage(It.IsAny<string>())).Returns(true);
            mockFileService.Setup(x => x.SaveAttachment(It.IsAny<MimeKit.MimePart>(), It.IsAny<int>(), It.IsAny<int>())).Returns("test.jpg");
            mockFileService.Setup(x => x.IgnoreList).Returns(new List<string>());
            mockFileService.Setup(x => x.TemplateDirectory).Returns("Z:\\Projects\\MailMinion\\MailMinion\\Views");

            string mboxString = "From MAILER-DAEMON Fri Jul  8 12:08:34 2011\n" +
                                "From: Author < author@example.com >\n" +
                                "To: Recipient < recipient@example.com >\n" +
                                "Subject: Sample message 1\n" +
                                "\n" +
                                "This is the body.\n" +
                                "> From(should be escaped).\n" +
                                "There are 3 lines.\n" +
                                "\n" +
                                "From MAILER-DAEMON Fri Jul  8 12:08:34 2011\n" +
                                "From: Author < author@example.com >\n" +
                                "To: Recipient < recipient@example.com >\n" +
                                "Subject: Sample message 2\n" +
                                "\n" +
                                "This is the second body.\n";

            // Create Text Stream for example parsing
            byte[] byteArray = Encoding.ASCII.GetBytes(mboxString);
            MemoryStream stream = new MemoryStream(byteArray);

            mockFileService.Setup(x => x.GetMailStream(It.IsAny<string>())).Returns(stream);

            MailBoxCreator mailBoxCreator = new MailBoxCreator(mockFileService.Object);

            // Act
            MailBox mailBox = mailBoxCreator.Create("test", new List<Tab>()
            {
                new Tab()
                {
                    Name = "TestTab",
                    Url = "TestUrl"
                }
            });

            // Assert
            Assert.IsNotNull(mailBox.Html);
        }
    }
}
