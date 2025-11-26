// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using iText.Kernel.Pdf;
// using iText.Kernel.Pdf.Xobject;
// using Paillave.Etl.Core;
// using SixLabors.ImageSharp;

// // using UglyToad.PdfPig;
// // using UglyToad.PdfPig.Content;
// // using UglyToad.PdfPig.Filters;

// namespace Paillave.Etl.Samples
// {
//     class Program
//     {
//         static void Main5(string[] args)
//         {
//             using (var stream = File.OpenRead("1T01 Marketing Sarl - TA 2011 - Taxation office.pdf"))
//             using (var pdfReader = new PdfReader(stream))
//             using (var pdfDocument = new PdfDocument(pdfReader))
//             {
//                 var idx = 1;
//                 var tmps = Enumerable
//                     .Range(1, pdfDocument.GetNumberOfPages())
//                     .Select(i => pdfDocument.GetPage(i).GetPdfObject().GetAsDictionary(PdfName.Resources).GetAsDictionary(PdfName.XObject))
//                     .ToList();
//                 foreach (var pageXObjects in tmps)
//                     foreach (var imgRef in pageXObjects.KeySet())
//                     {
//                         PdfStream imgStream = pageXObjects.GetAsStream(imgRef);
//                         using var image = Image.Load("original.jpg");
//                         var tmp = imgStream.GetBytes();
//                         File.WriteAllBytes($"img{idx++}.jpg", tmp);
//                     }
//                 // var imgs = pdfDocument.GetPages().SelectMany(page => page.GetImages()).ToList();
//                 // foreach (var (img, idx) in imgs.Select((i, idx) => (i, idx)))
//                 // {
//                 //     if (img.TryGetBytes(out var jpgBytes))
//                 //     {
//                 //         jpgBytes = ApplyFilters(img);

//                 //         using (var image = Image.Load(jpgBytes.ToArray(), out var format))
//                 //         {
//                 //             // image.Mutate(c => c.Resize(30, 30));
//                 //             // image.Save(outputStream, format);
//                 //         }



//                 //         // File.WriteAllBytes($"img{idx}.jpg", jpgBytes.ToArray());
//                 //         // File.WriteAllBytes($"img{idx}.jpg", jpgBytes.ToArray());
//                 //     }
//                 //     else if (img.TryGetPng(out var pngBytes))
//                 //     {
//                 //         pngBytes = ApplyFilters(img);

//                 //         using (var image = Image.Load(pngBytes.ToArray(), out var format))
//                 //         {
//                 //             // image.Mutate(c => c.Resize(30, 30));
//                 //             // image.Save(outputStream, format);
//                 //         }

//                 //         // File.WriteAllBytes($"img{idx}.png", pngBytes.ToArray());
//                 //         // File.WriteAllBytes($"img{idx}.png", pngBytes.ToArray());
//                 //     }
//                 // }
//             }
//         }
//         // protected List<Stream> ManipulatePdf(string SRC)
//         // {
//         //     PdfDocument srcDoc = new PdfDocument(new PdfReader(SRC));

//         //     // Assume that there is a single XObject in the source document
//         //     // and this single object is an image.
//         //     PdfDictionary pageDict = srcDoc.GetFirstPage().GetPdfObject();
//         //     PdfDictionary pageResources = pageDict.GetAsDictionary(PdfName.Resources);
//         //     PdfDictionary pageXObjects = pageResources.GetAsDictionary(PdfName.XObject);
//         //     PdfName imgRef = pageXObjects.KeySet().First();
//         //     PdfStream imgStream = pageXObjects.GetAsStream(imgRef);
//         //     srcDoc.Close();
//         // }
//         // private static byte[] ApplyFilters(IPdfImage image)
//         // {
//         //     byte[] result = image.RawBytes.ToArray();
//         //     IReadOnlyList<IFilter> filters = DefaultFilterProvider.Instance.GetFilters(image.ImageDictionary);

//         //     foreach (var filter in filters)
//         //     {
//         //         if (filter.IsSupported)
//         //         {
//         //             result = filter.Decode(result, image.ImageDictionary, 0);
//         //         }
//         //     }
//         //     return result;
//         // }
//     }
// }
