using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;

public class ManagerCanvas : MonoBehaviour
{
    public Button[] buttons; // 9个棋盘按钮
    public Text gameStatusText;
    public Button restartButton;
    public Button startButton;
    public Button exitButton;
    public Button diffiButton_Easy;
    public Button diffiButton_Medium;
    public Button diffiButton_Hard;
    public Button firstTurnButton;
    public Button gameModeButton;
    public Sprite playerChess;
    public Sprite aiChess;
    public Image line;
    public bool playerFirst = true; // 玩家是否先手
    public bool singlePlay = true; //是否对战AI
    public int gameMode = 0; //难度

    private char[] board; // 棋盘状态：' '为空，'X'为玩家，'O'为AI 'X'为1P 'O'为2P
    private Vector2[] conditions = { //设置划线位置
            new Vector2 { x = 0, y = 160 },
            new Vector2 { x = 0, y = 0 },
            new Vector2 { x = 0, y = -160 },
            new Vector2 { x = -160, y = 0 },
            new Vector2 { x = 0, y = 0 },
            new Vector2 { x = 160, y = 0 },
            new Vector2 { x = 0, y = 0 },
            new Vector2 { x = 0, y = 0 },
    };
    private char playerSymbol = 'X';
    private char aiSymbol = 'O';
    private bool bStart = false;
    private bool isPlayerTurn;
    private bool gameOver;
    private bool canRestart = true;
    private int winCondition;

    void Start()
    {
        Init();
        firstTurnButton.onClick.AddListener(SwitchFirstTurn);
        gameModeButton.onClick.AddListener(SwitchMode);
        restartButton.onClick.AddListener(Restart);
        exitButton.onClick.AddListener(Exit);
        
    }

    void Init()
    {
        //初始化
        board = new char[9];
        for (int i = 0; i < 9; i++)
        {
            board[i] = ' ';
        }
        //重设划线
        ResetLine();
        // 设置初始回合
        if (singlePlay) isPlayerTurn = playerFirst; 
        else isPlayerTurn = true;
        UpdateGameStatus();
        UpdateBoardUI();
        // 是否AI先手
        if (!isPlayerTurn&&bStart&&singlePlay)
        {
            canRestart = false;
            StartCoroutine(AITurn());
        }
    }

    void SwitchFirstTurn() //切换先手
    {
        playerFirst = !playerFirst;
        UpdateBoardUI();
    }

    void SwitchMode() //切换单人双人
    {
        singlePlay = !singlePlay;
        UpdateBoardUI();
    }

    public void StartGame()//开始游戏
    {
        bStart = true;
        gameOver = false;
        //UpdateBoardUI();
        //UpdateGameStatus();
        Init();
    }

    public void SetGameMode(int setGameMode) //设置难度
    {
        if (!bStart)
        {
            gameMode = setGameMode;
            UpdateBoardUI();
        }
    }

    public void OnBoardButtonClick(int index)
    {
        if (singlePlay)
        {
            if (gameOver || !isPlayerTurn || board[index] != ' ')
            {
                return;
            }
        }
        else
        {
            if (gameOver || board[index] != ' ')
            {
                return;
            }
        }
        if(singlePlay){
            // 玩家落子
            MakeMove(index, playerSymbol);
            // 检查游戏是否结束
            if (!gameOver)
            {
                // 切换到AI回合
                isPlayerTurn = false;
                UpdateGameStatus();
                UpdateBoardUI();
                // AI落子
                canRestart = false;
                StartCoroutine(AITurn());
            }
        }
        else
        {
            if (isPlayerTurn)
            {
                MakeMove(index, playerSymbol);
                if (!gameOver)
                {
                    // 切换到2P回合
                    isPlayerTurn = false;
                    UpdateGameStatus();
                    UpdateBoardUI();
                }
            }
            else
            {
                MakeMove(index, aiSymbol);
                if (!gameOver)
                {
                    // 切换到1P回合
                    isPlayerTurn = true;
                    UpdateGameStatus();
                    UpdateBoardUI();
                }
            }
        }
    }

    void MakeMove(int index, char symbol)
    {
        board[index] = symbol;
        UpdateBoardUI();
        CheckGameResult();
    }

