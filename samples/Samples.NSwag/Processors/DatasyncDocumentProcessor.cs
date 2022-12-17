using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Diagnostics;

namespace Samples.NSwag.Processors
{
    public class DatasyncDocumentProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            Debug.WriteLine("DatasyncDocumentProcessor");
        }
    }
}
