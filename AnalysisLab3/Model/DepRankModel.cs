using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisLab3.Model
{
    internal class DepRankModel
    {
        public double Numb { get; set; }
        public double Alfa { get; set; }
        public double Rank { get; set; }

        public DepRankModel(double numb, double alfa, double rank) 
        {
            Numb = numb;
            Alfa = alfa;
            Rank = rank;
        }
    }
}
