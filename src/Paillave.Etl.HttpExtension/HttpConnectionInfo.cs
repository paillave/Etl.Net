﻿using System.Collections.Generic;

namespace Paillave.Etl.HttpExtension;

public class HttpConnectionInfo : IHttpConnectionInfo
{
    public string Url { get; set; }
    public List<string> HeaderParts { get; set; }
    public string AuthenticationType { get; set; }
    public int MaxAttempts { get; set; } = 5;
}
