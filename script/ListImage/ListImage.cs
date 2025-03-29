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

    // ������ �������� ��� �������� �������������
    

    public IReadOnlyList<Sprite> Images => imageL.AsReadOnly();

    public void AddToImageList(Sprite sprite) // ������� ����� �� ������ ������
    {
        if (sprite == null)
        {
            Debug.LogWarning("Attempted to add null sprite to ImageList");
            return;
        }

        imageL.Add(sprite);
    }

    // ��������� ������ �������� ��� ���������
    public void RemoveImage(Sprite sprite) => imageL.Remove(sprite);
    public void ClearImageList() => imageL.Clear();
    public bool Contains(Sprite sprite) => imageL.Contains(sprite);
}