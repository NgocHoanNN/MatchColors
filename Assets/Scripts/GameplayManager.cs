using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region START

    private bool hasGameFinished;

    public static GameplayManager Instance;

    public List<Color> Colors;

    private void Awake()
    {
        Instance = this;

        hasGameFinished = false;
        GameManager.Instance.IsInitialized = true;

        score = 0;
        _scoreText.text = ((int)score).ToString();

        // Lưu giá trị thời gian spawn ban đầu
        _originalSpawnTime = _spawnTime;

        StartCoroutine(SpawnScore());
    }

    #endregion

    #region GAME_LOGIC

    [SerializeField] private ScoreEffect _scoreEffect;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !hasGameFinished)
        {
            if (CurrentScore == null)
            {
                GameEnded();
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (!hit.collider || !hit.collider.gameObject.CompareTag("Block"))
            {
                GameEnded();
                return;
            }

            int currentScoreId = CurrentScore.ColorId;
            int clickedScoreId = hit.collider.gameObject.GetComponent<Player>().ColorId;

            if (currentScoreId != clickedScoreId)
            {
                GameEnded();
                return;
            }

            var t = Instantiate(_scoreEffect, CurrentScore.gameObject.transform.position, Quaternion.identity);
            t.Init(Colors[currentScoreId]);

            var tempScore = CurrentScore;
            if (CurrentScore.NextScore != null)
            {
                CurrentScore = CurrentScore.NextScore;
            }
            Destroy(tempScore.gameObject);

            UpdateScore();
        }
    }

    #endregion

    #region SCORE

    private float score;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;

    // Biến để lưu giá trị ban đầu của _spawnTime
    [SerializeField] private float _spawnTime;
    private float _originalSpawnTime;

    // Giới hạn thời gian spawn tối thiểu
    [SerializeField] private float _minSpawnTime = 0.2f;

    [SerializeField] private Score _scorePrefab;
    private Score CurrentScore;

    private void UpdateScore()
    {
        score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)score).ToString();

        // Giảm thời gian spawn mỗi 20 điểm, không thấp hơn giới hạn tối thiểu
        if (score % 20 == 0)
        {
            _spawnTime = Mathf.Max(_spawnTime - 0.1f, _minSpawnTime);
            Debug.Log($"Tốc độ thả block tăng: _spawnTime = {_spawnTime}");
        }
    }

    private IEnumerator SpawnScore()
    {
        Score prevScore = null;

        while (!hasGameFinished)
        {
            var tempScore = Instantiate(_scorePrefab);

            if (prevScore == null)
            {
                prevScore = tempScore;
                CurrentScore = prevScore;
            }
            else
            {
                prevScore.NextScore = tempScore;
                prevScore = tempScore;
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }

    #endregion

    #region GAME_OVER

    [SerializeField] private AudioClip _loseClip;
    public UnityAction GameEnd;

    public void GameEnded()
    {
        hasGameFinished = true;
        GameEnd?.Invoke();
        SoundManager.Instance.PlaySound(_loseClip);
        GameManager.Instance.CurrentScore = (int)score;
        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GoToMainMenu();
    }

    #endregion
}
