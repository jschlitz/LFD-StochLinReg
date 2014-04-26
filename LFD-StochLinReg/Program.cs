using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFD_StochLinReg
{
  class Program
  {
    const int SAMPLE_SIZE = 100;

    static void Main(string[] args)
    {
      var sum = Enumerable.Repeat(1, 100)
        .Select(_=>DoExperiment())
        .Aggregate((acc, x) => new Tuple<double, double>(acc.Item1 + x.Item1, acc.Item2 + x.Item2));
      Console.WriteLine("Avg Ein = {0:n3}, Avg Eout = {1:n3}", sum.Item1 / 100.0, sum.Item2 / 100.0); 

      Console.ReadKey(true);
    }

    private static Tuple<double,double> DoExperiment()
    {
      var trueW = new Tuple<double, double>(Rand.NextDouble() * 2 - 1, Rand.NextDouble() * 2 - 1);
      var arr = GetSamples(trueW);

      var guessW1 = 0.0;
      var guessW2 = 0.0;
      var norm = 1.0;
      var count = 1;
      //epoch
      do
      {
        Shuffle(arr);
        var startGuessW1 = guessW1;
        var startGuessW2 = guessW2;
        foreach (var sample in arr)
        {
          Mutate(0.01, sample.Item1, sample.Item2, sample.Item3, ref guessW1, ref guessW2);
        }
        norm = Math.Sqrt(Sq(startGuessW1 - guessW1) + Sq(startGuessW2 - guessW2));
        count++;
      }
      while (norm > 0.01);
      var check = GetSamples(trueW);
      double eIn = arr.Select(t=>Err(t.Item1, t.Item2, t.Item3, guessW1 , guessW2)).Average();
      double eOut = check.Select(t => Err(t.Item1, t.Item2, t.Item3, guessW1, guessW2)).Average();//Where(x => x.Item3 * (guessW1 * x.Item1 + guessW2 * x.Item2) < 0).Count() / ((double)SAMPLE_SIZE);
      Console.WriteLine("norm = {0}, Ein={1:n3}, Eout={3:n3}, count={2}", norm, eIn, count, eOut);

      return new Tuple<double, double>(eIn, eOut);
    }

    private static Tuple<double, double, double>[] GetSamples(Tuple<double, double> trueW)
    {
      var arr = new Tuple<double, double, double>[SAMPLE_SIZE];
      //var trueW = new Tuple<double, double>(-0.5, 0.5);
      var yGetter = getGetY(trueW);
      for (int i = 0; i < SAMPLE_SIZE; i++)
        arr[i] = MakeSample(yGetter);
      return arr;
    }

    private static double Sq (double x)    { return x * x; }

    private static Tuple<double, double, double> MakeSample(Func<double, double, double> yGetter)
    {
      double x1 = Math.Round(Rand.NextDouble() * 2 - 1, 3);
      double x2 = Math.Round(Rand.NextDouble() * 2 - 1, 3);
      return new Tuple<double, double, double> (x1, x2, yGetter(x1, x2));
    }

    static Func<double, double, double> getGetY(Tuple<double, double> w)
    {
      return (double x1, double x2) =>
      {
        return (w.Item1 * x1 + x2 * w.Item2 > 0) ? 1.0 : -1.0; //???
      };
    }

    private static double Err(double x1, double x2, double y, double w1, double w2)
    {
      return Math.Log(1 + Math.Exp(-1 * y * (x1 * w1 + x2 * w2)));
    }


    private static Tuple<double, double> Step(double x1, double x2, double y,  double w1,  double w2)
    {
      //oops this isn't quite the same as the error function, is it?
      var denom = 1 + Math.Exp(y * (x1 * w1 + x2 * w2));
      return new Tuple<double, double>(x1 * y / denom, x2 * y / denom);

    }

    private static void Mutate(double eta, double x1, double x2, double y, ref double w1, ref double w2)
    {
      var w = Step(x1, x2, y, w1, w2);
      w1 += eta * w.Item1;
      w2 += eta * w.Item2;
      return;

      var denom = 1 + Math.Exp(y * (x1 * w1 + x2 * w2));
      w1 +=  eta * x1 * y / denom;
      w2 +=  eta * x2 * y / denom;
    }

    static Random Rand = new Random();
    static void Shuffle<T>(IList<T> arr)
    { 
      for (int i = 0; i < arr.Count; i++)
			{
        var tmp = arr[i];
        var j = Rand.Next(arr.Count - i) + i;
        arr[i] = arr[j];
        arr[j] = tmp;
			}
    }
  }
}
