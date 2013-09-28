using System.IO;
using System.Windows.Forms;

namespace CSharpinator
{
    public class ClipboardWriter : StringWriter
    {
        public override void Close()
        {
            base.Close();
            Clipboard.SetText(GetStringBuilder().ToString());
        }

        public override void Flush()
        {
            base.Flush();
            Clipboard.SetText(GetStringBuilder().ToString());
        }
    }
}
