using System.Text;
using System.Xml.Linq;

namespace RepositoryAnalyticsApi.Extensibility
{
    public static class XmlHelper
    {
        /// <summary>
        /// Removes any UTF-8 Byte Order Mark before parsing into XDocument
        /// </summary>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        /// <remarks>
        /// .NET project files will ocassionally fail to be parsed into XDocuments due to a having a BOM.
        /// </remarks>
        public static XDocument RemoveUtf8ByteOrderMarkAndParse(string xmlContent)
        {
            string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xmlContent.StartsWith(byteOrderMarkUtf8))
            {
                xmlContent = xmlContent.Remove(0, byteOrderMarkUtf8.Length);

                // For some reason removing the UTF preamble sometimes removes the first XML character
                if (!xmlContent.StartsWith("<"))
                {
                    xmlContent = $"<{xmlContent}";
                }
            }

            return XDocument.Parse(xmlContent);
        }
    }
}
