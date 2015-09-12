using System;

namespace EyePaint
{
  /// <summary>
  /// Share the same Random instance throughout the application so that it is only seeded once.
  /// </summary>
  public class Random
  {
    static System.Random r = new System.Random();

    public static int Next(int minValue = 0, int maxValue = Int32.MaxValue)
    {
      return r.Next(minValue, maxValue);
    }

    public static double NextDouble()
    {
      return r.NextDouble();
    }
  }
}
