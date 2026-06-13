using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AmidaManager : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Image skillGauge;
    private float currentInvincibleTime = 0f;
    [SerializeField]
    private AudioSource invincibleBgmSource;
    private Image playerImage;
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Text gameOverText;

    [SerializeField]
    private AudioClip gameOverSE;
    [SerializeField]
    private CanvasGroup startCanvasGroup;
    [SerializeField]
    private AudioSource startAudioSource;

    [SerializeField]
    private AudioClip startSE;
    [SerializeField]
    private GameObject startText;
    [SerializeField]
    private float obstacleHitRadius = 25f;


    private float startY;
    [SerializeField]
    private GameObject retryButton;

    private int generatedRows = 0;

    [SerializeField]
    private int bufferRows = 10;

    [SerializeField]
    private RectTransform scrollTarget;

    [SerializeField]
    private float scrollSpeed = 5f;

    [SerializeField]
    private float followOffset = 100f;
    private enum MoveState
    {
        Fall,
        HorizontalMove
    }

    private MoveState state = MoveState.Fall;
    private RectTransform currentLine = null;

    private Vector2 horizontalTarget;

    [SerializeField]
    private GameObject obstaclePrefab;

    [SerializeField]
    private float obstacleChance = 0.3f;

    private List<RectTransform> obstacles =
        new List<RectTransform>();
    [SerializeField]
    private GameObject verticalLinePrehub;

    [SerializeField]
    private GameObject horizontalLinePrehub;

    [SerializeField]
    private RectTransform parentUI;

    [SerializeField]
    private int lineCount = 5;

    [SerializeField]
    private int rowCount = 5;

    [SerializeField]
    private float xSpacing = 150f;

    [SerializeField]
    private float ySpacing = 80f;

    [SerializeField]
    private Text resultText;

    [SerializeField]
    private GameObject HowtoStartText;

    [SerializeField]
    private Text scoreText;

    private int score = 0;

    [SerializeField]
    private RectTransform playerMarker;
    [SerializeField]
    private float invincibleTime = 3f;


    private bool isInvincible = false;

    private bool canUseInvincible = true;

    [SerializeField]
    private float moveSpeed = 300f;

    [SerializeField]
    private float fallSpeed = 200f;

    [SerializeField]
    private float lineCreateInterval = 0.2f;

    private float lineTimer = 0f;

    private bool isGameOver = false;

    private bool isStarted = false;

    [SerializeField]
    private float drawDistance = 30f;


    private Vector2 startDrawPosition;

    [SerializeField]
    private RectTransform previewLine;

    private bool isDrawing = false;

    [SerializeField]
    private Button[] startButtons;

    [SerializeField]
    private string[] rewards;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioClip moveSE;

    [SerializeField]
    private AudioClip goalSE;


    private List<GameObject> spawnedLines = new List<GameObject>();  

    private Dictionary<int, List<int>> horizontalLines = new Dictionary<int, List<int>>();

    private Vector2 lastMousePosition;
    private bool isMoving = false;

    //=====unity=====
    void Start()
    {
        gameOverText.gameObject.SetActive(false);
        Random.InitState(
            System.DateTime.Now.Millisecond
        );

        AdjustSpacing();

        ArrangeButtons();

        CreateVerticalLines();

        GenerateRows(bufferRows);

        // ←追加
        previewLine.gameObject
            .SetActive(false);

        playerMarker.anchoredPosition =
            new Vector2(0, -rowCount * ySpacing);

        startY =
            playerMarker
            .anchoredPosition.y;

        playerMarker.SetAsLastSibling();

        playerImage =
    playerMarker.GetComponent<Image>();
    }
    void Update()
    {
        if (isGameOver)
        {
            return;
        }

        // スタート前は完全停止
        if (!isStarted)
        {
            return;
        }

        // ↓開始後だけ
        HandleDrawInput();

        HandleInvincibleInput();

        CheckObstacleCollision();

        if (state == MoveState.Fall)
        {
            MoveUp();
        }
        else if (state == MoveState.HorizontalMove)
        {
            MoveHorizontal();
        }

        UpdateScroll();
        CheckGenerateRows();
        UpdateScore();
        UpdateDifficulty();
        UpdateRainbow();
        UpdateSkillGauge();
    }
    //=====ゲーム開始、終了=====
    public void StartGame(int index)
    {
        StartCoroutine(
            StartGameCoroutine(index)
        );
    }

    IEnumerator StartGameCoroutine(
    int index
)
    {
        HowtoStartText.SetActive(false);
        // ボタン無効化
        foreach (Button button in startButtons)
        {
            button.interactable = false;
        }

        // プレイヤー位置
        playerMarker.anchoredPosition =
            startButtons[index]
            .GetComponent<RectTransform>()
            .anchoredPosition;

        startY =
            playerMarker.anchoredPosition.y;

        // START表示
        startText.SetActive(true);

        startCanvasGroup.alpha = 1f;

        // スタートSE
        startAudioSource.PlayOneShot(startSE);

        // 演出待ち
        yield return new WaitForSeconds(
            startSE.length
        );

        // BGM開始
        bgmSource.Play();

        // フェードアウト
        float fadeTime = 0.5f;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            startCanvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    timer / fadeTime
                );

            yield return null;
        }

        startText.SetActive(false);

        // ←ここ超重要
        isStarted = true;

        Debug.Log("ゲーム開始");
    }
    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
    public void BackToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "TitleScene"
        );
    }
    IEnumerator GameOverCoroutine()
    {
        // プレイヤー停止
        isStarted = false;

        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        // GameOver表示
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "GAME OVER";

        // ピピー！SE
        audioSource.PlayOneShot(gameOverSE);

        // スコア保存
        ScoreManager.score = score;

        // 2秒待つ
        yield return new WaitForSeconds(2f);

        // Resultへ
        SceneManager.LoadScene("ResultScene");
    }
    void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        StartCoroutine(GameOverCoroutine());
    }

    //=====プレイヤー移動=====
    void MoveUp()
    {
        if (state != MoveState.Fall) return;

        playerMarker.anchoredPosition +=
            Vector2.up * fallSpeed * Time.deltaTime;

        CheckLineCollision();
    }
    void CheckLineCollision()
    {
        foreach (GameObject line in spawnedLines)
        {
            if (line == null)
            {
                continue;
            }

            if (!line.CompareTag("Line"))
            {
                continue;
            }

            RectTransform rect =
                line.GetComponent<RectTransform>();

            // Y距離
            float yDistance = Mathf.Abs(
                playerMarker.anchoredPosition.y
                - rect.anchoredPosition.y
            );

            // 線の左右端
            float halfWidth =
                rect.sizeDelta.x / 2f;

            float leftEdge =
                rect.anchoredPosition.x
                - halfWidth;

            float rightEdge =
                rect.anchoredPosition.x
                + halfWidth;

            // プレイヤーが線の範囲内にいる？
            bool isInsideLine =
                playerMarker.anchoredPosition.x
                >= leftEdge - 10f
                &&
                playerMarker.anchoredPosition.x
                <= rightEdge + 10f;

            // 同じ横線なら無視
            if (rect == currentLine)
            {
                continue;
            }

            // Yも近く、Xも線の上なら反応
            if (yDistance < 10f
                && isInsideLine)
            {
                currentLine = rect;

                StartHorizontalMove(rect);

                return;
            }
        }
    }
    void StartHorizontalMove(RectTransform lineRect)
    {
        if (state != MoveState.Fall)
        {
            return;
        }

        currentLine = lineRect;
        state = MoveState.HorizontalMove;

        float playerX =
            playerMarker.anchoredPosition.x;

        float startX =
            -((lineCount - 1) * xSpacing) / 2f;

        // 今いる縦線Indexを取得
        int currentIndex =
            Mathf.RoundToInt(
                (playerX - startX)
                / xSpacing
            );

        // 左右どちらへ行くか判定
        if (playerX < lineRect.anchoredPosition.x)
        {
            // 右へ
            int nextIndex =
                Mathf.Min(
                    currentIndex + 1,
                    lineCount - 1
                );

            horizontalTarget =
                new Vector2(
                    startX
                    + nextIndex * xSpacing,
                    lineRect.anchoredPosition.y
                );
        }
        else
        {
            // 左へ
            int nextIndex =
                Mathf.Max(
                    currentIndex - 1,
                    0
                );

            horizontalTarget =
                new Vector2(
                    startX
                    + nextIndex * xSpacing,
                    lineRect.anchoredPosition.y
                );
        }
    }

    void MoveHorizontal()
    {
        if (currentLine == null)
        {
            state = MoveState.Fall;
            return;
        }

        // 横線のY固定
        float fixedY =
            currentLine.anchoredPosition.y;

        // Xだけ移動
        float newX =
            Mathf.MoveTowards(
                playerMarker.anchoredPosition.x,
                horizontalTarget.x,
                moveSpeed * Time.deltaTime
            );

        playerMarker.anchoredPosition =
            new Vector2(newX, fixedY);

        // 到着判定
        if (Mathf.Abs(
            playerMarker.anchoredPosition.x
            - horizontalTarget.x) < 5f)
        {
            playerMarker.anchoredPosition =
                new Vector2(
                    horizontalTarget.x,
                    fixedY
                );

            // 少し下げる
            playerMarker.anchoredPosition +=
                Vector2.up * 15f;

            state = MoveState.Fall;

            StartCoroutine(
                ResetLineDelay()
            );
        }
    }
    IEnumerator ResetLineDelay()
    {
        yield return new WaitForSeconds(0.25f);
        currentLine = null;
    }
    void CreatePlayerLine()
    {
        GameObject line =
            Instantiate( horizontalLinePrehub,parentUI);

        RectTransform rect =
            line.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(xSpacing,rect.sizeDelta.y);

        rect.anchoredPosition =playerMarker.anchoredPosition;

        spawnedLines.Add(line);
    }
    void HandleInvincibleInput()
    {
        if (Input.GetKeyDown(KeyCode.E)
            && canUseInvincible)
        {
            StartCoroutine(
                InvincibleCoroutine()
            );
        }
    }
    IEnumerator InvincibleCoroutine()
    {

        canUseInvincible = false;
        isInvincible = true;

        currentInvincibleTime =
    invincibleTime;

        // 通常BGM停止
        bgmSource.Stop();

        // 無敵BGM開始
        invincibleBgmSource.Play();


        Debug.Log("無敵開始");

        // プレイヤー半透明
        Image playerImage =
            playerMarker.GetComponent<Image>();

        if (playerImage != null)
        {
            Color c = playerImage.color;
            c.a = 0.5f;
            playerImage.color = c;
        }

        // スキルUI半透明（使用済み感）
        if (skillIcon != null)
        {
            Color c = skillIcon.color;
            c.a = 0.3f;
            skillIcon.color = c;
        }

        yield return new WaitForSeconds(
            invincibleTime
        );

        isInvincible = false;

        // 無敵BGM停止
        invincibleBgmSource.Stop();

        // 通常BGM再開
        bgmSource.Play();

        // プレイヤー元に戻す
        if (playerImage != null)
        {
            Color c = playerImage.color;
            c.a = 1f;
            playerImage.color = c;
        }

        Debug.Log("無敵終了");
    }
    void UpdateSkillGauge()
{
    if (skillGauge == null)
    {
        return;
    }

    // 無敵中
    if (isInvincible)
    {
        currentInvincibleTime -=
            Time.deltaTime;

        skillGauge.fillAmount =
            currentInvincibleTime
            / invincibleTime;
    }
    else
    {
        // 使用済みなら空
        if (!canUseInvincible)
        {
            skillGauge.fillAmount = 0f;
        }
        else
        {
            skillGauge.fillAmount = 1f;
        }
    }
}
    //=====線描画=====
    void HandleDrawInput()
    {
        // UIを押してるなら線を描かない
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (!isStarted)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localMouse;

            Camera cam =
    canvas.renderMode
    == RenderMode.ScreenSpaceOverlay
    ? null
    : canvas.worldCamera;


            RectTransformUtility
.ScreenPointToLocalPointInRectangle(
    canvas.transform as RectTransform,
    Input.mousePosition,
    cam,
    out localMouse
);

            // Scroll分を補正
            localMouse -= scrollTarget.anchoredPosition;

            startDrawPosition = localMouse;

            isDrawing = true;

            previewLine.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0)
            && isDrawing)
        {
            UpdatePreviewLine();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;

            previewLine.gameObject.SetActive(false);

            CreateLine(
                startDrawPosition,
                Input.mousePosition
            );
        }
    }
    void UpdatePreviewLine()
    {
        Vector2 currentPos;

        Camera cam =
    canvas.renderMode
    == RenderMode.ScreenSpaceOverlay
    ? null
    : canvas.worldCamera;

        RectTransformUtility
.ScreenPointToLocalPointInRectangle(
    canvas.transform as RectTransform,
    Input.mousePosition,
    cam,
    out currentPos
);

        currentPos -= scrollTarget.anchoredPosition;

        Vector2 center =
            (startDrawPosition + currentPos) / 2f;

        previewLine.anchoredPosition =
            center;

        float length =
            Mathf.Abs(
                currentPos.x
                - startDrawPosition.x
            );

        previewLine.sizeDelta =
            new Vector2(
                length,
                previewLine.sizeDelta.y
            );
    }
    void CreateLine(
    Vector2 startPos,
    Vector2 endMousePos)
    {
        Vector2 endPos;

        Camera cam =
    canvas.renderMode
    == RenderMode.ScreenSpaceOverlay
    ? null
    : canvas.worldCamera;

        RectTransformUtility
 .ScreenPointToLocalPointInRectangle(
     canvas.transform as RectTransform,
     endMousePos,
     cam,
     out endPos
 );

        endPos -= scrollTarget.anchoredPosition;

        // 少ししか動かしてないなら作らない
        if (Vector2.Distance(startPos, endPos) < 30f)
        {
            return;
        }

        // 縦線開始位置
        float startX =
            -((lineCount - 1)
            * xSpacing) / 2f;

        // 一番近い縦線に吸着
        int lineIndex =
            Mathf.RoundToInt(
                (startPos.x - startX)
                / xSpacing
            );

        lineIndex = Mathf.Clamp(
            lineIndex,
            0,
            lineCount - 1
        );

        // 開始位置を縦線に固定
        startPos.x =
            startX
            + lineIndex * xSpacing;

        // 右方向か判定
        bool drawRight =
            endPos.x > startPos.x;

        // 左右端チェック
        if (!drawRight && lineIndex <= 0)
        {
            return;
        }

        if (drawRight
            && lineIndex >= lineCount - 1)
        {
            return;
        }

        // 行き先X
        float finalX =
            drawRight
            ? startX
                + (lineIndex + 1)
                * xSpacing
            : startX
                + (lineIndex - 1)
                * xSpacing;

        // 線の中心
        Vector2 center =
            new Vector2(
                (startPos.x + finalX)
                / 2f,
                startPos.y
            );

        GameObject line =
            Instantiate(
                horizontalLinePrehub,
                parentUI
            );

        RectTransform rect =
            line.GetComponent
            <RectTransform>();

        rect.anchoredPosition =
            center;

        // 長さ
        rect.sizeDelta =
            new Vector2(
                Mathf.Abs(
                    finalX
                    - startPos.x
                ),
                rect.sizeDelta.y
            );

        spawnedLines.Add(line);
    }
    //=====障害物=====

    void CreateObstacle(
    RectTransform lineRect)
    {
        if (Random.value >
            obstacleChance)
        {
            return;
        }

        GameObject obstacle =
            Instantiate(
                obstaclePrefab,
                parentUI);

        RectTransform rect =
            obstacle.GetComponent
            <RectTransform>();

        float offsetX =
            Random.Range(
                -xSpacing / 3f,
                xSpacing / 3f);

        rect.anchoredPosition =
            lineRect.anchoredPosition
            + new Vector2(offsetX, 0);

        obstacles.Add(rect);

        spawnedLines.Add(obstacle);
    }
    void CheckObstacleCollision()
    {
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            RectTransform obstacle = obstacles[i];

            if (obstacle == null)
            {
                obstacles.RemoveAt(i);
                continue;
            }

            float distance =
                Vector2.Distance(
                    playerMarker.anchoredPosition,
                    obstacle.anchoredPosition);

            if (distance < obstacleHitRadius)
            {
                if (isInvincible)
                {
                    return;
                }

                GameOver();
                return;
            }
        }
    }
    //=====ステージ作成=====
    void CheckGenerateRows()
    {
        int playerRow =
    Mathf.FloorToInt(
        playerMarker
        .anchoredPosition.y
        / ySpacing
    );

        if (generatedRows
            < playerRow
            + bufferRows)
        {
            GenerateRows(5);
        }
    }
    void GenerateRows(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int row = generatedRows;

            horizontalLines[row] =
                new List<int>();

            for (int col = 0;
                 col < lineCount - 1;
                 col++)
            {
                if (Random.value < 0.6f)
                {
                    if (
                        horizontalLines[row]
                        .Count > 0
                        &&
                        horizontalLines[row][
                        horizontalLines[row]
                        .Count - 1]
                        == col - 1)
                    {
                        continue;
                    }

                    horizontalLines[row]
                        .Add(col);

                    GameObject line =
                        Instantiate(
                            horizontalLinePrehub,
                            parentUI
                        );

                    spawnedLines.Add(line);

                    RectTransform rect =
                        line.GetComponent
                        <RectTransform>();

                    rect.sizeDelta =
                        new Vector2(
                            xSpacing,
                            rect.sizeDelta.y
                        );

                    float startX =
                        -((lineCount - 1)
                        * xSpacing)
                        / 2f;

                    rect.anchoredPosition =
                        new Vector2(
                            startX
                            + col * xSpacing
                            + xSpacing / 2f,
                            row * ySpacing
                        );

                    CreateObstacle(rect);
                }
            }

            generatedRows++;
        }
    }

    void CreateVerticalLines()
    {
        float startX =
            -((lineCount - 1) * xSpacing) / 2f;

        for (int i = 0; i < lineCount; i++)
        {
            GameObject line =
                Instantiate(
                    verticalLinePrehub,
                    parentUI
                );

            spawnedLines.Add(line);

            RectTransform rect =
                line.GetComponent<RectTransform>();

            // ←これ超重要
            rect.localScale = Vector3.one;
            rect.localRotation =
                Quaternion.identity;

            rect.anchoredPosition =
                new Vector2(
                    startX + i * xSpacing,
                    7200f
                );

            rect.sizeDelta =
                new Vector2(
                    rect.sizeDelta.x,
                    15000f
                );

            Debug.Log(
                "縦線 " + i +
                " X:" +
                rect.anchoredPosition.x
            );
        }
    }
    //=====UI更新=====
    void UpdateScroll()
    {
        float targetY =
            Mathf.Max(
                0,
                playerMarker.anchoredPosition.y
                + followOffset
            );

        Vector2 targetPos =
            scrollTarget.anchoredPosition;

        targetPos.y =
            Mathf.Lerp(
                scrollTarget.anchoredPosition.y,
                -targetY,
                scrollSpeed
                * Time.deltaTime
            );

        scrollTarget.anchoredPosition =
            targetPos;
    }


    void UpdateScore()
    {
        float distance =
     playerMarker.anchoredPosition.y
     - startY;

        score =
            Mathf.Max(
                score,
                Mathf.FloorToInt(
                    distance / 10f
                )
            );

        scoreText.text =
         "現在 :" + score * 10 + "m";
    }
    void UpdateDifficulty()
    {
        // スピード上昇
        fallSpeed =
     Mathf.Lerp(
         200f,
         450f,
         generatedRows / 300f
     );
        // 障害物率上昇
        obstacleChance =
            Mathf.Clamp(
                0.1f + generatedRows * 0.001f,
                0.1f,
                0.3f
            );
    }

    //=====レイアウト補助=====
    void AdjustSpacing()
    {
        float width =parentUI.rect.width;

        float margin = 100f;

        xSpacing =(width - margin)/ (lineCount - 1);

        ySpacing = 80f;
    }

    void ArrangeButtons()
    {
        float startX =
            -((lineCount - 1)
            * xSpacing) / 2f;

        for (int i = 0;
             i < startButtons.Length;
             i++)
        {
            startButtons[i]
                .GetComponent<RectTransform>()
                .anchoredPosition =
                new Vector2(
                    startX + i * xSpacing,
                    -rowCount * ySpacing
                );
        }
    }
    void UpdateRainbow()
    {
        if (playerImage == null)
        {
            return;
        }

        // 無敵中だけ虹色
        if (isInvincible)
        {
            Color rainbowColor =
                Color.HSVToRGB(
                    Mathf.Repeat(
                        Time.time * 2f,
                        1f
                    ),
                    1f,
                    1f
                );

            rainbowColor.a = 1f;

            playerImage.color =
                rainbowColor;
        }
        else
        {
            // 通常色に戻す
            playerImage.color =
                Color.white;
        }
    }

    // ===== 旧システム（保留）=====
