using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HGO.ASPNetCore.FileManager.CommandsProcessor
{
    /// <summary>Processes commands submitted to the file manager.</summary>
    public interface IFileManagerCommandsProcessor
    {
        /// <summary>Processes a file-manager command and returns its MVC action result.</summary>
        /// <param name="id">The file-manager instance identifier.</param>
        /// <param name="command">The command name.</param>
        /// <param name="parameters">The command parameters serialized as JSON.</param>
        /// <param name="file">The uploaded file, when the command is an upload.</param>
        /// <returns>A task that represents the asynchronous operation and contains the action result.</returns>
        Task<IActionResult> ProcessCommandAsync(string id, string command, string parameters, IFormFile file);
    }
}