    // AI回合
    IEnumerator AITurn()
    {
        // 模拟AI思考
        yield return new WaitForSeconds(0.5f);

        MakeAIMove();

        if (!gameOver)
        {
            isPlayerTurn = true;
            UpdateGameStatus();
            UpdateBoardUI();
        }
    }

    // AI落子
    void MakeAIMove()
    {
        if (gameMode == 2) //困难 AI通过Minimax寻找落子
        {
            int bestMove = FindBestMove();
            MakeMove(bestMove, aiSymbol);
        }
        else if(gameMode == 1) //中等 AI随机落子或Minimax落子
        {
            if (UnityEngine.Random.Range(0f, 1f) > 0.35f)
            {
                int move = FindMove();
                MakeMove(move, aiSymbol);
            }
            else
            {
                int bestMove = FindBestMove();
                MakeMove(bestMove, aiSymbol);
            }
        }
        else //简单 AI随机落子
        {
            int move = FindMove();
            MakeMove(move, aiSymbol);
        }
        canRestart = true;
    }

    int FindMove()
    {
        List<int> Moves = new List<int>();
        for(int i = 0;i< 9; i++)
        {
            if(board[i] == ' ')
            {
                Moves.Add(i);
            }
        }
        if (Moves.Count > 0)
        {
            int randomPos = UnityEngine.Random.Range(0, Moves.Count);
            return Moves[randomPos];
        }
        return -1;
    }

