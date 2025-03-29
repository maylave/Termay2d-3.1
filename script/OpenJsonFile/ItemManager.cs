using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
public enum ItemType
{
    Sword,
    Instrument,
    Block,
    Item,
    Armor
}

public enum InstrumentType
{
    Pickaxe,
    Shovel,
    Axe,
    Hoe,
    Hammer,
    Shears,
    Chainsaw,
    OmniWrench
}

[System.Serializable]
public struct ListItem
{
    public string name;
    public bool isMenu;
    public ItemType itemType;

    public ListItem(string name, ItemType itemType, bool isMenu)
    {
        this.name = name;
        this.itemType = itemType;
        this.isMenu = isMenu;
    }
}

[System.Serializable]
public abstract class Item
{
    public string name;
    public int maxStack;
    public ItemType itemType;
    public string imagePath;

    protected Item(string name, int maxStack, ItemType itemType, string imagePath)
    {
        this.name = name;
        this.maxStack = maxStack;
        this.itemType = itemType;
        this.imagePath = imagePath;
    }
}

[System.Serializable]
public class Sword : Item
{
    public int damage;

    public Sword(string name, int maxStack, string imagePath, int damage)
        : base(name, maxStack, ItemType.Sword, imagePath)
    {
        this.damage = damage;
    }
}

[System.Serializable]
public class GameItem : Item
{
    public GameItem(string name, int maxStack, string imagePath)
        : base(name, maxStack, ItemType.Item, imagePath)
    {
    }
}

[System.Serializable]
public class Instrument : Item
{
    public int durability;
    public float hardness;
    public int level;
    public InstrumentType instrumentType;

    public Instrument(string name, int maxStack, string imagePath,
                     int durability, float hardness, int level, InstrumentType instrumentType)
        : base(name, maxStack, ItemType.Instrument, imagePath)
    {
        this.durability = durability;
        this.hardness = hardness;
        this.level = level;
        this.instrumentType = instrumentType;
    }
}

[System.Serializable]
public class Block : Item
{
    public string VIDName;
    public byte hardness;
    public float breakTime;
    public InstrumentType requiredTool;

    public Block(string name, int maxStack, string imagePath,
               string vidName, byte hardness, float breakTime,
               InstrumentType requiredTool)
        : base(name, maxStack, ItemType.Block, imagePath)
    {
        this.VIDName = vidName;
        this.hardness = hardness;
        this.breakTime = breakTime;
        this.requiredTool = requiredTool;
    }
}
[Serializable]
public class BlockData
{
    public string name;
    public int maxStack;
    public string imagePath;
    public string VIDName;
    public byte hardness;
    public float breakTime;
    public InstrumentType requiredTool;
}


public class ItemManager : MonoBehaviour
{

    [SerializeField] private ItemType selectedType;
    [SerializeField] private ListImage listImage;
    [SerializeField] private List<TextAsset> jsonConfigs;
    public List<BlockData> blocks;
    private List<ListItem> itemsData = new List<ListItem>();
    public readonly List<Item> contener = new List<Item>();
    public readonly List<Block> contenerBlock = new List<Block>();
    public IL IL;
    public geneshnmaps geneshnmaps;
    private void Start()
    {
        InitializeDirectories();
        LoadMainList(); // Загружаем основной список
        LoadItems();
       
    }

    private void InitializeDirectories()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "config");
        string imagePath = Path.Combine(configPath, "image");

        if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
        if (!Directory.Exists(imagePath)) Directory.CreateDirectory(imagePath);
    }

    private void LoadMainList()
    {
        string listPath = Path.Combine(Application.persistentDataPath, "config", "listId.json");
        Debug.Log(listPath);
        if (File.Exists(listPath))
        {
            string json = File.ReadAllText(listPath);
            itemsData = JsonConvert.DeserializeObject<List<ListItem>>(json);
            
        }
        else
        {
            Debug.LogError("Main list file not found!");
        }
    }

    public void LoadItems()
    {
        StartCoroutine(LoadAllItems());
    }
   
    private IEnumerator LoadAllItems()
    {
        if (itemsData.Count == 0)
        {
            Debug.LogError("No items data loaded!");
            yield break;
        }
       
        foreach (var itemData in itemsData)
        {
            string configPath = Path.Combine(Application.persistentDataPath, "config", "Items",itemData.name)+ ".json";
            string config = File.ReadAllText(configPath);
            
            Debug.Log(config);
            if (config == null)
            {
                Debug.LogWarning($"Config not found for: {itemData.name}");
                continue;
            }

            Item item = null;
            
            switch (itemData.itemType)
            {
                case ItemType.Block:
                    
                    
                        item = JsonConvert.DeserializeObject<Block>(config);
                        if (item != null)
                        {
                            
                            contenerBlock.Add((Block)item);
                        Block block = (Block)item;
                        Debug.Log($"Добавлен блок: {block.name}, Hardness: {block.hardness}, Tool: {block.requiredTool}");

                        IL.LoadImages(item.name);
                        }
                    
                    
                    
                    break;

                case ItemType.Sword:
                    
                    
                        item = JsonConvert.DeserializeObject<Sword>(config);
                    
                   
                    break;

                case ItemType.Item:
                    
                    
                        item = JsonConvert.DeserializeObject<GameItem>(config);
                    IL.LoadImages(item.name);



                    break;
            }

            if (item != null)
            {
                contener.Add(item);
            }

            yield return new WaitForSeconds(0.1f);
        }

        FilterBlocks();
    }

    private IEnumerator LoadItemImage(string imagePath)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "config", "image", Path.GetFileName(imagePath))+".png";

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Image file not found: {fullPath}");
            yield break;
        }

        byte[] imageData = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.zero
        );

        listImage.AddToImageList(sprite); // Добавляем спрайт в ListImage
    }

    public void FilterBlocks()
    {
        contenerBlock.Clear();
        contenerBlock.Add(null); // Пустой слот

        foreach (var item in contener)
        {
            if (item is Block block)
            {
                contenerBlock.Add(block);
                Debug.Log($"Block added: {block.name}");
            }
        }
    }
}

