using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class game : MonoBehaviour
{
    
    public int score = 0;
    public int speed = 1;
    public int mnoj_score = 0;
    public GameObject TextScore;
    public GameObject TextSpeed;
    public GameObject TextCof;

    public GameObject block;

    GameObject[,] all_blocks;
    int[,] playing_field = new int[18, 9];

    GameObject[,] all_blocks_wall;

    GameObject[,] Next_figur;
    int[,] next_figur_xy = new int[4, 4];

    //Номер текущей фигуры
    int figur_number = 1;

    //Номер следующей фигуры
    int new_figure_global;

    //Кординаты левого верхневого угла для вращения фигуры
    int Xcor, Ycor;

    Color block_color = Color.blue;
    Color helper_block_color = Color.blue;

    /*int[,] playing_field = new int[,] {
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,1,0,0,0},
        {0,0,0,0,1,1,1,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0},
        {0,0,0,2,2,2,2,2,2},
        {2,2,2,0,0,0,0,0,0},
    };
    */

    bool speedup = true;
    
    //Коофециент умножения скорости падения элементов
    float cof = 0;

    void Start()
    {
        Fill_Field();
    }


    void Update()
    {
        New_figur();

        Draw_Field();
        
        if (speedup == true)
        {
            InvokeRepeating("Move_Down", 0.003f, 0.5f - cof);
            speedup = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            CancelInvoke();
            InvokeRepeating("Move_Down", 0.003f, 0.04f);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            CancelInvoke();
            speedup = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            InvokeRepeating("Move_Left", 0.003f, 0.1f);
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            CancelInvoke();
            speedup = true;
        }


        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            InvokeRepeating("Move_Right", 0.003f, 0.1f);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            CancelInvoke();
            speedup = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Figur_Rotation();
        }

        if (Input.GetKeyUp(KeyCode.Equals))
        {
            //Увеличение коофециента скорости;
            cof = cof + 0.01f;
            speed++;
            CancelInvoke();
            speedup = true;
        }

        if (Input.GetKeyUp(KeyCode.Minus))
        {
            if (speed > 0)
            {
                //Увеличение коофециента скорости;
                cof = cof + 0.01f;
                speed--;
                CancelInvoke();
                speedup = true;
            }
        }


        Check_Delet();

        TextScore.GetComponent<Text>().text = "Score: " + score.ToString();
        TextSpeed.GetComponent<Text>().text = "Speed: " + speed.ToString();

    }


    private void Fill_Field()
    {
        //Игровое поле
        all_blocks = new GameObject[18, 9];

        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                all_blocks[y, x] = GameObject.Instantiate(block);
                all_blocks[y, x].transform.position = new Vector3(x, 17 - y, 0);
            }
        }

        //Границы

        all_blocks_wall = new GameObject[20, 11];

        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 11; x++)
            {
                all_blocks_wall[y, x] = GameObject.Instantiate(block);
                all_blocks_wall[y, x].transform.position = new Vector3(x - 1, 18 - y, 0);

                if (x == 0 || x == 10 || y == 19)
                {
                    all_blocks_wall[y, x].SetActive(true);
                }
                else
                    all_blocks_wall[y, x].SetActive(false);
            }
        }

        //Следующая фигура
        Next_figur = new GameObject[4, 4];

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Next_figur[y, x] = GameObject.Instantiate(block);
                Next_figur[y, x].transform.position = new Vector3(15 + x, 16 - y, 0);
            }
        }
    }

    private void Draw_Field()
    {
        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] > 0)
                {
                    all_blocks[y, x].SetActive(true);
                }
                else
                {
                    all_blocks[y, x].SetActive(false);
                }

                if (playing_field[y, x] == 1)
                {
                    all_blocks[y, x].GetComponent<Renderer>().material.color = block_color;
                }
            }
        }
    }

    //Отрисовка следующей фигуры
    private void Draw_helper()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (next_figur_xy[y, x] == 1)
                {
                    Next_figur[y, x].SetActive(true);
                }
                else
                {
                    Next_figur[y, x].SetActive(false);
                }

                if (next_figur_xy[y, x] == 1)
                {
                    Next_figur[y, x].GetComponent<Renderer>().material.color = helper_block_color;
                }
            }
        }
    }

    private void Move_Down()
    {
        bool coll = false;

        //Если фигуры нет
        bool fig = true;
        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] == 1)
                    fig = false;
            }
        }

        if (fig == true)
            return;

        //Проверка столкновения
        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] == 1)
                {
                    if (y == 17)
                    {
                        coll = true;
                        break;
                    }
                    else
                    {
                        if (playing_field[y + 1, x] == 2)
                        {
                            coll = true;
                            break;
                        }
                    }
                }
            }

            if (coll == true)
                break;
        }

        //Шаг
        if (coll == true)
        {
            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (playing_field[y, x] == 1)
                        playing_field[y, x] = 2;
                }
            }
        }
        else
        {
            int[,] xy_move = new int[2, 4];

            int id_x = 0;

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (playing_field[y, x] == 1 && y < 17)
                    {
                        playing_field[y, x] = 0;
                        xy_move[0, id_x] = y + 1;
                        xy_move[1, id_x] = x;

                        id_x++;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playing_field[xy_move[0, i], xy_move[1, i]] = 1;
            }
        }

        if (Ycor < 17)
            Ycor++;
    }

    private void Move_Left()
    {
        bool coll = false;

        //Проверка столкновения
        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] == 1)
                {
                    if (x == 0)
                    {
                        coll = true;
                        break;
                    }
                    else
                    {
                        if (playing_field[y, x - 1] == 2)
                        {
                            coll = true;
                            break;
                        }
                    }
                }
            }

            if (coll == true)
                break;
        }

        //Движение
        if (coll == false)
        {
            int[,] xy_move = new int[2, 4];

            int id_x = 0;

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (playing_field[y, x] == 1)
                    {
                        playing_field[y, x] = 0;
                        xy_move[0, id_x] = y;
                        xy_move[1, id_x] = x - 1;


                        id_x++;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playing_field[xy_move[0, i], xy_move[1, i]] = 1;
            }
        }

        if (Xcor > 0)
            Xcor--;
    }

    private void Move_Right()
    {
        bool coll = false;

        //Проверка столкновения
        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] == 1)
                {
                    if (x == 8)
                    {
                        coll = true;
                        break;
                    }
                    else
                    {
                        if (playing_field[y, x + 1] == 2)
                        {
                            coll = true;
                            break;
                        }
                    }
                }
            }

            if (coll == true)
                break;
        }

        if (coll == false)
        {
            int[,] xy_move = new int[2, 4];

            int id_x = 0;

            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (playing_field[y, x] == 1)
                    {
                        playing_field[y, x] = 0;
                        xy_move[0, id_x] = y;
                        xy_move[1, id_x] = x + 1;

                        id_x++;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                playing_field[xy_move[0, i], xy_move[1, i]] = 1;
            }
        }

        if (Xcor < 6)
            Xcor++;
    }

    //Проверка отсутствия движущихся фигур и создания новой
    private void New_figur()
    {

        //Проверка на наличия фигур вверху (Если есть, рестарт игры)
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (playing_field[y, x] == 2)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        bool not_anFigure = true;

        for (int y = 0; y < 18; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] == 1)
                {
                    not_anFigure = false;
                    break;
                }
            }
            if (not_anFigure == false)
                break;
        }

        if (not_anFigure == true)
        {
            int start_x = 4;

            figur_number = new_figure_global;

            switch (figur_number)
            {
                //Квадрат
                case 1:
                    playing_field[0, start_x] = 1;
                    playing_field[0, start_x + 1] = 1;
                    playing_field[1, start_x] = 1;
                    playing_field[1, start_x + 1] = 1;

                    block_color = Color.red;

                    break;
                //Палка
                case 2:
                    playing_field[0, start_x] = 1;
                    playing_field[1, start_x] = 1;
                    playing_field[2, start_x] = 1;
                    playing_field[3, start_x] = 1;

                    block_color = Color.green;

                    Xcor = start_x - 2;
                    Ycor = 0;

                    break;
                //Г влево
                case 3:
                    playing_field[0, start_x - 1] = 1;
                    playing_field[0, start_x] = 1;
                    playing_field[1, start_x] = 1;
                    playing_field[2, start_x] = 1;

                    Xcor = start_x - 1;
                    Ycor = 0;

                    block_color = Color.magenta;

                    break;
                //Г вправо
                case 4:
                    playing_field[0, start_x + 1] = 1;
                    playing_field[0, start_x] = 1;
                    playing_field[1, start_x] = 1;
                    playing_field[2, start_x] = 1;

                    Xcor = start_x - 1;
                    Ycor = 0;

                    block_color = Color.magenta;

                    break;
                //z вправо
                case 5:
                    playing_field[0, start_x] = 1;
                    playing_field[0, start_x + 1] = 1;
                    playing_field[1, start_x + 1] = 1;
                    playing_field[1, start_x + 2] = 1;

                    Xcor = start_x;
                    Ycor = 0;

                    block_color = Color.grey;

                    break;
                //z влево
                case 6:
                    playing_field[0, start_x] = 1;
                    playing_field[0, start_x - 1] = 1;
                    playing_field[1, start_x - 1] = 1;
                    playing_field[1, start_x - 2] = 1;

                    Xcor = start_x - 2;
                    Ycor = 0;

                    block_color = Color.grey;

                    break;
                //Хз
                case 7:
                    playing_field[0, start_x] = 1;
                    playing_field[1, start_x] = 1;
                    playing_field[1, start_x - 1] = 1;
                    playing_field[1, start_x + 1] = 1;

                    Xcor = start_x - 1;
                    Ycor = 0;

                    block_color = Color.blue;

                    break;

            }

            Next_figure();
        }

    }

    //Выбор следующей фигуры
    private void Next_figure()
    {
        new_figure_global = Random.Range(0, 8);

        Clear_field();
        Draw_helper();

        switch (new_figure_global)
        {
            //Квадрат
            case 1:
                next_figur_xy[1, 1] = 1;
                next_figur_xy[1, 2] = 1;
                next_figur_xy[2, 1] = 1;
                next_figur_xy[2, 2] = 1;

                helper_block_color = Color.red;

                break;
            //Палка
            case 2:
                next_figur_xy[0, 2] = 1;
                next_figur_xy[1, 2] = 1;
                next_figur_xy[2, 2] = 1;
                next_figur_xy[3, 2] = 1;

                helper_block_color = Color.green;

                break;
            //Г влево
            case 3:
                next_figur_xy[0, 1] = 1;
                next_figur_xy[0, 2] = 1;
                next_figur_xy[1, 2] = 1;
                next_figur_xy[2, 2] = 1;

                helper_block_color = Color.magenta;

                break;
            //Г вправо
            case 4:
                next_figur_xy[0, 2] = 1;
                next_figur_xy[0, 1] = 1;
                next_figur_xy[1, 1] = 1;
                next_figur_xy[2, 1] = 1;

                helper_block_color = Color.magenta;

                break;
            //z вправо
            case 5:
                next_figur_xy[0, 1] = 1;
                next_figur_xy[0, 2] = 1;
                next_figur_xy[1, 2] = 1;
                next_figur_xy[1, 3] = 1;

                helper_block_color = Color.grey;

                break;
            //z влево
            case 6:
                next_figur_xy[1, 0] = 1;
                next_figur_xy[1, 1] = 1;
                next_figur_xy[0, 1] = 1;
                next_figur_xy[0, 2] = 1;

                helper_block_color = Color.grey;

                break;
            //Хз
            case 7:
                next_figur_xy[0, 1] = 1;
                next_figur_xy[1, 0] = 1;
                next_figur_xy[1, 1] = 1;
                next_figur_xy[1, 2] = 1;

                helper_block_color = Color.blue;

                break;
        }

        Draw_helper();

    }

    //Очистка поля подсказки
    private void Clear_field()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                next_figur_xy[x, y] = 0;
            }
        }

        Draw_helper();
    }

    //Очистка совпашшей строки
    private void Check_Delet()
    {
        mnoj_score = 0;

        //Очистка совпавших полос
        bool check_sting = false;

        for (int y = 17; y > -1; y--)
        {
            check_sting = true;

            for (int x = 0; x < 9; x++)
            {
                if (playing_field[y, x] != 2)
                {
                    check_sting = false;
                }
            }

            if (check_sting == true)
            {
                for (int x = 0; x < 9; x++)
                {
                    playing_field[y, x] = 0;
                }

                for (int yY = y - 1; yY > 1; yY--)
                {
                    for (int xX = 0; xX < 9; xX++)
                    {
                        if (playing_field[yY, xX] == 2)
                        {
                            playing_field[yY + 1, xX] = 2;
                            playing_field[yY, xX] = 0;
                        }
                    }
                }

                y++;
                mnoj_score++;

            }
        }


        if (mnoj_score > 0)
        {

            //Увеличение коофециента скорости;
            cof = cof + 0.01f * mnoj_score;
            speed = speed + mnoj_score;
            CancelInvoke();
            speedup = true;

            //Очки
            score = score + mnoj_score * mnoj_score * 100;

            TextCof.GetComponent<Text>().text = "Cof: " + mnoj_score.ToString();
        }

    }

    //Поворот фигуры
    private void Figur_Rotation()
    {
        int[,] rotation_cord = new int[3, 3];
        int[,] rotation_cord_pal = new int[4, 4];

        switch (figur_number)
        {
            //Куб
            case 1:
                break;
            //Палка
            case 2:
                //кастыль
                int xX = Xcor;

                if (Ycor == 15)
                    return;

                for (int y = Ycor; y < Ycor + 4; y++)
                {
                    if (Xcor == 6)
                        xX = Xcor - 1;

                    for (int x = xX; x < xX + 4; x++)
                    {
                        rotation_cord_pal[y - Ycor, x - xX] = playing_field[y, x];
                        if (playing_field[y, x] == 2)
                        {
                            return;
                        }
                    }
                }

                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        playing_field[y + Ycor, x + xX] = rotation_cord_pal[3 - x, y];
                    }
                }

                break;
            //Остальные
            default:
                if (Ycor == 16)
                    return;

                for (int y = Ycor; y < Ycor + 3; y++)
                {
                    for (int x = Xcor; x < Xcor + 3; x++)
                    {
                        rotation_cord[y - Ycor, x - Xcor] = playing_field[y, x];
                        if (playing_field[y, x] == 2)
                        {
                            return;
                        }
                    }
                }

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        playing_field[y + Ycor, x + Xcor] = rotation_cord[2-x, y];
                    }
                }

                break;

        }
    }
    
}
