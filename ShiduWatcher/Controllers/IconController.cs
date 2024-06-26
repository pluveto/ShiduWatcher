using Microsoft.AspNetCore.Mvc;
using System;
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
        [HttpGet("get-icon-base64")]
        public async Task<IActionResult> GetIconBase64([FromQuery] string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(path);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        icon.ToBitmap().Save(ms, ImageFormat.Png);
                        string base64Image = Convert.ToBase64String(ms.ToArray());
                        return Ok(new { error = (string)null, data = base64Image });
                    }
                }
                else
                {
                    return NotFound(new { error = "File not found", data = (string)null });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, data = (string)null });
            }
        }

        [HttpGet("get-icon")]
        // Directly return the icon file image/png
        public async Task<IActionResult> GetIcon([FromQuery] string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(path);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        icon.ToBitmap().Save(ms, ImageFormat.Png);
                        return File(ms.ToArray(), "image/png");
                    }
                }
                else
                {
                    return NotFound(new { error = "File not found", data = (string)null });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, data = (string)null });
            }
        }
    }
}
