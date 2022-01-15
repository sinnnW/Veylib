using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Veylib
{
    public class English
    {
        public class Structure
        {
            public class Settings
            {
                public bool RemoveManners = true;
                public bool RemovePunctuation = true;
                public bool RemoveExtra = true;
                public bool RemoveInsults = true;
            }

            private Settings sett;
            public Structure(Settings settings)
            {
                sett = settings;
            }
            public Structure()
            {
                sett = new Settings();
            }

            private List<string> spl(string input)
            {
                var output = new List<string>();
                foreach (var word in input.Split(' ', ',', '.', '?', '!', '-', '\'', '/', '"', ':', ';'))
                    output.Add(word.ToLower());
                return output;
            }

            public string SimplifySentence(string input)
            {
                var output = new List<string>();

                string[] words = { "please", "thanks", "thank you", "my pleasure", "excuse me" };
                char[] puncs = { ',', '.', '?', '!', '-', '\'', '/', '"', ':', ';' };
                string[] extras = { "fuckin", "mate" };
                string[] insultInitializers = { "you" };
                string[] insultMiddle = { "stupid", "fuck", "retarded" };

                var builder = new StringBuilder();

                output = spl(input);


                #region Remove manners, please, thanks, and all that shit
                if (sett.RemoveManners)
                    foreach (var rem in words)
                        output.RemoveAll(word => word.Contains(rem));
                #endregion

                #region Remove punctuation
                if (sett.RemovePunctuation)
                    foreach (var punc in puncs)
                        output.RemoveAll(word => word.Contains(punc.ToString()));
                #endregion

                #region Remove extras
                if (sett.RemoveExtra)
                    foreach (var ex in extras)
                        output.RemoveAll(word => word.Contains(ex));
                #endregion

                #region Remove name calling
                if (sett.RemoveInsults)
                {
                    var removals = new List<List<string>>();
                    int removalsIndex = 0;

                    // look for a phrase
                    for (var x = 0;x < output.Count;x++)
                    {
                        foreach (var init in insultInitializers)
                        {
                            if (output[x].Contains(init))
                            {
                                removals.Add(new List<string>());

                                removals[removalsIndex].Add(output[x]);
                                int offset = 0;
                                while (true)
                                {
                                    offset++;
                                    int miss = 0;
                                    foreach (var mid in insultMiddle)
                                    {
                                        if (output[x + offset].Contains(mid))
                                        {
                                            removals[removalsIndex].Add(output[x + offset]);
                                        }
                                        else
                                            miss++;
                                    }

                                    // If its missed, try searching the dict
                                    if (miss == insultMiddle.Length)
                                    {
                                        foreach (var name in File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "names.txt")))
                                            if (output[x + offset].Contains(name.ToLower()))
                                                removals[removalsIndex].Add(output[x + offset]);    

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    string ou = string.Join(" ", output);
                    foreach (var rem in removals)
                        ou = ou.Replace(string.Join(" ", rem), "");

                    output = spl(ou);
                }
                #endregion

                return string.Join(" ", output);
            }
        }
    }
}
