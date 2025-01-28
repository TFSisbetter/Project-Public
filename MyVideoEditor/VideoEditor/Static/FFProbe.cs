﻿using System.Diagnostics;
using System.Text.Json;
using VideoEditor.Dtos;
namespace VideoEditor.Static;

public static class FFProbe
{
    public static FFProbeRapport? GetRapport(string fullName)
    {
        var arguments = $" -v error -show_format -show_streams -print_format json \"{fullName}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FFExecuteblesPaths.FFProbe.FullName,
                WorkingDirectory = FFExecuteblesPaths.FFProbe.Directory?.FullName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string json = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return JsonSerializer.Deserialize<FFProbeRapport>(json);
    }
}