//    void CreateLineInput()
//    {
//        lineTimer += Time.deltaTime;

//        if (Input.GetMouseButton(0) && lineTimer >= lineCreateInterval)
//        {
//            lineTimer = 0f;

//            CreatePlayerLine();
//        }
//    }
//    void DrawLine()
//    {
//        Vector2 mousePos = Input.mousePosition;

//        if (Vector2.Distance(mousePos, lastMousePosition) < drawDistance)
//        {
//            return;
//        }

//        Vector2 localPoint;

//        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentUI, mousePos, null, out localPoint);

//        GameObject line =
//            Instantiate(horizontalLinePrehub, parentUI);

//        RectTransform rect = line.GetComponent<RectTransform>();

//        rect.anchoredPosition = localPoint;

//        rect.sizeDelta = new Vector2(50f, rect.sizeDelta.y);

//        spawnedLines.Add(line);

//        lastMousePosition = mousePos;
//    }
//    void CreateHorizontalLines()
//    {
//        for (int row = 0; row < rowCount; row++)
//        {
//            horizontalLines[row] = new List<int>();

//            for (int col = 0; col < lineCount - 1; col++)
//            {
//                if (Random.value < 0.6f)
//                {
//                    if (horizontalLines[row].Count > 0 && horizontalLines[row][horizontalLines[row].Count - 1] == col - 1)
//                    {
//                        continue;
//                    }
//                    horizontalLines[row].Add(col);

