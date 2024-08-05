using System;
using System.Collections.Generic;
using UnityEngine;

// 데이터를 로드하는 정적 클래스
public static class DataLoader<T> where T : class
{
    // JSON 파일에서 데이터를 로드합니다.
    public static List<T> LoadDataFromJson(string fileName)
    {
        // Resources 폴더에서 JSON 파일을 로드
        TextAsset jsonFile = Resources.Load<TextAsset>($"Json/{fileName}");

        // 파일이 존재하면 데이터를 읽고, 존재하지 않으면 빈 리스트를 반환
        if (jsonFile != null)
        {
            string json = jsonFile.text;
            DataWrapper<T> dataWrapper = JsonUtility.FromJson<DataWrapper<T>>(json);
            return dataWrapper.data;
        }
        else
        {
            Debug.LogError("File not found: " + fileName);
            return new List<T>();
        }
    }

    // JSON 데이터를 담는 내부 클래스
    [Serializable]
    private class DataWrapper<TData>
    {
        public List<TData> data;
    }
}