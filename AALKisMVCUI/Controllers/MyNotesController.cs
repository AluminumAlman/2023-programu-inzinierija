using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AALKisMVCUI.Utility;
using AALKisShared;
using AALKisShared.Enums;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace AALKisMVCUI.Controllers;

[Route("[controller]")]
public class MyNotesController : Controller
{
    private readonly ILogger<MyNotesController> _logger;
    private readonly APIClient _client;

    public MyNotesController(ILogger<MyNotesController> logger, APIClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IActionResult> Index()
    {
        string targetUri = "/Folder";

        var folders = await _client
            .Fetch<List<FolderRecord<NoteRecord>>>(targetUri, HttpMethod.Get)
            ?? throw new JsonException($"Got empty response from {targetUri}");
        folders.Sort();
        // Order by access date descending.
        foreach (var folder in folders)
        {
            folder.Records.Sort();
            folder.Records = folder.Records
                .Where(record => (record.Flags & NoteFlags.Archived) == 0)
                .ToList();
        }
        return View(folders);
    }

    [HttpPost("[action]/{folderId}")]
    public async Task<IActionResult> CreateEmptyNote(int folderId)
    {
        try 
        {
            string targetUri = $"/Note/Create/{folderId}/Untitled";
            var id = await _client
                    .Fetch<int?>(targetUri, HttpMethod.Post)
                    ?? throw new JsonException($"Got empty response from {targetUri}");

            return Json(new { redirectToUrl = "Editor/" + id});


        }
        catch (Exception ex) {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError($"Failed to create EmptyNote\n" + ex.ToString());
            return BadRequest();
        }

    }

    [HttpPost("[action]/{folderName}")]
    public async Task<IActionResult> CreateEmptyFolder(string folderName)
    {
        try
        {
            string targetUri = "/Folder/" + folderName;
            var response = await _client
                    .Fetch(targetUri, HttpMethod.Post)
                    ?? throw new JsonException($"Got empty response from {targetUri}");
            return Ok();
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError($"Failed to create EmptyNote\n" + ex.ToString());
            return BadRequest();
        }
    }

    [HttpPost("[action]/{folderName}/{noteName}")]
    public async Task<IActionResult> ArchiveNote(string folderName, string noteName)
    {
        try
        {
            string targetUri = "/Note/" + noteName + "/" + folderName;

            NoteRecord fieldsToUpdate = new NoteRecord();
            fieldsToUpdate.Flags = NoteFlags.Archived; // Not sets, but switches.

            string jsonString = JsonConvert.SerializeObject(fieldsToUpdate);


            // Update Note Archived Flag
            await _client.Fetch($"Note/{folderName}/{noteName}",
                    HttpMethod.Put,
                    new StringContent(jsonString, Encoding.UTF8, "application/json"));

            return Ok();

        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError($"Failed to create EmptyNote\n" + ex.ToString());
            return BadRequest();
        }

    }

    [HttpPost("[action]/{newFolderName}/{oldFolderName}/{noteName}")]
    public async Task<IActionResult> ChangeFolderName(string newFolderName, string oldFolderName, string noteName)
    {
        try
        {
            string targetUri = "/Note/" + newFolderName + "/" + oldFolderName + "/" + noteName;


            // Update Note Archived Flag
            await _client.Fetch($"Note/{newFolderName}/{oldFolderName}/{noteName}",
                    HttpMethod.Post,
                    new StringContent(""));

            return Ok();

        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError($"Failed to create EmptyNote\n" + ex.ToString());
            return BadRequest();
        }

    }

}
