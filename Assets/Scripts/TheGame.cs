using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TheGame : MonoBehaviour
{
    const int maxFieldHight = 18, maxFieldWidth = 9;

    const int maxFieldWallHight = 20, maxFieldWallWidth = 11;

    public int score = 0;
    public int speed = 1;
    public int factorScore = 0;
    public Text TextScore;
    public Text TextSpeed;
    public Text TextFactor;

    public GameObject Block;

    GameObject[,] AllBlocks;
    int[,] playingField = new int[maxFieldHight, maxFieldWidth];

    GameObject[,] NextFigur;
    int[,] nextFigurxy = new int[4, 4];

    //Номер текущей фигуры
    int figurNumber = 1;

    //Номер следующей фигуры
    int newFigurGlobal;

    //Координаты левого верхнего угла для вращения фигуры
    int xCoor, yCoor;

    Color BlockColor = Color.blue;
    Color helperBlockColor = Color.blue;

    bool speedUp = true;

    bool left;

    //Коэффициент умножения скорости падения элементов
    float factor = 0;

    void Start()
    {
        FillField();

        TextScore.text = "Счёт: " + score.ToString();
        TextSpeed.text = "Скорость: " + speed.ToString();
        TextFactor.text = "Кол-во линий: " + factorScore.ToString();
    }
    
    void Update()
    {
        Newfigur();

        DrawField();

        if (speedUp)
        {
            InvokeRepeating("MoveDown", 0.003f, 0.5f - factor);
            speedUp = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            CancelInvoke();
            InvokeRepeating("MoveDown", 0.003f, 0.04f);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            CancelInvoke();
            speedUp = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            left = true;
            InvokeRepeating("MoveRightLeft", 0.003f, 0.1f);
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            CancelInvoke();
            speedUp = true;
        }


        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            left = false;
            InvokeRepeating("MoveRightLeft", 0.003f, 0.1f);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            CancelInvoke();
            speedUp = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FigurRotation();
        }

        if (Input.GetKeyUp(KeyCode.Equals))
        {
            //Увеличение коэффициента скорости;
            factor = factor + 0.01f;
            speed++;
            CancelInvoke();
            speedUp = true;
        }

        if (Input.GetKeyUp(KeyCode.Minus))
        {
            if (speed > 0)
            {
                //Уменьшение коэффициента скорости;
                factor = factor + 0.01f;
                speed--;
                CancelInvoke();
                speedUp = true;
            }

            if (speed == 0)
                speed = 1;
        }


        CheckDelet();

        TextScore.text = "Счёт: " + score.ToString();
        TextSpeed.text = "Скорость: " + speed.ToString();

    }


    private void FillField()
    {
        //Игровое поле
        AllBlocks = new GameObject[maxFieldHight, maxFieldWidth];

        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                AllBlocks[y, x] = GameObject.Instantiate(Block);
                AllBlocks[y, x].transform.position = new Vector3(x, maxFieldHight - 1 - y, 0);
            }
        }

        //Границы

        GameObject[,] AllBlocksWall = new GameObject[maxFieldWallHight, maxFieldWallWidth];

        for (int y = 0; y < maxFieldWallHight; y++)
        {
            for (int x = 0; x < maxFieldWallWidth; x++)
            {
                AllBlocksWall[y, x] = GameObject.Instantiate(Block);
                AllBlocksWall[y, x].transform.position = new Vector3(x - 1, maxFieldHight - y, 0);

                if (x == 0 || x == maxFieldWidth + 1 || y == maxFieldHight + 1)
                {
                    AllBlocksWall[y, x].SetActive(true);
                }
                else
                    AllBlocksWall[y, x].SetActive(false);
            }
        }

        //Следующая фигура
        NextFigur = new GameObject[4, 4];

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                NextFigur[y, x] = GameObject.Instantiate(Block);
                NextFigur[y, x].transform.position = new Vector3(15 + x, 16 - y, 0);
            }
        }
    }

    private void DrawField()
    {
        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] > 0)
                {
                    AllBlocks[y, x].SetActive(true);
                }
                else
                {
                    AllBlocks[y, x].SetActive(false);
                }

                if (playingField[y, x] == 1)
                {
                    AllBlocks[y, x].GetComponent<Renderer>().material.color = BlockColor;
                }
            }
        }
    }

    //Отрисовка следующей фигуры
    private void Drawhelper()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (nextFigurxy[y, x] == 1)
                {
                    NextFigur[y, x].SetActive(true);
                }
                else
                {
                    NextFigur[y, x].SetActive(false);
                }

                if (nextFigurxy[y, x] == 1)
                {
                    NextFigur[y, x].GetComponent<Renderer>().material.color = helperBlockColor;
                }
            }
        }
    }

    //Смещение фигуры на 1 клетку вниз
    private void MoveDown()
    {
        bool coll = false;

        //Если фигуры нет
        bool figureExists = true;
        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] == 1)
                    figureExists = false;
            }
        }

        if (figureExists)
            return;

        //Проверка столкновения
        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] == 1)
                {
                    if (y == maxFieldHight - 1)
                    {
                        coll = true;
                        break;
                    }
                    else
                    {
                        if (playingField[y + 1, x] == 2)
                        {
                            coll = true;
                            break;
                        }
                    }
                }
            }

            if (coll)
                break;
        }

        //Шаг
        if (coll)
        {
            for (int y = 0; y < maxFieldHight; y++)
            {
                for (int x = 0; x < maxFieldWidth; x++)
                {
                    if (playingField[y, x] == 1)
                        playingField[y, x] = 2;
                }
            }
        }
        else
        {
            int[,] xyMove = new int[2, 4];

            int idx = 0;

            for (int y = 0; y < maxFieldHight; y++)
            {
                for (int x = 0; x < maxFieldWidth; x++)
                {
                    if (playingField[y, x] == 1 && y < maxFieldHight - 1)
                    {
                        playingField[y, x] = 0;
                        xyMove[0, idx] = y + 1;
                        xyMove[1, idx] = x;

                        idx++;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playingField[xyMove[0, i], xyMove[1, i]] = 1;
            }
        }

        if (yCoor < maxFieldHight - 1)
            yCoor++;
    }

    //Движение фигуры в стороны
    private void MoveRightLeft()
    {
        bool coll = false;
        int xCollision;
        int rotation;

        //Лево, право?
        if (left)
        {
            xCollision = 0;
            rotation = -1;

            if (xCoor > 0)
                xCoor--;
        }
        else
        {
            xCollision = maxFieldWidth - 1;
            rotation = 1;

            if (xCoor < 6)
                xCoor++;
        }

        //Проверка столкновения
        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] == 1)
                {
                    if (x == xCollision)
                    {
                        coll = true;
                        break;
                    }
                    else
                    {
                        if (playingField[y, x + rotation] == 2)
                        {
                            coll = true;
                            break;
                        }
                    }
                }
            }

            if (coll)
                break;
        }

        //Шаг
        if (!coll)
        {
            int[,] xyMove = new int[2, 4];

            int idx = 0;

            for (int y = 0; y < maxFieldHight; y++)
            {
                for (int x = 0; x < maxFieldWidth; x++)
                {
                    if (playingField[y, x] == 1)
                    {
                        playingField[y, x] = 0;
                        xyMove[0, idx] = y;
                        xyMove[1, idx] = x + rotation;

                        idx++;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playingField[xyMove[0, i], xyMove[1, i]] = 1;
            }
        }
    }

    //Проверка отсутствия движущихся фигур и создания новой
    private void Newfigur()
    {

        //Проверка на наличия фигур вверху (Если есть, рестарт игры)
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (playingField[y, x] == 2)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        bool figureNotExists = true;

        for (int y = 0; y < maxFieldHight; y++)
        {
            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] == 1)
                {
                    figureNotExists = false;
                    break;
                }
            }
            if (!figureNotExists)
                break;
        }

        if (figureNotExists)
        {
            int startX = 4;

            figurNumber = newFigurGlobal;

            switch (figurNumber)
            {
                //Квадрат
                case 1:
                    playingField[0, startX] = 1;
                    playingField[0, startX + 1] = 1;
                    playingField[1, startX] = 1;
                    playingField[1, startX + 1] = 1;

                    BlockColor = Color.red;

                    break;
                //Палка
                case 2:
                    playingField[0, startX] = 1;
                    playingField[1, startX] = 1;
                    playingField[2, startX] = 1;
                    playingField[3, startX] = 1;

                    BlockColor = Color.green;

                    xCoor = startX - 2;
                    yCoor = 0;

                    break;
                //Г влево
                case 3:
                    playingField[0, startX - 1] = 1;
                    playingField[0, startX] = 1;
                    playingField[1, startX] = 1;
                    playingField[2, startX] = 1;

                    xCoor = startX - 1;
                    yCoor = 0;

                    BlockColor = Color.magenta;

                    break;
                //Г вправо
                case 4:
                    playingField[0, startX + 1] = 1;
                    playingField[0, startX] = 1;
                    playingField[1, startX] = 1;
                    playingField[2, startX] = 1;

                    xCoor = startX - 1;
                    yCoor = 0;

                    BlockColor = Color.magenta;

                    break;
                //z вправо
                case 5:
                    playingField[0, startX] = 1;
                    playingField[0, startX + 1] = 1;
                    playingField[1, startX + 1] = 1;
                    playingField[1, startX + 2] = 1;

                    xCoor = startX;
                    yCoor = 0;

                    BlockColor = Color.grey;

                    break;
                //z влево
                case 6:
                    playingField[0, startX] = 1;
                    playingField[0, startX - 1] = 1;
                    playingField[1, startX - 1] = 1;
                    playingField[1, startX - 2] = 1;

                    xCoor = startX - 2;
                    yCoor = 0;

                    BlockColor = Color.grey;

                    break;
                //Т - образная фигура
                case 7:
                    playingField[0, startX] = 1;
                    playingField[1, startX] = 1;
                    playingField[1, startX - 1] = 1;
                    playingField[1, startX + 1] = 1;

                    xCoor = startX - 1;
                    yCoor = 0;

                    BlockColor = Color.blue;

                    break;

            }

            NextFigure();
        }

    }

    //Выбор следующей фигуры
    private void NextFigure()
    {
        newFigurGlobal = Random.Range(0, 8);

        Clearfield();
        Drawhelper();

        switch (newFigurGlobal)
        {
            //Квадрат
            case 1:
                nextFigurxy[1, 1] = 1;
                nextFigurxy[1, 2] = 1;
                nextFigurxy[2, 1] = 1;
                nextFigurxy[2, 2] = 1;

                helperBlockColor = Color.red;

                break;
            //Палка
            case 2:
                nextFigurxy[0, 2] = 1;
                nextFigurxy[1, 2] = 1;
                nextFigurxy[2, 2] = 1;
                nextFigurxy[3, 2] = 1;

                helperBlockColor = Color.green;

                break;
            //Г влево
            case 3:
                nextFigurxy[0, 1] = 1;
                nextFigurxy[0, 2] = 1;
                nextFigurxy[1, 2] = 1;
                nextFigurxy[2, 2] = 1;

                helperBlockColor = Color.magenta;

                break;
            //Г вправо
            case 4:
                nextFigurxy[0, 2] = 1;
                nextFigurxy[0, 1] = 1;
                nextFigurxy[1, 1] = 1;
                nextFigurxy[2, 1] = 1;

                helperBlockColor = Color.magenta;

                break;
            //z вправо
            case 5:
                nextFigurxy[0, 1] = 1;
                nextFigurxy[0, 2] = 1;
                nextFigurxy[1, 2] = 1;
                nextFigurxy[1, 3] = 1;

                helperBlockColor = Color.grey;

                break;
            //z влево
            case 6:
                nextFigurxy[1, 0] = 1;
                nextFigurxy[1, 1] = 1;
                nextFigurxy[0, 1] = 1;
                nextFigurxy[0, 2] = 1;

                helperBlockColor = Color.grey;

                break;
            //Т - образная фигура
            case 7:
                nextFigurxy[0, 1] = 1;
                nextFigurxy[1, 0] = 1;
                nextFigurxy[1, 1] = 1;
                nextFigurxy[1, 2] = 1;

                helperBlockColor = Color.blue;

                break;
        }

        Drawhelper();

    }

    //Очистка поля подсказки
    private void Clearfield()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                nextFigurxy[x, y] = 0;
            }
        }

        Drawhelper();
    }

    //Очистка совпавших строк и отпускание остальных фигур
    private void CheckDelet()
    {
      factorScore = 0;

        //Очистка совпавших полос
        bool checkSting = false;

        for (int y = maxFieldHight - 1; y > -1; y--)
        {
            checkSting = true;

            for (int x = 0; x < maxFieldWidth; x++)
            {
                if (playingField[y, x] != 2)
                {
                    checkSting = false;
                }
            }

            if (checkSting)
            {
                for (int x = 0; x < maxFieldWidth; x++)
                {
                    playingField[y, x] = 0;
                }

                for (int yY = y - 1; yY > 1; yY--)
                {
                    for (int xX = 0; xX < maxFieldWidth; xX++)
                    {
                        if (playingField[yY, xX] == 2)
                        {
                            playingField[yY + 1, xX] = 2;
                            playingField[yY, xX] = 0;
                        }
                    }
                }

                y++;
                factorScore++;

            }
        }


        if (factorScore > 0)
        {

            //Увеличение коэффициента скорости;
            factor = factor + 0.01f * factorScore;
            speed = speed + factorScore;
            CancelInvoke();
            speedUp = true;

            //Очки
            score = score + factorScore * factorScore * 100;

            TextFactor.text = "Кол-во линий: " + factorScore.ToString();
        }

    }

    //Поворот фигуры
    private void FigurRotation()
    {
        int[,] rotationcord = new int[3, 3];
        int[,] rotationcordpal = new int[4, 4];

        switch (figurNumber)
        {
            //Куб
            case 1:
                break;
            //Палка
            case 2:
                //кастыль
                int xX = xCoor;

                if (yCoor == 15)
                    return;

                for (int y = yCoor; y < yCoor + 4; y++)
                {
                    if (xCoor == 6)
                        xX = xCoor - 1;

                    for (int x = xX; x < xX + 4; x++)
                    {
                        rotationcordpal[y - yCoor, x - xX] = playingField[y, x];
                        if (playingField[y, x] == 2)
                        {
                            return;
                        }
                    }
                }

                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        playingField[y + yCoor, x + xX] = rotationcordpal[3 - x, y];
                    }
                }

                break;

            //Остальные фигуры
            default:
                if (yCoor == 16)
                    return;

                for (int y = yCoor; y < yCoor + 3; y++)
                {
                    for (int x = xCoor; x < xCoor + 3; x++)
                    {
                        rotationcord[y - yCoor, x - xCoor] = playingField[y, x];
                        if (playingField[y, x] == 2)
                        {
                            return;
                        }
                    }
                }

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        playingField[y + yCoor, x + xCoor] = rotationcord[2 - x, y];
                    }
                }

                break;

        }
    }
}
