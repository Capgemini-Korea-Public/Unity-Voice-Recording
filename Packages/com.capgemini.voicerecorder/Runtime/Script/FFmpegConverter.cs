using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class FFmpegConverter
{
    /// <summary>
    /// FFmpeg를 사용하여 WAV 파일을 OGG 파일로 변환합니다.
    /// FFmpeg가 시스템 PATH에 있거나, ffmpegPath에 FFmpeg 실행 파일의 전체 경로를 지정해야 합니다.
    /// </summary>
    /// <param name="wavFilePath">변환할 원본 WAV 파일의 전체 경로</param>
    /// <param name="oggFilePath">저장할 OGG 파일의 전체 경로</param>
    /// <param name="ffmpegPath">FFmpeg 실행 파일 경로 (PATH에 있다면 "ffmpeg"만 입력)</param>
    /// 


    public static void ConvertWavToOgg(string wavFilePath, string oggFilePath, string ffmpegPath)
    {
        if (!File.Exists(wavFilePath))
        {
            Debug.LogError("WAV file does not exist: " + wavFilePath);
            return;
        }
        if (!File.Exists(ffmpegPath))
        {
            Debug.LogError("FFmpeg executable does not exist: " + ffmpegPath);
        }

        // FFmpeg 명령어: -y (자동 덮어쓰기), -i "input.wav" "output.ogg"
        string arguments = $"-y -i \"{wavFilePath}\" \"{oggFilePath}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath, // 예: "ffmpeg" 또는 "C:\\Path\\To\\ffmpeg.exe"
            Arguments = arguments,
            CreateNoWindow = true,    // 콘솔 창을 생성하지 않음
            UseShellExecute = false,  // Shell 사용 안 함
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                // FFmpeg가 처리하는 동안 출력 및 에러 메시지를 읽습니다.
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Debug.Log("FFmpeg output: " + output);
                if (!string.IsNullOrEmpty(error))
                    Debug.LogWarning("FFmpeg error: " + error);

                if (process.ExitCode == 0)
                {
                    Debug.Log("Conversion successful: " + oggFilePath);
                }
                else
                {
                    Debug.LogError("FFmpeg conversion failed. Exit code: " + process.ExitCode);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception during FFmpeg execution: " + ex.Message);
        }
    }
}
