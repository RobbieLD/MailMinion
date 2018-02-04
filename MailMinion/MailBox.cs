namespace MailMinion
{
    public class MailBox
    {
        public string Html { get; set; }
        public int MessageCount { get; set; }
        public int ErrorCount { get; set; }
        public int IgnoreCount { get; set; }
        public string Name { get; private set; }

        public MailBox(string name)
        {
            Name = name;
        }

        public string GetSummary()
        {
            return string.Format("{0} Summary -> Total Messages: {1}, Totall Successful Messages: {2}, Total Errors: {3}, Total Ignored: {4}", 
                Name, MessageCount, MessageCount - (ErrorCount + IgnoreCount), ErrorCount, IgnoreCount);
        }
    }
}