//                    GameObject line = Instantiate(horizontalLinePrehub, parentUI);

//                    spawnedLines.Add(line);

//                    RectTransform rect = line.GetComponent<RectTransform>();

//                    rect.sizeDelta = new Vector2(xSpacing, rect.sizeDelta.y);

//                    float startX = -((lineCount - 1) * xSpacing) / 2f;

//                    rect.anchoredPosition = new Vector2(startX + col * xSpacing + xSpacing / 2f, -row * ySpacing);

//                    CreateObstacle(rect);
//                }
//            }
//        }
//    }
//    public void StartAmida(int startIndex)
//    {
      

//        if (isMoving)
//        {
//            return;
//        }

//        isMoving = true;

//        Debug.Log("押された");
//        StopAllCoroutines();

//        StartCoroutine(MoveAmida(startIndex));
//    }
//    IEnumerator MoveAmida(int startIndex)
//    {
//        int current = startIndex;

//        float startX = -((lineCount - 1) * xSpacing) / 2f;
//        playerMarker.anchoredPosition =
//     startButtons[startIndex]
//     .GetComponent<RectTransform>()
//     .anchoredPosition;

//        for (int row = 0; row < rowCount; row++)
//        {
//            Vector2 downPos = new Vector2(startX + current * xSpacing, -row * ySpacing);

