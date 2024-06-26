using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ShiduWatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IconController : ControllerBase
    {
        private (bool isValid, string? fullPath, string? errorMessage) ValidateAndNormalizePath(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);

                if (!System.IO.File.Exists(fullPath))
                {
                    return (false, null, "File not found");
                }
                return (true, fullPath, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private (Icon? icon, string? errorMessage) ExtractIcon(string path)
        {
            try
            {
                Icon? icon = Icon.ExtractAssociatedIcon(path);
                if (icon == null)
                {
                    return (null, "Unable to extract icon");
                }
                return (icon, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        [HttpGet("get-icon-base64")]
        public async Task<IActionResult> GetIconBase64([FromQuery] string path)
        {
            var validation = ValidateAndNormalizePath(path);
            if (!validation.isValid)
            {
                return BadRequest(new { error = validation.errorMessage });
            }

            var extraction = ExtractIcon(validation.fullPath!);
            if (extraction.icon == null)
            {
                return BadRequest(new { error = extraction.errorMessage });
            }

            using (MemoryStream ms = new MemoryStream())
            {
                extraction.icon.ToBitmap().Save(ms, ImageFormat.Png);
                string base64Image = Convert.ToBase64String(ms.ToArray());
                return Ok(new { data = base64Image });
            }
        }

        [HttpGet("get-icon")]
        public async Task<IActionResult> GetIcon([FromQuery] string path)
        {
            var validation = ValidateAndNormalizePath(path);
            if (!validation.isValid)
            {
                return BadRequest(new { error = validation.errorMessage });
            }

            var extraction = ExtractIcon(validation.fullPath!);
            if (extraction.icon == null)
            {
                return BadRequest(new { error = extraction.errorMessage });
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bitmap = extraction.icon.ToBitmap())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                }
                return File(ms.ToArray(), "image/png");
            }
        }
        private static readonly int CacheSize = 1000;
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new ConcurrentDictionary<string, byte[]>();
        private static readonly Queue<string> CacheOrder = new Queue<string>();

        [HttpGet("get-favicon")]
        public async Task<IActionResult> GetFavicon([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest(new { error = "Invalid URL." });
            }

            string faviconUrl = $"{uriResult.Scheme}://{uriResult.Host}/favicon.ico";

            // Check if the result is already in the cache
            if (Cache.TryGetValue(faviconUrl, out byte[]? cachedFavicon))
            {
                return File(cachedFavicon, "image/png");
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(faviconUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(new { error = "Failed to download favicon." });
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (Bitmap bitmap = new Bitmap(stream))
                            {
                                // Validate the image format
                                if (bitmap.RawFormat.Equals(ImageFormat.Icon) || bitmap.RawFormat.Equals(ImageFormat.Png))
                                {
                                    bitmap.Save(ms, ImageFormat.Png);
                                }
                                else
                                {
                                    return BadRequest(new { error = "Invalid favicon format." });
                                }
                            }

                            byte[] faviconBytes = ms.ToArray();

                            // Add to cache
                            Cache[faviconUrl] = faviconBytes;
                            CacheOrder.Enqueue(faviconUrl);

                            // Ensure cache size limit
                            if (CacheOrder.Count > CacheSize)
                            {
                                string oldestKey = CacheOrder.Dequeue();
                                Cache.TryRemove(oldestKey, out _);
                            }

                            return File(faviconBytes, "image/png");
                        }
                    }
                }
                catch
                {
                    return BadRequest(new { error = "Error occurred while fetching the favicon." });
                }
            }
        }
    }
}
