using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Outline;

namespace Paillave.Pdf
{
    public class StructureReader
    {
        private readonly HeadersSetup _rootHeaderLevel = new HeadersSetup();
        private List<HeadersSetup> _currentHeaderPath = new List<HeadersSetup>();

        public StructureReader(PdfDocument pdfDocument, IList<HeadersSetup>? levelHeader)
        {
            _currentHeaderPath = new List<HeadersSetup> { _rootHeaderLevel };
            if (levelHeader != null)
                _rootHeaderLevel.DirectSubLevels.AddRange(levelHeader);
            var _bookmarks = GetBookmarks(pdfDocument) ?? new List<List<DocumentBookmarkNode>>();
            var bookmarkTemplates = _bookmarks.GroupBy(i => i.Count).Select(i => new { Name = i.Key, Nodes = i.ToList() }).ToList();
            HeadersSetup current = _rootHeaderLevel;
            foreach (var bookmarkTemplate in bookmarkTemplates)
            {
                var newLevel = new HeadersSetup(o => o.Bookmark(bookmarkTemplate.Nodes));
                current.DirectSubLevels.Add(newLevel);
                current = newLevel;
            }
        }
        private List<List<DocumentBookmarkNode>> GetBookmarks(IReadOnlyList<BookmarkNode> nodes, List<DocumentBookmarkNode> parentPath)
        {
            var ret = new List<List<DocumentBookmarkNode>>();
            foreach (var bookmark in nodes.OfType<DocumentBookmarkNode>())
            {
                var currentPath = new List<DocumentBookmarkNode> { bookmark };
                currentPath.AddRange(parentPath);
                ret.Add(currentPath);
                ret.AddRange(GetBookmarks(currentPath));
            }
            return ret;
        }
        private List<List<DocumentBookmarkNode>> GetBookmarks(List<DocumentBookmarkNode> parents) => GetBookmarks(parents[0].Children, parents);
        private List<List<DocumentBookmarkNode>> GetBookmarks(PdfDocument pdfDocument)
        {
            if (!pdfDocument.TryGetBookmarks(out var bookmarks)) return null;
            return GetBookmarks(bookmarks.Roots, new List<DocumentBookmarkNode>());
        }
        // public string[] GetCurrent() => _current.Select.ToDictionary(i => i.Key, i => i.Value);
        public List<string> Current { get; private set; } = new List<string>();
        public bool ProcessLine(TextLine textLine, Page page, List<HorizontalLine> lines)
        {
            var successfullCheck = this._currentHeaderPath.Last().DirectSubLevels.FirstOrDefault(i => i.Template.Check(textLine, page, lines));
            if (successfullCheck != null)
            {
                this.Current = Current.ToList();
                Current.Add(textLine.Text);
                _currentHeaderPath.Add(successfullCheck);
                return true;
            }
            if (this._currentHeaderPath.Count == 1)
            {
                return false;
            }
            if (this._currentHeaderPath.Count == 2)
            {
                var subRootSuccessfullCheck = _rootHeaderLevel.DirectSubLevels.FirstOrDefault(i => i.Template.Check(textLine, page, lines));
                if (subRootSuccessfullCheck != null)
                {
                    _currentHeaderPath.RemoveAt(1);
                    _currentHeaderPath.Add(subRootSuccessfullCheck);
                    this.Current = Current.ToList();
                    Current.Clear();
                    Current.Add(textLine.Text);
                    return true;
                }
            }
            else
            {
                var successfullParentsCheck = this._currentHeaderPath
                   .Select((i, idx) => new { HeaderLevel = i, Index = idx })
                   .Skip(1)
                   .AsEnumerable().Reverse()
                   .FirstOrDefault(i => i.HeaderLevel.Template.Check(textLine, page, lines));
                if (successfullParentsCheck != null)
                {
                    this.Current = Current.ToList();
                    for (int i = _currentHeaderPath.Count - 1; i >= successfullParentsCheck.Index; i--)
                    {
                        _currentHeaderPath.RemoveAt(i);
                        Current.RemoveAt(i - 1);
                    }
                    Current.Add(textLine.Text);
                    _currentHeaderPath.Add(successfullParentsCheck.HeaderLevel);
                    return true;
                }
            }
            return false;
        }
    }
    public class HeadersSetup
    {
        internal HeadersSetup(params HeadersSetup[] directSubLevels)
            => (Template, DirectSubLevels) = (null, directSubLevels.ToList());
        public HeadersSetup(Func<TextTemplate, TextTemplate> templateBuilder, params HeadersSetup[] directSubLevels)
            => (Template, DirectSubLevels) = (templateBuilder(new TextTemplate()), directSubLevels.ToList());
        public TextTemplate? Template { get; }
        public List<HeadersSetup> DirectSubLevels { get; }
    }
}