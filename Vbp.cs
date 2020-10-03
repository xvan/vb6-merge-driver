using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace vb6_merge_driver
{
    public class Vbp
    {
        public List<string> OriginalLines { get; protected set; }
        protected List<VbpLine> vbpLines;
        public string Filename { get; protected set; }


        public List<VbpLine> Lines => vbpLines;

        protected Vbp() { }
        public Vbp(string filename) {
            Filename = filename;
            OriginalLines = File.ReadLines(filename).Select(x => x.TrimEnd() ).ToList();            
            vbpLines = OriginalLines.Select(x => new VbpLine(x)).ToList();
        }

        public virtual void Write(IEnumerable<string> merged)
        {
            //only overwrite if changes were found
            //if (!merged.Except(this.OriginalLines).Any() && !this.OriginalLines.Except(merged).Any())
                File.WriteAllLines(Filename, merged);
        }
    }
}
