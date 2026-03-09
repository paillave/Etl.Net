using System.Collections;
using System.Collections.Generic;
using Terminal.Gui;

namespace Paillave.Etl.ExecutionToolkit.ConsoleApp;

public class App : Window
{
    private readonly ListView _lv;
    public App() : base("Batch Runner")
    {
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();
        _lv = new ListView()
        {
            // AllowsMarking = true,
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = true,
        };

        this.Add(_lv);
    }
    public void SetData(List<TaskUnit> list)
    {
        Application.MainLoop.Invoke(() =>
        {
            _lv.SetSource(list);
            _lv.SetNeedsDisplay();
        });
    }
    public void DataRefreshed()
    {
        Application.MainLoop.Invoke(() =>
        {
            // _lv.SetSource(list);
            _lv.SetNeedsDisplay();
            // _lv.Redraw(_lv.Bounds);
        });
    }
}