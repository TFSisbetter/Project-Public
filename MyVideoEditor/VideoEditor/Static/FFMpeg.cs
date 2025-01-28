﻿using System.Diagnostics;
using VideoEditor.Enums;
using VideoEditor.FF;
using VideoEditor.Dtos;
namespace VideoEditor.Static;

public static class FFMpeg
{
    public static IEnumerable<byte[]> ReadFrames(
        string fullName,
        Resolution resolution,
        Timestamp? startTime = null)
    {
        startTime = startTime ?? new Timestamp();

        //var ffmpegArgs = $"-i \"{fullName}\" -vf scale={resolution.Width}:{resolution.Height} -pix_fmt rgb24 -f rawvideo -";
        var arguments = $"-i \"{fullName}\" " +
                        $"-ss {startTime} " +
                        $"-pix_fmt rgb24 -f rawvideo -";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = FFExecuteblesPaths.FFMpeg.FullName,
            WorkingDirectory = FFExecuteblesPaths.FFMpeg.Directory!.FullName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        using (var stream = process.StandardOutput.BaseStream)
        {
            int frameSize = resolution.Width * resolution.Height * 3; // rgb24 → 3 bytes per pixel
            byte[] buffer = new byte[frameSize];

            while (stream.Read(buffer, 0, frameSize) == frameSize)
            {
                yield return buffer;
            }
        }
    }

    public static void WriteFrames(
        string outputFullName,
        Resolution resolution,
        Fps fps,
        IEnumerable<byte[]> frames,
        byte crf = 23,
        FFMpegPreset preset = FFMpegPreset.medium)
    {
        var ffmpegArgs = $"-y -f rawvideo -pixel_format rgb24 " +
                         $"-video_size {resolution.Width}x{resolution.Height} " +
                         $"-framerate {fps} " +
                         $"-i - -c:v libx265 " +
                         $"-crf {crf} " +
                         $"-preset {Enum.GetName(preset)} " +
                         $"-r {fps} " +
                         $"\"{outputFullName}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = FFExecuteblesPaths.FFMpeg.FullName,
            WorkingDirectory = FFExecuteblesPaths.FFMpeg.Directory!.FullName,
            Arguments = ffmpegArgs,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        using (var stream = process.StandardInput.BaseStream)
        {
            int frameSize = resolution.Width * resolution.Height * 3; // rgb24 → 3 bytes per pixel

            foreach (var frame in frames)
            {
                if (frame.Length != frameSize)
                    throw new ArgumentException("Frame size mismatch!");

                stream.Write(frame, 0, frame.Length);
            }
        }
    }

    public static IEnumerable<byte[]> ReadAudio(
        string fullName,
        int channels,
        int sampleRate)
    {
        var ffmpegArgs = $"-i \"{fullName}\" -vn -f s16le -ac {channels} -ar {sampleRate} -";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = FFExecuteblesPaths.FFMpeg.FullName,
            WorkingDirectory = FFExecuteblesPaths.FFMpeg.Directory!.FullName,
            Arguments = ffmpegArgs,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        using (var stream = process.StandardOutput.BaseStream)
        {
            int bufferSize = channels * 2; // 16-bit = 2 bytes per sample
            var buffer = new byte[bufferSize];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, bufferSize);
                if (bytesRead == 0) yield break;
                yield return buffer;
            }
        }
    }

    public static void WriteAudio(
        string outputFullName,
        IEnumerable<byte[]> audioFrames,
        int channels = 2,
        int sampleRate = 48000,
        int quality = 1) // VBR quality factor (0 = best, 5 = worst)
    {
        var ffmpegArgs = $"-y -f s16le -ac {channels} -ar {sampleRate} -i - -c:a aac -q:a {quality} \"{outputFullName}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = FFExecuteblesPaths.FFMpeg.FullName,
            WorkingDirectory = FFExecuteblesPaths.FFMpeg.Directory!.FullName,
            Arguments = ffmpegArgs,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        using (var stream = process.StandardInput.BaseStream)
        {
            foreach (var frame in audioFrames)
            {
                stream.Write(frame, 0, frame.Length);
            }
        }
    }

    public static void MuxVideoWithMultipleAudioStreams(
        string outputFullName,
        string videoFullName,
        IEnumerable<string> audioFullNames)
    {
        // Basis FFmpeg argumenten voor het muxen van 1 video met meerdere audiostreams.
        var ffmpegArgs = $"-y -i \"{videoFullName}\" ";  // Voeg video toe als eerste input

        // Voeg de audio bestanden toe (meerdere bestanden kunnen worden toegevoegd)
        foreach (var audioFile in audioFullNames)
        {
            ffmpegArgs += $"-i \"{audioFile}\" ";  // Voeg elke audio stream toe
        }

        // Voeg de kopieeropties toe
        // -c:v copy voor video (geen herencoding)
        // -c:a copy voor audio (geen herencoding)
        // Voeg de audio streams in de juiste volgorde toe met -map
        ffmpegArgs += "-c:v copy ";  // Video niet her-encoderen
        int audioStreamIndex = 1;  // Start bij index 1 voor audiostreams (0 is de videostream)
        foreach (var _ in audioFullNames)
        {
            ffmpegArgs += $"-c:a:{audioStreamIndex} copy ";  // Audio niet her-encoderen
            audioStreamIndex++;
        }

        // Voeg de mapping toe voor de output bestand
        ffmpegArgs += "-map 0:v:0 ";  // Eerste video stream
        audioStreamIndex = 1;
        foreach (var _ in audioFullNames)
        {
            ffmpegArgs += $"-map {audioStreamIndex}:a:0 ";  // Voeg elke audiostream toe
            audioStreamIndex++;
        }

        // Specificeer de output bestandsnaam
        ffmpegArgs += $"\"{outputFullName}\"";

        // Start de FFmpeg process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = FFExecuteblesPaths.FFMpeg.FullName,
            WorkingDirectory = FFExecuteblesPaths.FFMpeg.Directory!.FullName,
            Arguments = ffmpegArgs,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            process.WaitForExit(); // Wacht tot het proces klaar is
        }
    }

}
