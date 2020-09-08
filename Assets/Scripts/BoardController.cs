
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Security.Principal;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance; //ссылка глобалльного доступа, настраиваем ее в Awake

    private int xSize, ySize;  //эти два поля нужны для инициализации массива
    private List<Sprite> tileSprite = new List<Sprite>(); //помещаем сюда все изображения для тайлов
    private Tile[,] tileArray; //двумерный массив, в этот массив мы будем передавать из класса Board массив, который заполняется из создания доски

    private Tile oldSelectTile; //поле, которое будет сохранять ссылку выбраного тайла
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; //массив, отвечающий за направление лучей. Мы будем стрелять вверх,вниз,влево,вправо.

    private bool isFindMatch = false;
    private bool isShift = false; //не будем давать пользователю играть, пока выполянется код смещения
    private bool isSearchEmptyTile = false; //разрешаем или запрещаем выполнения метода в update

    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite) //метод, для принятия данных
    {
        this.tileArray = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
    }

    private void Awake() 
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(isSearchEmptyTile) 
        {
            SearchEmptyTile();
        }
        //для того чтобы нажатие на тайл работал:

        if (Input.GetMouseButtonDown(0)) //если была нажата левая кнопка мышки
        {
            //тогда создаем переменую, которая будет содержать информацию о объекте, в который попадет мышка (луч)
            //Input.mousePosition - точка, где проиходило нажатие
            //Camera.main.ScreenPointToRay - запускает луч туда, где было нажатие
            //GetRayIntersection - возвращает информацию о попадание в какой-то объект, для этого мы настраивали Box Collaider 2D
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition)); //настраиваем луч
            if (ray!=false) //если попали в какой-то объект 
            {
                CheckSelectTile(ray.collider.gameObject.GetComponent<Tile>()); 
            }
        }
    }
    #region(Выделить тайл, Снять выделение с тайла, Управление выделением)


    private void SelectTile(Tile tile) //метод выделения тайла, private чтобы другие классы не могли взаимодействовать 
    {
        tile.isSelected = true; //запоминаем, что данный тайл выбран
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); //меняем цвет, чтобы выдеть, что он выделен
        oldSelectTile = tile; //помещаем ссылку на выделеный тайл
    }


    private void DeselectTile(Tile tile) //метод снятия выделения
    {
        tile.isSelected = false; //запоминаем, что тайл не выбран
        tile.spriteRenderer.color = new Color(1, 1, 1); //возвращаем старый цвет
        oldSelectTile = null; //удаляем ссылку на выделеный тайл
    }


    private void CheckSelectTile(Tile tile) //метод, который отвечает логику выбора тайла
    {
        if (tile.isEmpty || isShift) //если у тайла нет изображения или когда происходит смещение тайлов
        {
            return; //тогда ничего не делаем
        }
        if (tile.isSelected) //если тайл выбран
        {
            DeselectTile(tile); //снимаем с него выделение
        }
        else
        {
            //первое выделение тайла
            if (!tile.isSelected&& oldSelectTile == null) //если тайл не выбран
            {
                SelectTile(tile); //применяем метод выделения
            }

            //попытка выбрать другой тайл
            else
            {
                //если второй выбранный тайл сосед предыдущего тайла
                //Contains проверяет находится ли tile в списке соседних тайлов AdjacentTiles()
                if (AdjacentTiles(tile).Contains(tile))
                {
                    SwapTwoTiles(tile); //вызываем метод, меняющий местами
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile); //снимаем выделение с старого тайла
                }
                //новое выделение, забываем старый тайл
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);
                }
            }
        }
    }


    #endregion
    #region(Поиск совпадения, удаление спрайтов, Поиск всех совпадений)

    //метод поиска совпадения по вертикали и горизонатали. Если он обнаружит совпадения то он вернет список этих файлов
    //Tile tile - начало поиска
    //Vector2 dir - конец поиска, двухмерный вектор отвечаеющий за луч
    private List<Tile> FindMatch(Tile tile, Vector2 dir) 
    {
        List<Tile> cashFindTiles = new List<Tile>(); //список, в котором будет помещатся тайл, при обнаружения совпадения
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir); //информация о тайле, в который попадет луч
        while(hit.collider != null  //пока мы попадаем в какие-то тайлы, и изображения тайлов совпадает с тайлом из которого идет выстрел луча, тогда:
            && hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite) //цикл поиска ВСЕХ совпадений
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>()); //при обнаружении заносим в список совпадения
            hit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir); //запускаем новый луч, с тайла в который мы попали по совпаднию, а не с центрального тайла)
        }
        return cashFindTiles;
    }


    //метод, отвечающий за удаление изображения у выбранных тайлов
    //Tile tile - начало это выбранный тайл
    //Vector2[] dirArray - сначала по горизонтали, потом по вертикали 
    private void DeleteSprite(Tile tile, Vector2[] dirArray) {
        List<Tile> cashFindSprite = new List<Tile>(); //список, в который будем помещать найденные тайлы

        for (int i=0; i<dirArray.Length; i++) //2 операции в цикле, влево-вправо, вверх-вниз
        {
            cashFindSprite.AddRange(FindMatch(tile, dirArray[i])); //помещаем результат поиска (список)
        }
        if (cashFindSprite.Count >= 2) //если больше двух элементов, значит это 3 в ряд. т.к. начальный тайл не учитываем
        {
            for (int i=0; i < cashFindSprite.Count; i++) //проходим по всем элементам, которые есть в списке
            {
                cashFindSprite[i].spriteRenderer.sprite = null; //сбрасываем ссылку изображений
            }
            isFindMatch = true; //нашли совпадания
        }
    }


    private void FindAllMatch(Tile tile) //метод, за поиск ВСЕХ совпадений, помимо тех с которыми мы работали
    {
        if(tile.isEmpty) //проверяем, если у тайла уже нет изображения, то мы ничего не делаем
        {
            return;
        }
        //tile - начальная позиция
        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down}); //запускаем метод удаления спрайта, сначала вверх-вниз
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right}); //влево-вправо
        if (isFindMatch) //если совпадения есть
        {
            isFindMatch = false; //сбрасываем счетчик
            tile.spriteRenderer.sprite = null; //сбрасываем изображения
            isSearchEmptyTile = true; //разрешаем поиск пустых тайлов
        }
    }


    #endregion
    #region(Смена 2х тайлов, Соседние тайлы)


    private void SwapTwoTiles(Tile tile) //смена тайлов местами
    {
        if (oldSelectTile.spriteRenderer.sprite==tile.spriteRenderer.sprite) //если тайлы одинаковые, то ничего не делаем
        {
            return; //заканчиваем работу с классом
        }
        Sprite cashSprite = oldSelectTile.spriteRenderer.sprite; //в этой переменной сохраняем спрайт, предыдущего выделеного тайла
        oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite; //меняем спрайт на новый выбранный тайл
        tile.spriteRenderer.sprite = cashSprite; //в новый тайл, помещаем старый спрайт

        UI.instance.Moves(1);
    }


    //List<Tile> Он будет возвращать
    private List<Tile> AdjacentTiles(Tile tile) //метод, который будет возращать список тайлов по соседству
    {
        List<Tile> cashTiles = new List<Tile>(); //список, принимающий тайлы
        for (int i = 0; i<dirRay.Length; i++) 
        {
            //Raycast() - создаем луч
            //oldSelectTile.transform.position - начальная точка, откуда будет стрелять луч
            //dirRay[i] - куда будет стрелять массив
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]); 
            if (hit.collider!=null) //если во что-то попали
            {
                cashTiles.Add(hit.collider.gameObject.GetComponent<Tile>()); //помещаем информацию об объекте, в который врезались
            }
        }
        return cashTiles; 
    }
    #endregion
    #region(Поиск пустого тайла, Сдвиг тайла вниз, Установить новое изображение, Выбрать новое изображение)


    private void SearchEmptyTile() //метод, который ищет пустой тайл
    {
        for(int x = 0; x < xSize; x++) //пробегаемся по массиву тайла
        {
            for(int y = 0; y < ySize; y++)
            {
                if (tileArray[x, y].isEmpty)
                {
                    ShiftTileDown(x, y);
                    break;
                }
                if (x == xSize && y == ySize)
                {
                    isSearchEmptyTile = false;
                }
            }
        }

        for (int x = 0; x < xSize; x++) //пробегаемся по массиву тайла
        {
            for (int y = 0; y < ySize; y++)
            {
                FindAllMatch(tileArray[x, y]); //вызываем поиск всех похожих комбинаци  
            }
        }
    }


    private void ShiftTileDown(int xPos, int yPos) //метод, отвечающий за заполнение пустых тайлов (сдвиг тайлов)
    {
        isShift = true; //смещение началось
        //список, через который будем менять изображения у тайлов
        List<SpriteRenderer> cashRenderer = new List<SpriteRenderer>();
        int count = 0;
        for (int y = yPos; y < ySize; y++) //начиная с позиции тайла и до конца оси
        {
            Tile tile = tileArray[xPos, y]; //содержит ссылку на выбраный тайл из массива
            if (tile.isEmpty) //если тайл пустой
            {
                count++; //считает количество пустых тайлов
                
            }
            //тогда заносим его ссылку на его в spriteRen. список
            cashRenderer.Add(tile.spriteRenderer);
        }
        for (int i=0; i< count; i++)
        {
            UI.instance.Score(50);
            SetNewSprite(xPos, cashRenderer); //лишнее?
        }

        isShift = false; //смещение закончилось
    }


    //установка новых изображений у пустых тайлов
    private void SetNewSprite(int xPos, List<SpriteRenderer> renderer) 
    {
        for(int y = 0; y < renderer.Count - 1; y++) //цикл по всем элементам
        {
            //передаем изображение, которое находится сверху
            renderer[y].sprite = renderer[y + 1].sprite; 
            //верхнему даем случайное изображение
            renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1); //y-1 чтобы мы не выходили за размер списка
        }

    }


    //возвращает новое изображение с учетом остальных тайлов(избегает совпадения)
    private Sprite GetNewSprite(int xPos, int yPos) 
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite); //заполняем список спрайтов списком изображений, который получен из GameManager

        if (xPos > 0) //если пустой тайл, не находится в первом столбце
        {
            cashSprite.Remove(tileArray[xPos - 1, yPos].spriteRenderer.sprite); //можем удалить изоб. у соседнего тайла слева
        }
        if (xPos < xSize - 1) //если пустой тайл, находится не в последнем столбце
        {
            cashSprite.Remove(tileArray[xPos + 1, yPos].spriteRenderer.sprite); //можем удалить изображения справа
        }
        if (yPos > 0) //если пустой тайл, не в первой строке
        {
            cashSprite.Remove(tileArray[xPos, yPos - 1].spriteRenderer.sprite); //удаляем изоб. снизу
        }
        return cashSprite[Random.Range(0, cashSprite.Count)]; //возвращаем случайное изоб. из этого списка
    }


    #endregion

}