    int FindBestMove()//寻找最好落子位置
    {
        int bestScore = int.MinValue;
        int bestMove = -1;

        for (int i = 0; i < 9; i++)
        {
            if (board[i] == ' ')
            {
                // 尝试在这个位置落子
                board[i] = aiSymbol;
                int score = Minimax(board, 0, false);
                board[i] = ' '; // 撤销落子

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }

        return bestMove;
    }

    // Minimax算法实现
    int Minimax(char[] currentBoard, int depth, bool isMaximizing)
    {
        char result = CheckWinner(currentBoard);//检查胜者
        if (result == aiSymbol) // AI获胜
            return 10 - depth;
        else if (result == playerSymbol) // 玩家获胜
            return depth - 10;
        else if (IsBoardFull(currentBoard)) // 平局
            return 0;

        if (isMaximizing) // AI回合，最大化分数
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (currentBoard[i] == ' ')
                {
                    currentBoard[i] = aiSymbol;
                    int score = Minimax(currentBoard, depth + 1, false);
                    currentBoard[i] = ' ';
                    bestScore = Mathf.Max(score, bestScore);
                }
            }
            return bestScore;
        }
        else // 玩家回合，最小化分数
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (currentBoard[i] == ' ')
                {
                    currentBoard[i] = playerSymbol;
                    int score = Minimax(currentBoard, depth + 1, true);
                    currentBoard[i] = ' ';
                    bestScore = Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }
    }

    char CheckWinner(char[] currentBoard)//检查是否有胜者
    {
        // 所有可能的获胜组合
        int[,] winConditions = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // 横
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // 竖
            {0, 4, 8}, {2, 4, 6}             // 斜
        };

        for (int i = 0; i < 8; i++)
        {
            int a = winConditions[i, 0];
            int b = winConditions[i, 1];
            int c = winConditions[i, 2];

            if (currentBoard[a] != ' ' && currentBoard[a] == currentBoard[b] && currentBoard[a] == currentBoard[c])
            {
                winCondition = i;// 记录获胜情况
                return currentBoard[a];
            }
        }

        return ' '; // 没有获胜者
    }

    bool IsBoardFull(char[] currentBoard) //检查棋盘已满
    {
        for (int i = 0; i < 9; i++)
        {
            if (currentBoard[i] == ' ')
                return false;
        }
        return true;
    }

    void CheckGameResult() //游戏结果
    {
        char winner = CheckWinner(board);
        
        if (winner != ' ')
        {
            bStart = false;
            gameOver = true;
            if(singlePlay)gameStatusText.text = winner == playerSymbol ? "你赢了！" : "AI赢了！";
            else gameStatusText.text = winner == playerSymbol ? "1P赢了！" : "2P赢了！";
            SetLine();
            UpdateBoardUI();
        }
        else if (IsBoardFull(board))
        {
            bStart = false;
            gameOver = true;
            gameStatusText.text = "平局！";
            UpdateBoardUI();
        }
    }

    void SetLine()//设置划线
    {
        Image lineImage = line.GetComponent<Image>();
        lineImage.enabled = true;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchoredPosition = conditions[winCondition];
        if (winCondition == 3 || winCondition == 4||winCondition == 5)
        {
            rect.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        if (winCondition == 6||winCondition==7)
        {
            Vector3 currentScale = rect.localScale;
            rect.localScale = new Vector3(1.5f, currentScale.y, currentScale.z);
            if(winCondition == 6)
            {
                rect.rotation = Quaternion.Euler(0f, 0f, -45f);
            }
            else if(winCondition == 7)
            {
                rect.rotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }
    }

    void ResetLine()//重置划线
    {
        Image lineImage = line.GetComponent<Image>();
        lineImage.enabled = false;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.localScale = new Vector3(1.0f,1.0f,1.0f);
        rect.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void UpdateBoardUI()//刷新棋盘UI
    {
        Text text = firstTurnButton.GetComponentInChildren<Text>();
        if (playerFirst)
        {
            text.text = "玩家先手";
        }
        else
        {
            text.text = "AI先手";
        }
        text = gameModeButton.GetComponentInChildren<Text>();
        if (singlePlay)
        {
            firstTurnButton.interactable = true;
            text.text = "对战AI";
        }
        else
        {
            firstTurnButton.interactable = false;
            text.text = "双人对战";
        }
        if (!bStart)
        {
            gameModeButton.interactable = true;
            startButton.interactable = true;
            restartButton.interactable = false;
            exitButton.interactable = false;
            if (gameMode == 2&&singlePlay){ 
                diffiButton_Hard.interactable = false;
                diffiButton_Medium.interactable = true;
                diffiButton_Easy.interactable = true;
            }
            else if(gameMode == 1&&singlePlay)
            {
                diffiButton_Hard.interactable = true;
                diffiButton_Medium.interactable = false;
                diffiButton_Easy.interactable = true;
            }
            else if(gameMode == 0&&singlePlay)
            {
                diffiButton_Hard.interactable = true;
                diffiButton_Medium.interactable = true;
                diffiButton_Easy.interactable = false;
            }
            else
            {
                diffiButton_Hard.interactable = false;
                diffiButton_Medium.interactable = false;
                diffiButton_Easy.interactable = false;
            }
        }
        else
        {
            restartButton.interactable = true;
            exitButton.interactable = true;
            firstTurnButton.interactable = false;
            gameModeButton.interactable= false;
            diffiButton_Hard.interactable = false;
            diffiButton_Medium.interactable = false;
            diffiButton_Easy.interactable = false;
            startButton.interactable = false;
        }
        for (int i = 0; i < 9; i++)
        {
            Image buttonImage = buttons[i].GetComponent<Image>();
            if (board[i] == 'X')
            {
                buttonImage.color = Color.white;
                buttonImage.sprite = playerChess;
            }
            else if (board[i] == 'O')
            {
                buttonImage.color = Color.white;
                buttonImage.sprite = aiChess;
            }
            else
            {
                buttonImage.color = Color.black;
                buttonImage.sprite = null;
            }

            //Text buttonText = buttons[i].GetComponentInChildren<Text>();
            //buttonText.text = board[i].ToString();
            // 禁用已落子的按钮
            //if(singlePlay)buttons[i].interactable = (board[i] == ' ' && !gameOver && isPlayerTurn&&bStart);
        }
    }

    void UpdateGameStatus()//刷新text文本
    {
        if (!bStart)
        {
            gameStatusText.text = "请开始游戏";
            return;
        }
        if (gameOver)
            return;
        if(singlePlay)gameStatusText.text = isPlayerTurn ? "你的回合" : "AI思考中...";
        else gameStatusText.text = isPlayerTurn ? "1P的回合" : "2P的回合";
    }

    void Restart()//重开 设置canRestart防止在协程模拟AI思考过程中玩家重开或结束
    {
        if(canRestart)StartGame();
    }

    void Exit()//结束
    {
        if (canRestart) {
            gameOver = true;
            bStart = false;
            Init();
        }
    }

    void Update()
    {
        
    }
}
