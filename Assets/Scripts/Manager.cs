using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;

public class ManagerCanvas : MonoBehaviour
{
    public Button[] buttons; // 9�����̰�ť
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
    public bool playerFirst = true; // ����Ƿ�����
    public bool singlePlay = true; //�Ƿ��սAI
    public int gameMode = 0; //�Ѷ�

    private char[] board; // ����״̬��' 'Ϊ�գ�'X'Ϊ��ң�'O'ΪAI 'X'Ϊ1P 'O'Ϊ2P
    private Vector2[] conditions = { //���û���λ��
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
        //��ʼ��
        board = new char[9];
        for (int i = 0; i < 9; i++)
        {
            board[i] = ' ';
        }
        //���軮��
        ResetLine();
        // ���ó�ʼ�غ�
        if (singlePlay) isPlayerTurn = playerFirst; 
        else isPlayerTurn = true;
        UpdateGameStatus();
        UpdateBoardUI();
        // �Ƿ�AI����
        if (!isPlayerTurn&&bStart&&singlePlay)
        {
            canRestart = false;
            StartCoroutine(AITurn());
        }
    }

    void SwitchFirstTurn() //�л�����
    {
        playerFirst = !playerFirst;
        UpdateBoardUI();
    }

    void SwitchMode() //�л�����˫��
    {
        singlePlay = !singlePlay;
        UpdateBoardUI();
    }

    public void StartGame()//��ʼ��Ϸ
    {
        bStart = true;
        gameOver = false;
        //UpdateBoardUI();
        //UpdateGameStatus();
        Init();
    }

    public void SetGameMode(int setGameMode) //�����Ѷ�
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
            // �������
            MakeMove(index, playerSymbol);
            // �����Ϸ�Ƿ����
            if (!gameOver)
            {
                // �л���AI�غ�
                isPlayerTurn = false;
                UpdateGameStatus();
                UpdateBoardUI();
                // AI����
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
                    // �л���2P�غ�
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
                    // �л���1P�غ�
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

    // AI�غ�
    IEnumerator AITurn()
    {
        // ģ��AI˼��
        yield return new WaitForSeconds(0.5f);

        MakeAIMove();

        if (!gameOver)
        {
            isPlayerTurn = true;
            UpdateGameStatus();
            UpdateBoardUI();
        }
    }

    // AI����
    void MakeAIMove()
    {
        if (gameMode == 2) //���� AIͨ��MinimaxѰ������
        {
            int bestMove = FindBestMove();
            MakeMove(bestMove, aiSymbol);
        }
        else if(gameMode == 1) //�е� AI������ӻ�Minimax����
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
        else //�� AI�������
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

    int FindBestMove()//Ѱ���������λ��
    {
        int bestScore = int.MinValue;
        int bestMove = -1;

        for (int i = 0; i < 9; i++)
        {
            if (board[i] == ' ')
            {
                // ���������λ������
                board[i] = aiSymbol;
                int score = Minimax(board, 0, false);
                board[i] = ' '; // ��������

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }

        return bestMove;
    }

    // Minimax�㷨ʵ��
    int Minimax(char[] currentBoard, int depth, bool isMaximizing)
    {
        char result = CheckWinner(currentBoard);//���ʤ��
        if (result == aiSymbol) // AI��ʤ
            return 10 - depth;
        else if (result == playerSymbol) // ��һ�ʤ
            return depth - 10;
        else if (IsBoardFull(currentBoard)) // ƽ��
            return 0;

        if (isMaximizing) // AI�غϣ���󻯷���
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
        else // ��һغϣ���С������
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

    char CheckWinner(char[] currentBoard)//����Ƿ���ʤ��
    {
        // ���п��ܵĻ�ʤ���
        int[,] winConditions = new int[,]
        {
            {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // ��
            {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // ��
            {0, 4, 8}, {2, 4, 6}             // б
        };

        for (int i = 0; i < 8; i++)
        {
            int a = winConditions[i, 0];
            int b = winConditions[i, 1];
            int c = winConditions[i, 2];

            if (currentBoard[a] != ' ' && currentBoard[a] == currentBoard[b] && currentBoard[a] == currentBoard[c])
            {
                winCondition = i;// ��¼��ʤ���
                return currentBoard[a];
            }
        }

        return ' '; // û�л�ʤ��
    }

    bool IsBoardFull(char[] currentBoard) //�����������
    {
        for (int i = 0; i < 9; i++)
        {
            if (currentBoard[i] == ' ')
                return false;
        }
        return true;
    }

    void CheckGameResult() //��Ϸ���
    {
        char winner = CheckWinner(board);
        
        if (winner != ' ')
        {
            bStart = false;
            gameOver = true;
            if(singlePlay)gameStatusText.text = winner == playerSymbol ? "��Ӯ�ˣ�" : "AIӮ�ˣ�";
            else gameStatusText.text = winner == playerSymbol ? "1PӮ�ˣ�" : "2PӮ�ˣ�";
            SetLine();
            UpdateBoardUI();
        }
        else if (IsBoardFull(board))
        {
            bStart = false;
            gameOver = true;
            gameStatusText.text = "ƽ�֣�";
            UpdateBoardUI();
        }
    }

    void SetLine()//���û���
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

    void ResetLine()//���û���
    {
        Image lineImage = line.GetComponent<Image>();
        lineImage.enabled = false;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.localScale = new Vector3(1.0f,1.0f,1.0f);
        rect.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void UpdateBoardUI()//ˢ������UI
    {
        Text text = firstTurnButton.GetComponentInChildren<Text>();
        if (playerFirst)
        {
            text.text = "�������";
        }
        else
        {
            text.text = "AI����";
        }
        text = gameModeButton.GetComponentInChildren<Text>();
        if (singlePlay)
        {
            firstTurnButton.interactable = true;
            text.text = "��սAI";
        }
        else
        {
            firstTurnButton.interactable = false;
            text.text = "˫�˶�ս";
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
            // ���������ӵİ�ť
            //if(singlePlay)buttons[i].interactable = (board[i] == ' ' && !gameOver && isPlayerTurn&&bStart);
        }
    }

    void UpdateGameStatus()//ˢ��text�ı�
    {
        if (!bStart)
        {
            gameStatusText.text = "�뿪ʼ��Ϸ";
            return;
        }
        if (gameOver)
            return;
        if(singlePlay)gameStatusText.text = isPlayerTurn ? "��Ļغ�" : "AI˼����...";
        else gameStatusText.text = isPlayerTurn ? "1P�Ļغ�" : "2P�Ļغ�";
    }

    void Restart()//�ؿ� ����canRestart��ֹ��Э��ģ��AI˼������������ؿ������
    {
        if(canRestart)StartGame();
    }

    void Exit()//����
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
