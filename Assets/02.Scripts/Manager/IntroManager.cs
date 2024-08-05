using UnityEngine;

public class IntroManager : MonoBehaviour
{
    private bool _canStart;

    private void Start()
    {
        _canStart = true;
    }

    private void Update()
    {
        // 아무 키나 입력 처리
        if (_canStart && Input.anyKeyDown)
        {
            StartGame();
        }
    }

    // 게임 시작
    private void StartGame()
    {
        UIManager.Instance.SwitchToNextUI();
        gameObject.SetActive(false);
    }
}