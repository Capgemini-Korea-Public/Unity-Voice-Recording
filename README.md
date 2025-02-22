# Unity-Voice-Recording Module

The **Unity-Voice-Recording Module** is a Unity module that allows real-time voice recording. It converts the recorded audio into WAV/OGG formats for saving or into a tensor format for STT (Speech-to-Text) processing.

With this module, you can easily implement voice recording and speech recognition features in your Unity projects.

---

## Features

- **Real-Time Voice Recording and Playback:** Record audio in real time and play it back.
- **Live Recording Volume Meter:** Visualize the audio levels while recording.
- **Device Selection:** Choose the desired audio input device.
- **File Format Options:** Save recordings in either WAV or OGG format.
- **STT Integration:** Transmit audio to an external STT service.
- **Configurable Maximum Recording Length:** Set a maximum recording duration (up to 30 seconds).
- **Playback of Recorded Files:** Includes a function to play back the recorded audio.

---

## Requirements

### Unity Version
- **Minimum:** Unity 2021.3 or higher (6000.0.37f1 is recommended)

### External Dependencies
- **FFmpeg:** Must be downloaded separately and configured with the proper path (see the **External Dependencies** section for details).
- **STT Service:** This module does not include an STT service. You need to download and configure it separately.

---

## Installation and Usage

1. Open the **Package Manager** in Unity.
2. Click the **"+"** button in the top left and select **"Install Package from Git URL"**.
3. Enter the following URL: https://github.com/Capgemini-Korea-Public/Unity-Voice-Recording.git?path=Packages/com.capgemini.voicerecorder#main
)
4. If you wish to have the sample in your Assets folder, import the **Basic Example** from the **Samples** section in the Package Manager.
5. Click the **Record** button in the scene to start recording.
6. Click the **Stop** button to end the recording.
7. Depending on the selected file format, the recording will be saved. If STT is connected, the audio will be transmitted accordingly.

---

## External Dependencies

### FFmpeg Installation Guide (Windows)

1. Navigate to [ffmpeg.org](https://ffmpeg.org/).
2. Click on **Download**.
3. Select the blue Windows icon in the middle.
4. Choose **Windows builds from gyan.dev**.
5. Download **ffmpeg-git-full.7z**.
6. Install it in your project's **Plugin** folder.

---

### Documentation and Changelog

- **CHANGELOG.md**
- **LICENSE** and **THIRD PARTY NOTICES**

