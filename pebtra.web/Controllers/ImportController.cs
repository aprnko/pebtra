using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Pebtra.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ImportController : ControllerBase
{
    private static readonly string UploadDirectory = Path.Combine(Path.GetTempPath(), "PebtraUploads");
    private static readonly List<string> UploadedFiles = new List<string>();

    public ImportController()
    {
        if (!Directory.Exists(UploadDirectory))
        {
            Directory.CreateDirectory(UploadDirectory);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        if (files == null || !files.Any())
        {
            return BadRequest("No files were uploaded.");
        }

        var uploadResults = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // Generate a unique filename using GUID
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(UploadDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store original filename and generated filename
                var fileInfo = new
                {
                    OriginalName = file.FileName,
                    SavedAs = fileName,
                    Size = file.Length
                };

                uploadResults.Add(fileName);
                UploadedFiles.Add(fileName);
            }
        }

        return Ok(new { UploadedFiles = uploadResults });
    }

    [HttpGet("get")]
    public IActionResult Get()
    {
        return Ok(UploadedFiles);
    }
} 