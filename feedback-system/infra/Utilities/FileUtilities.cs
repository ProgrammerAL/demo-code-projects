using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulumiInfra.Utilities;

public static class FileUtilities
{
    public static string DetermineFileContentType(string fileName)
    {
        var lowerCaseExtension = Path.GetExtension(fileName).ToLower();
        switch (lowerCaseExtension)
        {
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".map":
                return "application/json";
            case ".json":
                return "application/json";
            case ".png":
                return "image/png";
            case ".jpg":
                return "image/jpeg";
            case ".jpeg":
                return "image/jpeg";
            case ".svg":
                return "image/svg+html";
            case ".pdf":
                return "application/pdf";
            case ".ico":
                return "image/x-icon";
            case ".blat":
                return "application/octet-stream";
            case ".dll":
                return "application/octet-stream";
            case ".dat":
                return "application/octet-stream";
            case ".wasm":
                return "application/wasm";
            case ".woff":
                return "application/font-woff";
            case ".woff2":
                return "application/font-woff2";
            case ".br":
                return "application/x-br";
            case ".gz":
                return "application/x-gz";
            case "":
                return "application/octet-stream";
            case ".eot":
                return "application/vnd.ms-fontobject";
            case ".ttf":
                return "application/font-sfnt";
            case ".otf":
                return "font/opentype";
            case ".md":
                return "text/markdown";
            default:
                {
                    Pulumi.Log.Warn($"Unable to determine file content type of extention '{lowerCaseExtension}' for file '{fileName}', defaulting to application/octet-stream");
                    return "application/octet-stream";
                }
        }
    }
}
