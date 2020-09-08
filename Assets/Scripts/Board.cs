using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance; //ссылка для глобального доступа, чтобы передавать данные из GameManager. Т.е. через instance будем взаимодействовать с этим классом
    //приватные, потому что эти классы будут получать данные из GameManager
    private int xSize, ySize;
    private Tile tileGO;
    private List<Sprite> tileSprite = new List<Sprite>(); //иницилизируем 

    private void Awake() //настраиваем ссылку для глобального доступа
    {
        instance = this;
    }


    public Tile[,] SetValue(int xSize, int ySize, Tile tileGO, List<Sprite> tileSprite) //метод, через который будем получать данные из GameManager
    {
        this.xSize = xSize; //this говорить что xSize принадлежит именно этому классу
        this.ySize = ySize;
        this.tileGO = tileGO;
        this.tileSprite = tileSprite;

        return CreateBoard(); //возвращает двумерный массив
    }

    /*
   
    Он дабы не повторялись спрайты, расставляет их в шахмонтном порядке!
    2 подрят у него не попадаются, а это плохо..

    */

    private Tile[,] CreateBoard() //метод, который создает игровую доску(тайлы) и размещать их на экране
    {
        Tile[,] tileArray = new Tile[xSize, ySize]; //иницилизация двумерного массива
        //получаем позицию на котором висит скрип
        float xPosition = transform.position.x; 
        float yPosition = transform.position.y;
        //Vector2 - двумерный массив
        //двумерный вектор, в котором помещен значение размера тайлов по х и у, это нужно для смещения, чтобы тайлы друг на друге не размещались
        Vector2 tileSize = tileGO.spriteRenderer.bounds.size; 

        Sprite cashSprite = null; //переменная, в которой хранится изображение, помещенное в тайл


        //цикл для заполнения тайлов
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //transsform.posititon означает что тайл будет оставаться на месте Board и без вращения?
                Tile newTile = Instantiate(tileGO, transform.position, Quaternion.identity); //переменная, в которой ссылка на тайл, созданого объекта.
                //настраивает позицию данного тайла, т.е. каждая новая плитка будет смещаться на новую позицию
                newTile.transform.position = new Vector3(xPosition + (tileSize.x * x), yPosition + (tileSize.y * y), 0); 
                newTile.transform.parent = transform; //тайлы, которые создаются - станут дочерними объектами Board

                tileArray[x, y] = newTile; //помещаем созданный тайл newTile в массив
                //новый список, в котором доступные изображени, переданные через GameManager. Т.е. из даного списка будем рандомно выбирать изображение для тайла и потом удалять его, чтобы не было повторения
                List<Sprite> tempSprite = new List<Sprite>(); 
                tempSprite.AddRange(tileSprite); //помещаем список спрайтов, который получили из GameManager, доступный для тайла
                tempSprite.Remove(cashSprite); //удалим из списка спрайт, который находится в переменной cashSprite
                if (x > 0) //если это не самый левый столбец
                {
                    tempSprite.Remove(tileArray[x - 1, y].spriteRenderer.sprite); //дополнительно из списка удаляем изображение, которое находится слева.
                }
                //созданный тайл через поле sprite  и помещаем в него случайный спрайт из списка, от 0 и до количество спрайтов в этом списке
                newTile.spriteRenderer.sprite = tempSprite[Random.Range(0, tempSprite.Count)]; //помещаем случайный тайл из списка
                cashSprite = newTile.spriteRenderer.sprite;  //после того как мы положили изображение, мы его запоминаем

            }
        }
        return tileArray; //возвращает массив, для доступа классу BoardController
    }
}
