namespace MyAudioPackage.Core
{
    public enum SaveMode
    {
        SaveWav,
        SaveOgg,
        None  // 저장하지 않음
    }

    public class SaveModeSelector
    {
        // 현재 선택된 저장 모드 (기본값: None)
        public SaveMode SelectedSaveMode { get; private set; } = SaveMode.None;

        // 저장 모드 변경 시 발생하는 이벤트
        public event System.Action<SaveMode> OnSaveModeChanged;

        /// <summary>
        /// SaveMode를 직접 설정합니다.
        /// </summary>
        public void SetSaveMode(SaveMode mode)
        {
            SelectedSaveMode = mode;
            OnSaveModeChanged?.Invoke(mode);
        }

        /// <summary>
        /// 드롭다운 인덱스를 기반으로 저장 모드를 설정합니다.
        /// 0: SaveWav, 1: SaveOgg, 2: None
        /// </summary>
        public void SetSaveModeFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    SetSaveMode(SaveMode.SaveWav);
                    break;
                case 1:
                    SetSaveMode(SaveMode.SaveOgg);
                    break;
                case 2:
                    SetSaveMode(SaveMode.None);
                    break;
                default:
                    SetSaveMode(SaveMode.SaveWav);
                    break;
            }
        }
    }
}
