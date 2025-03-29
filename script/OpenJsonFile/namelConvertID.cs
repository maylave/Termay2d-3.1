using System.Collections.Generic;
using UnityEngine;

public static class NameConverter
{
    public static ItemManager _saveList;


   
    // Поиск блока по имени в contenerBlock
    public static ushort FindBlockIndexByName(string name)
    {
        for (ushort i = 0; i < _saveList.contenerBlock.Count; i++)
        {
          
            if (_saveList.contenerBlock[i] != null &&
                _saveList.contenerBlock[i].name == name)
            {
                return i;
            }
        }
        return 0;
    }

    // Поиск элемента по имени в общем контейнере
    public static ushort FindItemIndexByName(string name, ItemType expectedType = ItemType.Block)
    {
        for (ushort i = 0; i < _saveList.contener.Count; i++)
        {
            var item = _saveList.contener[i];
            if (item != null &&
                item.name == name &&
                item.itemType == expectedType)
            {
                return i;
            }
        }
        return 0;
    }

    // Получение типа элемента по ID блока
    public static ItemType GetItemTypeByBlockId(ushort blockId)
    {
        if (blockId >= _saveList.contenerBlock.Count ||
            _saveList.contenerBlock[blockId] == null)
            return ItemType.Item;

        string vidName = _saveList.contenerBlock[blockId].VIDName;

        foreach (var item in _saveList.contener)
        {
            if (item != null && item.name == vidName)
            {
                return item.itemType;
            }
        }

        return ItemType.Item;
    }

    // Конвертация ID блока с проверкой типа
    public static ushort ConvertBlockId(ushort originalId)
    {
        if (originalId >= _saveList.contenerBlock.Count ||
            string.IsNullOrEmpty(_saveList.contenerBlock[originalId]?.name))
            return originalId;

        var itemType = GetItemTypeByBlockId(originalId);
        string vidName = _saveList.contenerBlock[originalId].VIDName;

        if (itemType != ItemType.Block)
        {
            // Поиск в общем контейнере для неблочных типов
            for (ushort i = 0; i < _saveList.contener.Count; i++)
            {
                if (_saveList.contener[i] != null &&
                    _saveList.contener[i].name == vidName)
                {
                    return i;
                }
            }
        }
        else
        {
            // Поиск среди блоков
            for (ushort i = 0; i < _saveList.contenerBlock.Count; i++)
            {
                if (_saveList.contenerBlock[i] != null &&
                    _saveList.contenerBlock[i].name == vidName)
                {
                    return i;
                }
            }
        }

        return originalId;
    }
}