//            yield return MoveTo(downPos);

//            if (current > 0 && horizontalLines[row].Contains(current - 1))
//            {
//                audioSource.PlayOneShot(moveSE);

//                current--;
//                Vector2 leftPos = new Vector2(startX + current * xSpacing, -row * ySpacing);

//                yield return MoveTo(leftPos);
//            }
//            else if (horizontalLines[row].Contains(current))
//            {
//                audioSource.PlayOneShot(moveSE);

//                current++;
//                Vector2 rightPos = new Vector2(startX + current * xSpacing, -row * ySpacing);

//                yield return MoveTo(rightPos);
//            }
//        }
//        resultText.text = "結果:" + rewards[current];
//        audioSource.PlayOneShot(goalSE);
//        isMoving = false;
//    }
//    IEnumerator MoveTo(Vector2 target)
//    {
//        while (Vector2.Distance(playerMarker.anchoredPosition, target) > 1f)
//        {
//            playerMarker.anchoredPosition = Vector2.MoveTowards(playerMarker.anchoredPosition, target, moveSpeed * Time.deltaTime);

//            yield return null;
//        }

//        playerMarker.anchoredPosition = target;

//    }
//    public int GetResult(int startIndex)
//    {
//        int current = startIndex;

//        for (int row = 0; row < rowCount; row++)
//        {
//            if (current > 0 && horizontalLines[row].Contains(current - 1))
//            {
//                current--;

//            }
//            else if (horizontalLines[row].Contains(current))
//            {
//                current++;
//            }
//        }

//        return current;
//    }
}




