using System;
using System.IO;

namespace OneDrive_CSharp
{
    public enum JobType
    {
        Uploading = 0,
        Downloading = 1,
        Deleting = 2,
        FileCreation = 3
    }

    public class File
    {
        public string path;
        public JobType job;
        public double progress;
        public long size;
        public TimeSpan eta;
        public bool done;


        public static string ExtensionToFontAwesome(string filePath)
        {
            if (Path.GetExtension(filePath) == "pdf")
                return "file-pdf";
            else if (Path.GetExtension(filePath) == "docx" || Path.GetExtension(filePath) == "dotx" || Path.GetExtension(filePath) == "dotm" || Path.GetExtension(filePath) == "docb")
                return "file-word";
            else if (Path.GetExtension(filePath) == "xlsx" || Path.GetExtension(filePath) == "xlsm" || Path.GetExtension(filePath) == "xltx" || Path.GetExtension(filePath) == "xltm")
                return "file-excel";
            else if (Path.GetExtension(filePath) == "pptx" || Path.GetExtension(filePath) == "ppt" || Path.GetExtension(filePath) == "pps" || Path.GetExtension(filePath) == "pptm" || Path.GetExtension(filePath) == "pot")
                return "file-powerpoint";
            else if (Path.GetExtension(filePath) == "jpg" || Path.GetExtension(filePath) == "jpeg" || Path.GetExtension(filePath) == "png" || Path.GetExtension(filePath) == "gif" || Path.GetExtension(filePath) == "webp" || Path.GetExtension(filePath) == "tiff" || Path.GetExtension(filePath) == "fit" || Path.GetExtension(filePath) == "fits" || Path.GetExtension(filePath) == "psd" || Path.GetExtension(filePath) == "raw" || Path.GetExtension(filePath) == "cr2" || Path.GetExtension(filePath) == "heif" || Path.GetExtension(filePath) == "svg" || Path.GetExtension(filePath) == "bmp")
                return "file-image";
            else if (Path.GetExtension(filePath) == "zip" || Path.GetExtension(filePath) == "rar" || Path.GetExtension(filePath) == "7z" || Path.GetExtension(filePath) == "tar" || Path.GetExtension(filePath) == "bz2" || Path.GetExtension(filePath) == "gz" || Path.GetExtension(filePath) == "cab")
                return "file-archive";
            else if (Path.GetExtension(filePath) == "mp3" || Path.GetExtension(filePath) == "wav" || Path.GetExtension(filePath) == "flac" || Path.GetExtension(filePath) == "ogg" || Path.GetExtension(filePath) == "3gp" || Path.GetExtension(filePath) == "alac" || Path.GetExtension(filePath) == "aiff" || Path.GetExtension(filePath) == "m4a" || Path.GetExtension(filePath) == "opus" || Path.GetExtension(filePath) == "wma" || Path.GetExtension(filePath) == "webm")
                return "file-audio";
            else if (Path.GetExtension(filePath) == "cs" || Path.GetExtension(filePath) == "d" || Path.GetExtension(filePath) == "cpp" || Path.GetExtension(filePath) == "h" || Path.GetExtension(filePath) == "py" || Path.GetExtension(filePath) == "bash" || Path.GetExtension(filePath) == "sh" || Path.GetExtension(filePath) == "sln" || Path.GetExtension(filePath) == "csproj" || Path.GetExtension(filePath) == "js" || Path.GetExtension(filePath) == "java" || Path.GetExtension(filePath) == "fs" || Path.GetExtension(filePath) == "php" || Path.GetExtension(filePath) == "css" || Path.GetExtension(filePath) == "razor" || Path.GetExtension(filePath) == "html" || Path.GetExtension(filePath) == "scss" || Path.GetExtension(filePath) == "sass" || Path.GetExtension(filePath) == "asp" || Path.GetExtension(filePath) == "swf" || Path.GetExtension(filePath) == "xhtml" || Path.GetExtension(filePath) == "jsp" || Path.GetExtension(filePath) == "rb" || Path.GetExtension(filePath) == "xml" || Path.GetExtension(filePath) == "asx")
                return "file-code";
            else if (Path.GetExtension(filePath) == "csv")
                return "file-csv";
            else if (Path.GetExtension(filePath) == "mp4" || Path.GetExtension(filePath) == "mkv" || Path.GetExtension(filePath) == "avi" || Path.GetExtension(filePath) == "vob" || Path.GetExtension(filePath) == "wmv" || Path.GetExtension(filePath) == "flv" || Path.GetExtension(filePath) == "m4v" || Path.GetExtension(filePath) == "mpg" || Path.GetExtension(filePath) == "m4p" || Path.GetExtension(filePath) == "mov")
                return "file-video";
            else
                return "file";
        }

    }
}