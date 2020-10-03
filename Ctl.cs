using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace vb6_merge_driver
{
    public class Ctl : Vbp
    {

        const string startMarker = "Begin VB";
        public List<string> Preamble { get; }
        public List<string> Body {get;}

        public Ctl(string filename)
        {
            Filename = filename;

            OriginalLines = File.ReadLines(filename).Select(x =>x.TrimEnd()).ToList();
            int v = OriginalLines.FindIndex(x => x.Substring(0, startMarker.Length) == startMarker);            

            Preamble = OriginalLines.Take(v).ToList();
            Body = OriginalLines.Skip(v).ToList();

            vbpLines = Preamble.Select(x => new VbpLine(x)).ToList();            
        }

        public override void Write(IEnumerable<string> merged)
        {
            //only overwrite if changes were found
            //if (!merged.Except(this.Preamble).Any() && !this.Preamble.Except(merged).Any())                
                File.WriteAllLines(Filename, merged.Concat( Body ) );
        }
    }
}
