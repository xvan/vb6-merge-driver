using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace vb6_merge_driver
{
    public enum MergeStatus { included, excluded }

    public enum Novelty
    {
        common,
        novel,
        modified
    }

    public enum Comparison
    {
        equal,
        different,
        modified
    }

    public class BaseLine { 
    
    }

    public class VbpLine : BaseLine
    {
        protected string _line;
        protected string _section;
        string _content;

        string _contentSeparator;
        string[] _contentSplit;
               
        public Novelty novelty;
        public bool processed;

        public MergeStatus mergestatsus;

        protected VbpLine() { }
        public VbpLine(string line) {
            processed = false;
            _line = line;
            
            var x = _line.Split("=", 2);
            _section = x[0];
            _content = x.Skip(1).FirstOrDefault();

            switch (_section) {

                case "Reference":                
                    parseRef();
                    break;
                case "Object":
                    parseObj();
                    break;
                case "Module":
                case "Class":
                case "Form":
                case "UserControl":
                    //TODO: se podria separar el ";" y hacer algo as cheto;

                default:
                    //TODO: Desambiguar quoted parameters;
                    break;
            }
        }

        internal Comparison Compare(VbpLine them)
        {
            if (them.Line == this.Line) return Comparison.equal;

            if (them.Section != this.Section) return Comparison.different;

            switch (this.Section)
            {
                case "Object":
                case "Reference":                
                    //System.Diagnostics.Debug.WriteLine("{0}\n{1}", this._content, them._content);
                    if (them._contentSplit[1] != this._contentSplit[1])
                        return Comparison.different;
                    if (them._contentSplit[2] != this._contentSplit[2])
                        return Comparison.different;
                    if (!PathEquals(them._contentSplit[3], this._contentSplit[3]))
                        return Comparison.different;                
                    break;
                case "Module":
                case "Class":
                case "Form":
                case "UserControl":
                    return Comparison.different;
                    
                default:
                    if (them._content.Trim('"') != this._content.Trim('"'))
                        return Comparison.modified;
                    break;
            }            
            return Comparison.equal;
        }

        private bool PathEquals(string v1, string v2)
        {
            
            var p1 = Path.GetFullPath( Path.Combine(globals.RootDir,v1), Directory.GetCurrentDirectory());
            var p2 = Path.GetFullPath(Path.Combine(globals.RootDir, v1), Directory.GetCurrentDirectory());

            bool rval =  string.Equals(p1, p2,StringComparison.InvariantCultureIgnoreCase);


            if (!rval) {
                return rval;
            }
            return rval;
        }

        private void parseRef()
        {
            _contentSeparator = "#";
            _contentSplit = _content.Split(_contentSeparator);
        }

        private void parseObj()
        {
            _contentSeparator = "#;";
            _contentSplit = _content.Split(_contentSeparator.ToCharArray());
        }

        public string Line => _line;
        public string Section => _section;
        public string Content => _content;
    }
}
