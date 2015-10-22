using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    public class UserGenerator
    {

        private static UserGenerator _instance = new UserGenerator();
        private IEnumerator<Tuple<string, string>> _enumerator;
        public static UserGenerator Instance
        {
            get
            {
                return _instance;
            }
        }

        private UserGenerator()
        {
            _enumerator = GetLoginPasswordEnum().GetEnumerator();
        }

        public Tuple<string, string> GetLoginPassword()
        {
            if (_enumerator.MoveNext())
            {
                return _enumerator.Current;
            }
            else
            {
                throw new InvalidOperationException("Login repository depleted.");
            }


        }

        private string[] syllabs = new[] { "mi", "lil", "lae", "shyn", "cli", "re", "lonna", "mae", "ka", "ya", "lu", "win", "el", "rue", "tae" };

        private IEnumerable<Tuple<string, string>> GetLoginPasswordEnum()
        {

            for (int i = 0; i < syllabs.Length; i++)
            {
                for (int j = -1; j < syllabs.Length; j++)
                {
                    for (int k = -1; k < syllabs.Length; k++)
                    {
                        var s1 = syllabs[i];
                        var s2 = j < 0 ? "" : syllabs[j];
                        var s3 = k < 0 ? "" : syllabs[k];
                        if (j < 0 && k >= 0)
                        {
                            continue;
                        }
                        yield return Tuple.Create(s1 + s2 + s3, s1 + "1-" + s2 + "356$;" + s3);
                    }
                }
            }
        }
    }
}
