using System.Collections.Generic;
using System.Text;

namespace Paillave.Pdf;

public class SvgBuilder
{
    private readonly List<string> _commands = new();
    private readonly double _width;
    private readonly double _height;
    private const string PageColor = "blue";
    private const string LabelColor = "red";
    public SvgBuilder(double width, double height, double pageNumber, string title, bool drawPage)
    {
        _width = width;
        _height = height;
        if (drawPage)
        {
            Rectangle(0, 0, width, height, PageColor);
            Text(width / 2, 5, pageNumber.ToString(), PageColor);
            Text(_width / 2, _height - 25, title, PageColor);
        }
    }
    public SvgBuilder HorizontalLine(double left, double top, double right, string label = null)
    {
        _commands.Add($"<path d='M{left} {_height - top} H{right}' fill='none'/>");
        if (label != null) Text((left + right) / 2, top, label, LabelColor);
        return this;
    }
    public SvgBuilder VerticalLine(double left, double top, double bottom, string label = null)
    {
        _commands.Add($"<path d='M{left} {_height - top} V{_height - bottom}' fill='none'/>");
        if (label != null) Text(left, (top + bottom) / 2, label, LabelColor);
        return this;
    }
    public SvgBuilder Rectangle(double left, double top, double right, double bottom, string color = null)
    {
        if (color == null) _commands.Add($"<path d='M{left} {_height - top} V{_height - bottom} H{right} V{_height - top} Z' fill='none'/>");
        else _commands.Add($"<path d='M{left} {_height - top} V{_height - bottom} H{right} V{_height - top} Z' fill='none' style='stroke:{color}'/>");
        return this;
    }
    public SvgBuilder Line(double left, double top, double right, double bottom, string color = null, string label = null)
    {
        if (color == null) _commands.Add($"<path d='M{left} {_height - top} L{right} {_height - bottom}' fill='none'/>");
        else _commands.Add($"<path d='M{left} {_height - top} L{right} {_height - bottom}' fill='none' style='stroke:{color}'/>");
        if (label != null) Text((left + right) / 2, (top + bottom) / 2, label, LabelColor);
        return this;
    }
    public SvgBuilder Text(double left, double top, string text, string color = null)
    {
        if (color == null) _commands.Add($"<text x='{left}' y='{_height - top}' style='stroke-width:0'>{text}</text>");
        else _commands.Add($"<text x='{left}' y='{_height - top}' style='stroke-width:0' fill='{color}'>{text}</text>");
        return this;
    }
    public void Show() => Tools.OpenFile(GetSvg(true), "html");
    public string GetSvg(bool wrapsWithHtml)
    {
        var sb = new StringBuilder();
        if (wrapsWithHtml)
        {
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
        }
        sb.AppendLine($"<svg height='{_height}' width='{_width}' style='stroke:rgb(0,0,0);stroke-width:1'>");
        foreach (var command in _commands) sb.AppendLine(command);
        sb.AppendLine("</svg>");
        if (wrapsWithHtml)
        {
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
        }
        return sb.ToString();
    }
}