using System;
using System.IO;
using UnityEngine;

namespace Puzzle.Core
{
    /// <summary>
    /// 리플레이 데이터를 로컬 파일 시스템에 JSON 형식으로 저장하고 불러오는 유틸리티 클래스입니다.
    /// 저장 경로: Application.persistentDataPath/Replay/
    /// </summary>
    public static class ReplayStorage
    {
        /// <summary> 리플레이 파일이 저장되는 디렉터리 이름 </summary>
        private const string ReplayDirectory = "Replay";

        /// <summary>
        /// 리플레이 데이터를 JSON 파일로 저장합니다.
        /// </summary>
        /// <param name="replayData">저장할 리플레이 데이터</param>
        /// <returns>저장된 파일의 전체 경로. 실패 시 null</returns>
        public static string Save(ReplayData replayData)
        {
            try
            {
                string directoryPath = GetReplayDirectoryPath();
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"replay_{timestamp}.json";
                string filePath = Path.Combine(directoryPath, fileName);

                string json = JsonUtility.ToJson(replayData, true);
                File.WriteAllText(filePath, json);

                Debug.Log($"리플레이 저장 완료: {filePath}");
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"리플레이 저장 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 지정된 경로에서 리플레이 데이터를 불러옵니다.
        /// </summary>
        /// <param name="filePath">리플레이 JSON 파일의 전체 경로</param>
        /// <returns>로드된 리플레이 데이터. 실패 시 null</returns>
        public static ReplayData Load(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"리플레이 파일을 찾을 수 없습니다: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                ReplayData replayData = JsonUtility.FromJson<ReplayData>(json);

                Debug.Log($"리플레이 로드 완료: {filePath}");
                return replayData;
            }
            catch (Exception e)
            {
                Debug.LogError($"리플레이 로드 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 리플레이 저장 디렉터리의 전체 경로를 반환합니다.
        /// </summary>
        /// <returns>리플레이 디렉터리 경로</returns>
        public static string GetReplayDirectoryPath()
        {
            return Path.Combine(Application.dataPath, "05_Table", ReplayDirectory);
        }
    }
}
