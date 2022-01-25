using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;

namespace Paillave.Pdf
{
    public class LinesOfWords
    {
        private class LineOfWords
        {
            private readonly List<Word> _words = new List<Word>();
            private int _topSum;
            public int TopAverage { get; private set; }
            private int? _topMax;
            private int _bottomSum;
            private int _bottomAverage;
            private int? _bottomMax;
            public bool Belongs(Word word)
            {
                if (_topMax < word.BoundingBox.Centroid.Y || _bottomMax > word.BoundingBox.Centroid.Y)
                    // if (TopAverage < word.BoundingBox.Centroid.Y || _bottomAverage > word.BoundingBox.Centroid.Y)
                    return false;
                return true;
            }
            public void Add(Word word)
            {
                this._words.Add(word);
                this._topSum += (int)word.BoundingBox.Top;
                this.TopAverage = this._topSum / _words.Count;
                this._topMax = Math.Max(this._topMax ?? (int)word.BoundingBox.Top, (int)word.BoundingBox.Top);
                this._bottomSum += (int)word.BoundingBox.Bottom;
                this._bottomAverage = this._bottomSum / _words.Count;
                this._bottomMax = Math.Max(this._bottomMax ?? (int)word.BoundingBox.Bottom, (int)word.BoundingBox.Bottom);
            }
            public override string ToString()
                => string.Join(" ", _words.OrderBy(word => word.BoundingBox.Left).Select(word => word.Text));
        }
        public LinesOfWords() { }
        public LinesOfWords(IEnumerable<Word> words) => AddWords(words);
        private readonly List<LineOfWords> _lines = new List<LineOfWords>();
        public void AddWord(Word word)
        {
            var targetLine = _lines.FirstOrDefault(line => line.Belongs(word));
            if (targetLine == null)
            {
                targetLine = new LineOfWords();
                _lines.Add(targetLine);
            }
            targetLine.Add(word);
        }
        public void AddWords(IEnumerable<Word> words)
        {
            foreach (var word in words)
                this.AddWord(word);
        }
        public List<string> GetLines()
            => _lines.OrderByDescending(line => line.TopAverage).Select(line => line.ToString()).ToList();
    }
}
//TODO extract structure from expressions
//TODO locate table within structure
