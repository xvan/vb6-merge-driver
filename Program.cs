using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;

namespace vb6_merge_driver
{
    public class Program
    {
        public static int Main(string[] args)
        {
            string ours = args[0];
            string common = args[1];
            string theirs = args[2];
            string conflictMarker = args[3];
            string filepath = args[4];

            Console.WriteLine("Merging {0} {1} {2} {3} {4}", args);

            globals.initialize(filepath);

            var vbpOurs = createVbp(ours);
            var vbpCommon = createVbp(common);
            var vbpTheirs = createVbp(theirs);

            ProcessStep(vbpOurs, vbpTheirs, vbpCommon);
            ProcessStep(vbpTheirs, vbpOurs, vbpCommon);
            
            var merged = Merge(vbpOurs, vbpTheirs);

            vbpOurs.Write(merged);

            if (globals.VbpMode) return 0;

            vbpTheirs.Write(merged);

            Process git = new Process();
            git.StartInfo.FileName = "git";
            git.StartInfo.Arguments = String.Format("merge-file --marker-size={3} -L OURS -L BASE -L THEIRS {0} {1} {2}", args);
            git.Start();
            git.WaitForExit();

            return git.ExitCode;
        }

        private static Vbp createVbp(string filepath)
        {
            if (globals.VbpMode) {
                return new Vbp(filepath);
            }
            return new Ctl(filepath);
        }

        static IEnumerable<string> Merge(Vbp vbpOurs, Vbp vbpTheirs)
        {


            var oursIncluded = NumerateAndFilter(vbpOurs).ToList();
            var theirsIncluded = NumerateAndFilter(vbpTheirs);

            foreach (var element in theirsIncluded) {
                var objective = oursIncluded.Where(x => x.Item2.Section == element.Item2.Section && x.Item1 > element.Item1).FirstOrDefault();
                if (objective == null) {
                    objective = oursIncluded.Where(x =>  x.Item1 > element.Item1).FirstOrDefault();
                }
                if (objective == null)
                {
                    oursIncluded.Append(element);
                }
                else
                {
                    oursIncluded.Insert(oursIncluded.IndexOf(objective), element);
                }
            }

            return oursIncluded.Select(x => x.Item2.Line);
        }

        private static IEnumerable<Tuple<int, VbpLine>> NumerateAndFilter(Vbp vbp)
        {
            return vbp.Lines.Select((item, index) => new Tuple<int,VbpLine>(index,item) ).Where(x => x.Item2.mergestatsus == MergeStatus.included);
        }

        static void ProcessStep(Vbp vbpOurs, Vbp vbpTheirs, Vbp vbpCommon) {
            var unprocLines = vbpOurs.Lines.Where(ourLine => ! ourLine.processed);
            foreach (var ourLine in unprocLines)
            {
                if (ourLine.Line == "") {
                    ourLine.mergestatsus = MergeStatus.excluded;
                }
                else 
                { 
                    (var theirLine, var compareTheirs) = FindLine(ourLine, vbpTheirs);

                    if (compareTheirs == Comparison.different)
                    {
                        (var commonLine, var compareCommon) = FindLine(ourLine, vbpCommon);
                        if (compareCommon == Comparison.different)
                        {
                            ourLine.mergestatsus = MergeStatus.included;
                        }
                        else
                        {
                            ourLine.mergestatsus = MergeStatus.excluded;
                        }
                    }
                    else {

                        theirLine.processed = true;
                        theirLine.mergestatsus = MergeStatus.excluded;
                        ourLine.mergestatsus = MergeStatus.included;

                        if (compareTheirs == Comparison.modified) {
                            (var commonOurs, var compareCommonOurs) = FindLine(ourLine, vbpCommon);

                            if (compareCommonOurs == Comparison.equal) {
                                theirLine.mergestatsus = MergeStatus.included;
                                ourLine.mergestatsus = MergeStatus.excluded;
                            }
                        }
                    }
                }
                ourLine.processed = true;
            }
        }

        static (VbpLine, Comparison) FindLine(VbpLine queryLine, Vbp targetVbp)
        {
            var searchSpace = targetVbp.Lines.Where(targetLine => (!targetLine.processed) && (targetLine.Section == queryLine.Section));

            foreach (var targetLine in searchSpace)
            {
                var comparison = queryLine.Compare(targetLine);

                if (comparison != Comparison.different)
                    return (targetLine, comparison);
            }
            return (null, Comparison.different);
        }
  
    }
}
