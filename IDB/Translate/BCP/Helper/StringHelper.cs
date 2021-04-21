using System.Collections.Generic;

namespace IDB.Translate.BCP.Helper
{
    public static class StringHelper
    {
        public static string RemoveEscapeSequenceFromXmlString(this string xmlString, Dictionary<string, int> foundSequences, out long countReplaced)
        {
            countReplaced = 0;
            var startPos = xmlString.IndexOf("&#x");
            while (startPos != -1)
            {
                var endPos = xmlString.IndexOf(";", startPos); // look for end of escape sequence
                if (endPos == -1) break; // no escape sequence, it's something else

                var escSequence = xmlString.Substring(startPos, (endPos - startPos) + 1);
                if (escSequence == "&#xD;" || escSequence == "&#xA;")
                    xmlString = xmlString.Replace(escSequence, " ");
                else
                    xmlString = xmlString.Replace(escSequence, string.Empty);

                if (foundSequences != null)
                {
                    if (foundSequences.ContainsKey(escSequence))
                        foundSequences[escSequence]++;
                    else
                        foundSequences.Add(escSequence, 1);
                }
                countReplaced++;

                startPos = xmlString.IndexOf("&#x");
            }
            return xmlString;
        }
    }
}
