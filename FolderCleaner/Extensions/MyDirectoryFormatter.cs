using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FolderCleaner.Extensions
{
    public class MyDirectoryFormatter : IDirectoryFormatter
    {
        private const int KILOBYTE = 1024;
        private const int MEGABYTE = KILOBYTE * KILOBYTE;
        private const int GIGABYTE = KILOBYTE * MEGABYTE;



        /// <summary>
        /// Generates an HTML view for a directory.
        /// </summary>
        public async Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }

            context.Response.ContentType = "text/html;charset=UTF-8";

            if (context.Request.Method.ToUpper() == "HEAD")
            {
                // HEAD, no response body
                return;
            }

            PathString requestPath = context.Request.PathBase + context.Request.Path;
            
            var builder = new StringBuilder();

            builder.AppendFormat($@"
                <!DOCTYPE html>
                <html lang='{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}'>
                <head>
                    <title id='location'>{HtmlEncode(requestPath.Value).ToString().Substring(1)}</title>
                    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css' />
                    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.2/css/all.min.css' />
                </head>
                <body style='background:#f8f9fa;'>
                <header>
                    <nav class='navbar navbar-expand-lg navbar-dark bg-dark'>
                        <a class='navbar-brand' href='/'>FolderCleaner <img src='/favicon.ico' width='32px' style='-webkit-transform: scaleX(-1);transform:scaleX(-1);'/></a>
                        <div class='collapse container navbar-collapse' id='navbarText'>
                            <ul class='navbar-nav mr-auto'>
                                <li class='nav-item'>
                                    <a class='nav-link' href='/'>Home</a>
                                </li>
                                <li class='nav-item active'>
                                    <a class='nav-link' href='/C:'>DirectoryViewer<span class='sr-only'>(current)</span></a>
                                </li>
                                <li class='nav-item'>
                                    <a class='nav-link' href='/Home/History'>History</a>
                                </li>
                            </ul>
                        </div>
                    </nav>
                </header>
                    <div class='bg-light container' style='margin-top:15px;box-shadow: 0 .25rem .75rem rgba(0, 0, 0, .25);'>
                        <div class='container body-content' style='padding-top:15px;'>
                            <main role='main' class='pb-3'>
                                <h3>{GetHeaderLinks(requestPath)}</h1>
                                <table class='table table-hover'>
                                    <thead>
			                        <tr>
				                        <th></th>
				                        <th>Name</th>
				                        <th>Size</th>
				                        <th>Last Modified</th>
			                        </tr>
			                        </thead>
			                        <tbody>
                                        {GetTableLines(contents.ToList())}
                                    </tbody>
                                </table>
                            </main>
                         </div>
	                </div>
                    <footer class='border-top text-center footer text-muted' style='margin-top:20px;margin-bottom:20px;'>
                        <div class='container mt-3'>
                            &copy; 2022 <img src='/favicon.ico' width='12px' style='-webkit-transform: scaleX(-1);transform:scaleX(-1);' />
                        </div>
                    </footer>
                    <script src='https://code.jquery.com/jquery-3.4.1.slim.min.js' integrity='sha384-J6qa4849blE2+poT4WnyKhv5vZF5SrPo0iEjwBvKU7imGFAV0wwj1yYfoRSJoZ+n' crossorigin='anonymous'></script>
                    <script src='https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js' integrity='sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo' crossorigin='anonymous'></script>
                    <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js' integrity='sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6' crossorigin='anonymous'></script>
                    <script src='/js/site.js' type='text/javascript' language='javascript'></script>
            </body>
            </html>
            ");
            await context.Response.WriteAsync(builder.ToString());
        }
        private string GetHeaderLinks(PathString requestPath)
        {
            string cumulativePath = "/";
            var header = "";
            var requestPathSplit = requestPath.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in requestPathSplit)
            {
                if(requestPathSplit.Length < 2)
                {
                    cumulativePath = cumulativePath + segment + "/";
                    header += $@"<a href='{HtmlEncode(cumulativePath)}'>{HtmlEncode(segment)}</a>";
                }
                else
                {
                    cumulativePath = cumulativePath + segment + "/";
                    header += $@"<a href='{HtmlEncode(cumulativePath)}'>{HtmlEncode(segment)}/</a>";
                }
            }
            return header;
        }

        private string GetTableLines(List<IFileInfo> contents)
        {
            var builder = new StringBuilder();
            foreach (var subdir in contents.Where(info => info.IsDirectory))
            {
                builder.AppendFormat($@"
                    <tr style='cursor:pointer;' onclick='copyToClipboard({string.Format($"\"{HtmlEncode(subdir.Name)}\"")})' data-container='body' data-toggle='popover' data-placement='top'>
                        <td><i class='fas fa-archive badge badge-success' style='{GetStyle(subdir.Name)}'></i></td>
                        <td><a href='{HtmlEncode(subdir.Name)}/'>{HtmlEncode(subdir.Name)}/</a></td>
                        <td></td>
                        <td>{HtmlEncode(subdir.LastModified.LocalDateTime.ToString("dd/MM/yyyy HH:mm"))}</td>
                    </tr>
                    ");
            }
            foreach (var file in contents.Where(info => !info.IsDirectory))
            {
                builder.AppendFormat($@"
                  <tr>
                    <td><i class='fa-lg {GetFontAwesomeIcon(file.Name)}' style='{GetStyle(file.Name)}'></i></td>
                    <td><p>{HtmlEncode(file.Name)}</p></td>
                    <td>{GetLengthString(file.Length)}</td>
                    <td>{HtmlEncode(file.LastModified.LocalDateTime.ToString("dd/MM/yyyy HH:mm"))}</td>
                  </tr>
                ");
            }
            return builder.ToString();
        }

        private string GetLengthString(double number)
        {
            string units;
            double convertedNumber;
            if (number > GIGABYTE)
            {
                units = "Gb";
                convertedNumber = number * 1.0 / GIGABYTE;
            }
            else if (number > MEGABYTE)
            {
                units = "Mb";
                convertedNumber = number * 1.0 / MEGABYTE;
            }
            else if (number > KILOBYTE)
            {
                units = "Kb";
                convertedNumber = number / KILOBYTE;
            }
            else
            {
                units = "b";
                convertedNumber = number;
            }
            return Convert.ToDouble($"{convertedNumber:G2}").ToString("R0") + " " + units;
        }

        private string GetFontAwesomeIcon(string fileName)
        {
            var fileExtension = fileName.Split('.').Last();
            switch (fileExtension)
            {
                case "txt":
                    return "fas fa-file-alt badge badge-success";
                case "xml":
                    return "fas fa-file-alt badge badge-success";
                case "zip":
                    return "fas fa-file-archive badge badge-success";
                case "png":
                    return "fas fa-file-image badge badge-success";
                case "jpg":
                    return "fas fa-file-image badge badge-success";
                case "pdf":
                    return "fas fa-file-pdf badge badge-success";
                case "pptx":
                    return "fas fa-file-powerpoint badge badge-success";
                case "xlsx":
                    return "fas fa-file-excel badge badge-success";
                case "xls":
                    return "fas fa-file-excel badge badge-success";
                case "cs":
                    return "fas fa-file-code badge badge-success";
                case "xslt":
                    return "fas fa-file-code badge badge-success";
                case "mp3":
                    return "fas fa-file-audio badge badge-success";
                case "mp4":
                    return "fas fa-file-video badge badge-success";
                case "avi":
                    return "fas fa-file-video badge badge-success";
                case "mov":
                    return "fas fa-file-video badge badge-success";
                case "wmv":
                    return "fas fa-file-video badge badge-success";
                case "mpeg":
                    return "fas fa-file-video badge badge-success";
                case "flv":
                    return "fas fa-file-video badge badge-success";
                case "rar":
                    return "fas fa-file-archive badge badge-success";
                case "bmp":
                    return "fas fa-file-image badge badge-success";
                case "csv":
                    return "fas fa-file-csv badge badge-success";
                case "psd":
                    return "fas fa-file-image badge badge-success";
                case "gif":
                    return "fas fa-file-image badge badge-success";
                case "dll":
                    return "fas fa-cog badge badge-success";
                case "config":
                    return "fas fa-tools badge badge-success";
                case "exe":
                    return "fas fa-dragon badge badge-success";
                case "jpeg":
                    return "fas fa-file-image badge badge-success";
                case "json":
                    return "fab fa-js badge badge-success";
                case "css":
                    return "fab fa-css3-alt badge badge-success";
                case "sln":
                    return "fas fa-file-alt badge badge-success";
                case "csproj":
                    return "fas fa-file-alt badge badge-success";
                case "lnk":
                    return "fas fa-paperclip badge badge-success";
                case "bat":
                    return "fas fa-cogs badge badge-success";
                case "inf":
                    return "fas fa-file-invoice badge badge-success";
                case "cmd":
                    return "fas fa-terminal badge badge-success";
                case "cat":
                    return "fas fa-paw badge badge-success";
                case "sys":
                    return "fas fa-wrench badge badge-success";
                case "properties":
                    return "fas fa-project-diagram badge badge-secondary";
                case "policy":
                    return "fas fa-poll-h badge badge-success";
                case "mdf":
                    return "fas fa-database badge badge-success";
                default:
                    return "fas fa-question badge badge-success";
            }
        }
        private string GetStyle(string fileName)
        {
            var fileExtension = fileName.Split('.').Last();
            switch (fileExtension)
            {
                case "txt":
                    return "display:unset !important;font-size:18px;background:#2d2c2b;";
                case "xml":
                    return "display:unset !important;font-size:18px;background:#f67c01;";
                case "zip":
                    return "display:unset !important;font-size:18px;background:#fdb614;";
                case "png":
                    return "display:unset !important;font-size:18px;background:#e86756;";
                case "jpg":
                    return "display:unset !important;font-size:18px;background:#3171b7;";
                case "tif":
                    return "display:unset !important;font-size:18px;background:#5f2501;";
                case "pdf":
                    return "display:unset !important;font-size:18px;background:#ff1b0e;";
                case "pptx":
                    return "display:unset !important;font-size:18px;background:#cf4320;";
                case "xlsx":
                    return "display:unset !important;font-size:18px;background:#06b036;";
                case "xls":
                    return "display:unset !important;font-size:18px;background:#06b036;";
                case "cs":
                    return "display:unset !important;font-size:18px;background:#009202;";
                case "xslt":
                    return "display:unset !important;font-size:18px;background:#6e235f;";
                case "mp3":
                    return "display:unset !important;font-size:18px;background:#61c8ff;";
                case "mp4":
                    return "display:unset !important;font-size:18px;background:#3081c6;";
                case "avi":
                    return "display:unset !important;font-size:18px;background:#49cfae;";
                case "mov":
                    return "display:unset !important;font-size:18px;background:#e87071";
                case "wmv":
                    return "display:unset !important;font-size:18px;background:#005fa8;";
                case "mpeg":
                    return "display:unset !important;font-size:18px;background:#10bcf5;";
                case "flv":
                    return "display:unset !important;font-size:18px;background:#82ce83;";
                case "rar":
                    return "display:unset !important;font-size:18px;background:#5d147d;";
                case "bmp":
                    return "display:unset !important;font-size:18px;background:#34485c;";
                case "csv":
                    return "display:unset !important;font-size:18px;background:#b3da73;";
                case "psd":
                    return "display:unset !important;font-size:18px;background:#00c8ff;";
                case "gif":
                    return "display:unset !important;font-size:18px;background:#463f3f;";
                case "dll":
                    return "display:unset !important;font-size:18px;background:#576a77;";
                case "config":
                    return "display:unset !important;font-size:18px;background:#bdbab3;"; 
                case "exe":
                    return "display:unset !important;font-size:18px;background:#d44545;";
                case "jpeg":
                    return "display:unset !important;font-size:18px;background:#bb9c23;";
                case "json":
                    return "display:unset !important;font-size:18px;background:#e6a329;";
                case "css":
                    return "display:unset !important;font-size:18px;background:#0170ba;";
                case "sln":
                    return "display:unset !important;font-size:18px;background:#8a00ce;";
                case "csproj":
                    return "display:unset !important;font-size:18px;background:#006931;";
                case "lnk":
                    return "display:unset !important;font-size:18px;background:#4a4a4a;";
                case "bat":
                    return "display:unset !important;font-size:18px;background:#333232;";
                case "inf":
                    return "display:unset !important;font-size:18px;background:linear-gradient(80deg, rgba(5, 103, 11, 0.96) 0%, #2fb191 70%);";
                case "cmd":
                    return "display:unset !important;font-size:18px;background:linear-gradient(80deg, rgba(0, 0, 0, 0.96) 0%, #4f5957 70%);";
                case "cat":
                    return "display:unset !important;font-size:18px;background:linear-gradient(-8deg, rgba(0, 0, 0, 0.96) 0%, #bdaa43 70%);";
                case "sys":
                    return "display:unset !important;font-size:18px;background:linear-gradient(-45deg, rgba(0, 0, 0, 0.96) 0%, #b0afaa 70%);";
                case "properties":
                    return "display:unset !important;font-size:18px;";
                case "policy":
                    return "display:unset !important;font-size:18px;background:repeating-linear-gradient(180deg, rgb(92, 113, 153) 0%, #5994bf 70%)";
                case "mdf":
                    return "display:unset !important;font-size:18px;background:linear-gradient(10deg, rgb(25, 36, 35) 0%, #0a9326 60%)";
                default:
                    return "display:unset !important;font-size:18px;background:linear-gradient(180deg, rgb(5, 39, 103) 0%, #1378c3 70%);";
            }
        }

        private static string HtmlEncode(string body)
        {
            return WebUtility.HtmlEncode(body);
        }
    }
}
