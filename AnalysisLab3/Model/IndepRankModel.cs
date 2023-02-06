using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisLab3.Model
{
    internal class IndepRankModel
    {
        public double Numb { get; set; }
        public double Inversions { get; set; }

        public IndepRankModel(double numb)
        {
            Numb = numb;
        }
    }
}
