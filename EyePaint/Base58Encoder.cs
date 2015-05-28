using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyePaint
{
  class Base58Encoder
  {
    public static String alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
    public static String Encode(ulong n)
    {
      String encoded = "";
      ulong l = (ulong) alphabet.Length;

      while (n > 0)
      {
        var r = n % l;
        n = n / l;
        encoded = alphabet.ElementAt((int)r) + encoded;
      }

      return encoded;
    }

    public static ulong Decode(String s)
    {
      ulong n = 0;
      ulong t = 1;
      ulong l = (ulong)alphabet.Length;

      while (s.Length > 0)
      {
        String current = s.Substring(s.Length - 1);
        n = n + (t * (ulong)alphabet.IndexOf(current));
        t = t * l;
        s = s.Substring(0, s.Length - 1);
      }

      return n;
    }
  }
}
