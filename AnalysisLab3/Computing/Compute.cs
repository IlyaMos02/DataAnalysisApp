using AnalysisLab3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisLab3.Computing
{
    internal class Compute
    {
        double alpha = 0.05;
        string format = "F4";
        //Characteristic
        public double getSquareMean(List<double> content)
        {
            var sum = 0.0;
            foreach (var item in content)
                sum += Math.Pow(item, 2);
            return sum / content.Count;
        }
        public double getMean(List<double> content)
        {
            var sum = 0.0;
            foreach (var item in content)
                sum += item;
            return sum / content.Count;
        }
        public double getShiftedVariance(List<double> content)
        {
            return getSquareMean(content) - Math.Pow(getMean(content), 2);
        }
        public double getUnshiftedVariance(List<double> content)
        {
            return ((double)content.Count / ((double)content.Count - 1)) * getShiftedVariance(content);
        }
        public double getStandartDeviation(List<double> content)
        {
            return Math.Sqrt(getUnshiftedVariance(content));

        }

        public double getMedian(List<double> content)
        {
            content.Sort();

            if (content.Count % 2 == 0)
            {
                double num1 = content[(int)content.Count / 2];
                double num2 = content[(int)content.Count / 2 + 1];

                return 0.5 * (num1 + num2);
            }
            else
            {
                return content[(int)((content.Count + 1) / 2)];
            }
        }
        private double getFirstCoefficientOfSkewness(List<double> content)
        {
            var pre = 1.0 / (content.Count * Math.Pow(getShiftedVariance(content), 1.5));
            var sum = 0.0;

            foreach (var item in content)
            {
                sum += Math.Pow(item - getMean(content), 3);
            }

            return pre * sum;
        }
        public double getSecondCoefficientOfSkewness(List<double> content)
        {
            var num1 = Math.Sqrt(content.Count * (content.Count - 1));
            var num2 = num1 / (content.Count - 2);

            return num2 * getFirstCoefficientOfSkewness(content);
        }
        private double getFirstCoefficientOfKurtosis(List<double> content)
        {
            var pre = 1.0 / (content.Count * Math.Pow(getShiftedVariance(content), 2));
            var sum = 0.0;

            foreach (var item in content)
            {
                sum += Math.Pow(item - getMean(content), 4);
            }

            return (pre * sum) - 3;
        }
        public double getSecondCoefficientOfKurtosis(List<double> content)
        {
            var pre = (Math.Pow(content.Count, 2) - 1) / ((content.Count - 2) * (content.Count - 3));
            var suf = getFirstCoefficientOfKurtosis(content) + (6.0 / (content.Count + 1));

            return pre * suf;
        }

        public (double, double, bool) IsNormalDist(List<double> content)
        {
            double skew = getSecondCoefficientOfSkewness(content)/ getStandartDeviationSecondSkewness(content);
            double kurt = getSecondCoefficientOfKurtosis(content)/ getStandartDeviationSecondKurtosis(content);
            bool isNorm = (Math.Abs(skew) < getStudent((1.0 - alpha / 2.0), content.Count - 1)) && (Math.Abs(kurt) < getStudent((1.0 - alpha / 2.0), content.Count - 1));

            return (Math.Round(skew, 4), Math.Round(kurt, 4), isNorm);
        }

        private double getStandartDeviationSecondSkewness(List<double> content)
        {
            double N = content.Count;
            double num = ((6 * N) * (N - 1.0)) / ((N + 1.0) * (N + 3.0) * (N - 2.0));

            return Math.Sqrt(num);
        }

        private double getStandartDeviationSecondKurtosis(List<double> content)
        {
            double N = content.Count;
            double num = ((24 * N) * Math.Pow(N - 1.0, 2)) / ((N + 5.0) * (N + 3.0) * (N - 2.0) * (N - 3.0));

            return Math.Sqrt(num);
        }
        public string getMeanInterval(List<double> content)
        {
            double N = content.Count;

            var min = getMean(content) - getStudent(1.0 - alpha, content.Count - 1) * getMeanStandartDeviation(content);
            var max = getMean(content) + getStudent(1.0 - alpha, N - 1) * getMeanStandartDeviation(content);

            return min.ToString(format) + ";" + max.ToString(format);
        }

        private double getMeanStandartDeviation(List<double> content)
        {
            return getStandartDeviation(content) / Math.Sqrt(content.Count);
        }

        public string getMedianInterval(List<double> content)
        {
            content.Sort();

            double N = content.Count;
            var minIdx = (N / 2.0) - getNormal(1.0 - alpha / 2.0) * (Math.Sqrt(N) / 2.0);
            int j = (int)minIdx;
            var maxIdx = N / 2.0 + 1 + getNormal(1.0 - alpha / 2.0) * (Math.Sqrt(N) / 2.0);
            int k = (int)maxIdx;

            return content[j].ToString(format) + ";" + content[k].ToString(format);
        }

        //Quantils
        public double getNormal(double p)
        {
            if (p <= 0.5)
            {
                return -getFi(p);
            }
            else
            {
                return getFi(1 - p);
            }
        }
        private double getFi(double p)
        {
            double c0 = 2.515517, c1 = 0.802853, c2 = 0.010328;
            double d1 = 1.432788, d2 = 0.1892659, d3 = 0.001308;
            double up = c0 + c1 * getT(p) + c2 * Math.Pow(getT(p), 2);
            double down = 1 + d1 * getT(p) + d2 * Math.Pow(getT(p), 2) + d3 * Math.Pow(getT(p), 3);

            return getT(p) - (up / down);
        }
        private double getT(double p)
        {
            return Math.Sqrt(-2 * Math.Log(p));
        }
        public double getStudent(double p, double v)
        {
            var num = getNormal(p) + (1.0 / v) * getG1(getNormal(p)) + (1.0 / Math.Pow(v, 2)) * getG2(getNormal(p)) + (1.0 / Math.Pow(v, 3)) * getG3(getNormal(p)) + (1.0 / Math.Pow(v, 4)) * getG4(getNormal(p));

            return num;
        }
        private double getG1(double norm)
        {
            var num = 0.25 * (Math.Pow(norm, 3) + norm);
            return num;
        }
        private double getG2(double norm)
        {
            var num = (1.0 / 96.0) * (5 * Math.Pow(norm, 5) + 16 * Math.Pow(norm, 3) + 3 * norm);
            return num;
        }
        private double getG3(double norm)
        {
            var num = (1.0 / 384.0) * (3 * Math.Pow(norm, 7) + 19 * Math.Pow(norm, 5) + 17 * Math.Pow(norm, 3) - 15 * norm);
            return num;
        }
        private double getG4(double norm)
        {
            var num = (1.0 / 92160.0) * (79 * Math.Pow(norm, 9) + 779 * Math.Pow(norm, 7) + 1482 * Math.Pow(norm, 5) - 1920 * Math.Pow(norm, 3) - 945 * norm);
            return num;
        }

        public double getFisher(double p, double v1, double v2)
        {
            double first = getNormal(p) * Math.Sqrt(getSigma(v1, v2) / 2.0) - (1.0 / 6.0) * getDelta(v1, v2) * (Math.Pow(getNormal(p), 2) + 2.0);
            double secondFirst = (getSigma(v1, v2) / 24.0) * (Math.Pow(getNormal(p), 2) + 3.0 * getNormal(p));
            double secondSecond = (1.0 / 72.0) * (Math.Pow(getDelta(v1, v2), 2) / getSigma(v1, v2)) * (Math.Pow(getNormal(p), 3) + 11.0 * getNormal(p));
            double second = Math.Sqrt(getSigma(v1, v2) / 2.0) * (secondFirst + secondSecond);
            double thirdFirst = (Math.Pow(getNormal(p), 4) + 9.0 * Math.Pow(getNormal(p), 2) + 8.0);
            double third = ((getSigma(v1, v2) * getDelta(v1, v2)) / 120.0) * thirdFirst;
            double fourthFirst = (3.0 * Math.Pow(getNormal(p), 4) + 7.0 * Math.Pow(getNormal(p), 2) - 16.0);
            double fourth = (Math.Pow(getDelta(v1, v2), 3) / (3240.0 * getSigma(v1, v2))) * fourthFirst;
            double fivethFirstFirst = (Math.Pow(getNormal(p), 5) + 20.0 * Math.Pow(getNormal(p), 3) + 15.0 * getNormal(p));
            double fivethFirst = (Math.Pow(getSigma(v1, v2), 2) / 1920.0) * fivethFirstFirst;
            double fiveSecondFirst = (Math.Pow(getNormal(p), 5) + 44.0 * Math.Pow(getNormal(p), 3) + 183.0 * getNormal(p));
            double fiveSecond = (Math.Pow(getDelta(v1, v2), 4) / 2880.0) * fiveSecondFirst;
            double fiveThirdFirst = (9.0 * Math.Pow(getNormal(p), 5) - 284.0 * Math.Pow(getNormal(p), 3) - 1513.0 * getNormal(p));
            double fiveThird = (Math.Pow(getDelta(v1, v2), 4) / (155520.0 * getSigma(v1, v2))) * fiveThirdFirst;
            double five = Math.Sqrt(getSigma(v1, v2) / 2.0)*(fivethFirst + fiveSecond + fiveThird);

            double result = first + second - third + fourth + five;

            return Math.Exp(2*result);
        }
        private double getSigma(double v1, double v2)
        {
            return 1.0/v1 + 1.0/v2;
        }
        private double getDelta(double v1, double v2)
        {
            return 1.0/v1 - 1.0/v2;
        }

        //Equality
        private double WeightedMean(Dictionary<string, List<double>> kvp)
        {
            string[] keys = kvp.Keys.ToArray();

            double up = (kvp[keys[0]].Count - 1.0) * getUnshiftedVariance(kvp[keys[0]]) + (kvp[keys[1]].Count - 1.0) * getUnshiftedVariance(kvp[keys[1]]);
            double down = kvp[keys[0]].Count + kvp[keys[1]].Count - 2.0;

            return up / down;
        }

        public double IndepCriteriaMean(Dictionary<string, List<double>> kvp)
        {
            string[] keys = kvp.Keys.ToArray();

            double up = getMean(kvp[keys[0]]) - getMean(kvp[keys[1]]);
            double down = Math.Sqrt(WeightedMean(kvp) / kvp[keys[0]].Count + WeightedMean(kvp) / kvp[keys[1]].Count);

            return up / down;
        }

        public double IndepCriteriaMeanNegative(Dictionary<string, List<double>> kvp)
        {
            string[] keys = kvp.Keys.ToArray();

            double up = getMean(kvp[keys[0]]) - getMean(kvp[keys[1]]);
            double down = Math.Sqrt(getUnshiftedVariance(kvp[keys[0]]) / kvp[keys[0]].Count + getUnshiftedVariance(kvp[keys[1]]) / kvp[keys[1]].Count);

            return up / down;
        }

        public double getIndepStudentV(Dictionary<string, List<double>> kvp)
        {
            string[] keys = kvp.Keys.ToArray();

            double first = getUnshiftedVariance(kvp[keys[0]]) / kvp[keys[0]].Count + getUnshiftedVariance(kvp[keys[1]]) / kvp[keys[1]].Count;
            double second = (1.0 / (kvp[keys[0]].Count - 1.0)) * Math.Pow(getUnshiftedVariance(kvp[keys[0]]) / kvp[keys[0]].Count, 2);
            double third = (1.0 / (kvp[keys[1]].Count - 1.0)) * Math.Pow(getUnshiftedVariance(kvp[keys[1]]) / kvp[keys[1]].Count, 2);

            double result = Math.Pow(first, 2) * Math.Pow(second + third, -1);

            return result;
        }

        public double DepCriteriaMean(Dictionary<string, List<double>> kvp)
        {
            return (getMean(kvp["z"]) * Math.Sqrt(kvp["z"].Count)) / getStandartDeviation(kvp["z"]);
        }

        //Rank
        public double DepRank(Dictionary<string, List<double>> kvp)
        {
            List<DepRankModel> z = new List<DepRankModel>();                        
            foreach(var num in kvp["z"])
            {
                if(num != 0)
                {
                    int alfa;
                    if (num > 0)
                        alfa = 1;
                    else
                        alfa = 0;

                    z.Add(new DepRankModel(Math.Abs(num), alfa, 0));
                }                                      
            }

            z.Sort(delegate (DepRankModel x, DepRankModel y)
            {
                return x.Numb.CompareTo(y.Numb);
            });

            for (int i = 0; i < z.Count; i++) 
            {
                z[i].Rank = i + 1;
            }    

            List<DepRankModel> copy = new List<DepRankModel>(z);
            foreach(var item in copy)
            {
                int count = z.Count(x => x.Numb == item.Numb);
                if(count > 1)
                {
                    double sum = z.Where(x => x.Numb == item.Numb).Sum(x => x.Rank);
                    foreach (var num in z.Where(x => x.Numb == item.Numb))
                        num.Rank = sum / count;
                }
            }            

            return (getDepT(z) - getDepE(z)) / Math.Sqrt(getDepD(z));
        }
        private double getDepT(List<DepRankModel> z)
        {
            double sum = 0;

            foreach(var item in z)
            {
                if (item.Alfa == 1)
                    sum += item.Alfa * item.Rank;
            }

            return sum;
        }
        private double getDepE(List<DepRankModel> z)
        {
            return (1.0 / 4.0) * z.Count * (z.Count + 1);
        }
        private double getDepD(List<DepRankModel> z)
        {
            return (1.0 / 24.0) * z.Count * (z.Count + 1) * (2 * z.Count + 1);
        }

        public double IndepRank(Dictionary<string, List<double>> kvp)
        {
            List<IndepRankModel> model = new List<IndepRankModel>();

            int i = 0;
            string[] keys = kvp.Keys.ToArray();

            foreach (var numb in kvp.First().Value)
            {
                model.Add(new IndepRankModel(numb));

                model[i].Inversions = kvp[keys[1]].Count(y => y < numb) * 1.0 + kvp[keys[1]].Count(y => y == numb) * 0.5;
                i++;
            }

            double v = model.Sum(x => x.Inversions);

            return (v - getIndepE(kvp)) / Math.Sqrt(getIndepD(kvp));
        }
        private double getIndepE(Dictionary<string, List<double>> kvp) 
        {
            string[] keys = kvp.Keys.ToArray();
            return 0.5 * kvp[keys[0]].Count * kvp[keys[1]].Count;
        }
        private double getIndepD(Dictionary<string, List<double>> kvp)
        {
            string[] keys = kvp.Keys.ToArray();
            return (1.0 / 12.0) * kvp[keys[0]].Count * kvp[keys[1]].Count * (kvp[keys[0]].Count + kvp[keys[1]].Count + 1);
        }
    }
}
