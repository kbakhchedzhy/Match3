using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //чтобы отобразились настройки в юнити
public class BoardSetting //класс, через который будем настраивать игровую доску
{
    public int xSize, ySize; //размер поля по х и у (количество тайлов)
    public Tile tileGo; //в него поместим префаб, Tile потому что мы можем в него поместить только те префабы у которых есть даный скрипт как компонент
    public List<Sprite> tileSprite; //хранит список изображений наших тайлов
}

public class GameManager : MonoBehaviour
{
    [Header ("Параметры игровой доски")] //описание настроек в юнити
    public BoardSetting boardSetting;

    void Start() //используя ссылку глоабльного доступа, будем передавать данные из boardSetting
    {
        //первый параметр вызываемые - двумерный массив, дальше все по порядку
        BoardController.instance.SetValue(Board.instance.SetValue(boardSetting.xSize, boardSetting.ySize, boardSetting.tileGo, boardSetting.tileSprite),
            boardSetting.xSize, boardSetting.ySize,
            boardSetting.tileSprite);
    }

    void Update()
    {
        
    }
}
