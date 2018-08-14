using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    public class CsvSplit
    {
        private readonly char _fieldDelimiter;
        private readonly char _textDelimiter;
        private readonly Regex _regex;

        public CsvSplit(char fieldDelimiter = ';', char textDelimiter = '"')
        {
            this._fieldDelimiter = fieldDelimiter;
            this._textDelimiter = textDelimiter;
            string sep = GetRegexTranslation(fieldDelimiter);
            string tq = GetRegexTranslation(textDelimiter);
            this._regex = new Regex(sep + @"(?=(?:[^" + tq + "]*" + tq + "[^" + tq + "]*" + tq + ")*(?![^" + tq + "]*" + tq + "))", RegexOptions.Compiled | RegexOptions.Singleline);
        }
        private string GetRegexTranslation(char character)
        {
            switch ((int)char.GetNumericValue(character))
            {
                case 0x7: return @"\a";
                case 0x8: return @"\b";
                case 0x9: return @"\t";
                case 0xD: return @"\r";
                case 0xB: return @"\v";
                case 0xC: return @"\f";
                case 0xA: return @"\n";
                case 0x1B: return @"\e";
                default: return (@".$^{[(|)*+?\""".IndexOf(character) >= 0) ? @"\" + character.ToString() : character.ToString();
            }
        }

        private IEnumerable<string> InternalParseCsvLine(string line)
        {
            List<string> cells = new List<string>();
            foreach (string cell in _regex.Split(line))
            {
                string newCell = cell;
                if (cell.StartsWith(_textDelimiter.ToString()) && cell.EndsWith(_textDelimiter.ToString()))
                    newCell = cell.Substring(1, cell.Length - 2).Replace($"{_textDelimiter.ToString()}{_textDelimiter.ToString()}", $"{_textDelimiter.ToString()}");
                yield return newCell;
            }
        }

        public IList<string> ParseCsvLine(string line)
        {
            return InternalParseCsvLine(line).ToList();
        }

        public string JoinCsvLine(IList<string> fields)
        {
            return string.Join(this._fieldDelimiter.ToString(), fields.Select(i =>
            {
                if (i.Contains(_fieldDelimiter)) return $"{_textDelimiter}{i.Replace(_textDelimiter.ToString(), $"{_textDelimiter}{_textDelimiter}")}{_textDelimiter}";
                else return i;
            }));
        }
    }
}
