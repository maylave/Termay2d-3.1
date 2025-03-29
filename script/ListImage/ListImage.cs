using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(
    fileName = "New Image List",
    menuName = "Inventory/Image List",
    order = 0)]
public class ListImage : ScriptableObject
{
    public List<Sprite> imageL = new List<Sprite>();

    // Старое название для обратной совместимости
    

    public IReadOnlyList<Sprite> Images => imageL.AsReadOnly();

    public void AddToImageList(Sprite sprite) // Добавил метод со старым именем
    {
        if (sprite == null)
        {
            Debug.LogWarning("Attempted to add null sprite to ImageList");
            return;
        }

        imageL.Add(sprite);
    }

    // Остальные методы остаются без изменений
    public void RemoveImage(Sprite sprite) => imageL.Remove(sprite);
    public void ClearImageList() => imageL.Clear();
    public bool Contains(Sprite sprite) => imageL.Contains(sprite);
}