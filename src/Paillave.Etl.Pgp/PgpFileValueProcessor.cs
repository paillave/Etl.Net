using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using PgpCore;

namespace Paillave.Etl.Pgp;

public enum PgpOperation
{
    Encrypt,
    EncryptAndSign,
    Decrypt,
    Sign,
    UnSign,
    Verify
}
public class PgpAdapterProcessorParameters
{
    public PgpOperation Operation { get; set; } = PgpOperation.Decrypt;
    [Sensitive]
    public string? PrivateKey { get; set; }
    [Sensitive]
    public string? Passphrase { get; set; }
    [Sensitive]
    public string? PublicKey { get; set; }
    public bool UseStreamCopy { get; set; } = true;
}
// public class PgpFileValueMetadata : IFileValueWithDestinationMetadata
// {
//     public string ParentFileName { get; set; }
//     public Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
// }
public class PgpFileValueProcessor : FileValueProcessorBase<PgpAdapterConnectionParameters, PgpAdapterProcessorParameters>
{
    public PgpFileValueProcessor(string code, string name, string connectionName, PgpAdapterConnectionParameters connectionParameters, PgpAdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private EncryptionKeys CreateEncryptionKeys(PgpAdapterProcessorParameters processorParameters)
    {
        if (!string.IsNullOrWhiteSpace(processorParameters.Passphrase) && !string.IsNullOrWhiteSpace(processorParameters.PublicKey) && !string.IsNullOrWhiteSpace(processorParameters.PrivateKey))
            return new EncryptionKeys(processorParameters.PublicKey, processorParameters.PrivateKey, processorParameters.Passphrase);
        else if (!string.IsNullOrWhiteSpace(processorParameters.Passphrase) && !string.IsNullOrWhiteSpace(processorParameters.PrivateKey))
            return new EncryptionKeys(processorParameters.PrivateKey, processorParameters.Passphrase);
        else if (!string.IsNullOrWhiteSpace(processorParameters.PublicKey))
            return new EncryptionKeys(processorParameters.PublicKey);
        throw new ArgumentException("Missing key parameters");
    }
    protected override void Process(IFileValue fileValue, PgpAdapterConnectionParameters connectionParameters, PgpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        using var inputStream = fileValue.Get(processorParameters.UseStreamCopy);

        switch (processorParameters.Operation)
        {
            case PgpOperation.Decrypt:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    pgp.Decrypt(inputStream, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    push(new PgpFileValue(ms, fileValue.Name, fileValue));
                }
                break;
            case PgpOperation.Encrypt:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    pgp.Encrypt(inputStream, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    push(new PgpFileValue(ms, fileValue.Name, fileValue));
                }
                break;
            case PgpOperation.Sign:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    pgp.Sign(inputStream, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    push(new PgpFileValue(ms, fileValue.Name, fileValue));
                }
                break;
            case PgpOperation.EncryptAndSign:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    pgp.EncryptAndSign(inputStream, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    push(new PgpFileValue(ms, fileValue.Name, fileValue));
                }
                break;
            case PgpOperation.UnSign:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    pgp.ClearSign(inputStream, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    push(new PgpFileValue(ms, fileValue.Name, fileValue));
                }
                break;
            case PgpOperation.Verify:
                {
                    var encryptionKeys = CreateEncryptionKeys(processorParameters);
                    var pgp = new PGP(encryptionKeys);
                    var ms = new MemoryStream();
                    var verified = pgp.Verify(inputStream, ms);
                    if (verified)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        push(new PgpFileValue(ms, fileValue.Name, fileValue));
                    }
                }
                break;
        }
    }

    protected override void Test(PgpAdapterConnectionParameters connectionParameters, PgpAdapterProcessorParameters processorParameters) { }
}
