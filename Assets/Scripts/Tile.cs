using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // поле, через которое будет менятся изображение спрайтов
    public bool isSelected; //поле, через которое будет определятся выбран даный тайл или нет
    public bool isEmpty //есть ли изображение у даного тайла
    {
        get
        {
            return spriteRenderer.sprite == null ? true : false; //если у спрайта даного тайла нет изображения, то мы возвращаем тру, иначе фолз. т.е. когда выстроем 3 одинаковых в ряд, у них не будет спрайта.
        }
    }
}
